using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Humanizer;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Items;
using Kesmai.Server.Network;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Game;

public partial class StockEntry
{
	public Func<ItemEntity> Create { get; set; }
		
	public Action<PlayerEntity, ItemEntity> Purchased { get; set;  }
		
	public int Cost { get; set; }
		
	public int CurrentQuantity { get; set; }
	public int MaximumQuantity { get; set; }
		
	public string Name { get; set; }

	public bool IsEmpty => (CurrentQuantity <= 0);
	public bool IsFull => (CurrentQuantity >= MaximumQuantity);
		
	public bool AllowRestock { get; set; }
		
	public StockEntry(string name, Func<ItemEntity> create, int cost, int maximumQuantity, Action<PlayerEntity, ItemEntity> purchased, bool allowRestock = true)
	{
		Name = name;
		
		Cost = cost;

		CurrentQuantity = 0;
		MaximumQuantity = maximumQuantity;
			
		Create = create;
		Purchased = purchased;

		AllowRestock = allowRestock;
	}
		
	/// <summary>
	/// Determines whether this instance responds to the specified noun.
	/// </summary>
	public bool RespondsTo(string noun)
	{
		var name = Name;

		return name.Length >= noun.Length
		       && name.Substring(0, noun.Length).Matches(noun, true);
	}
}
	
public partial class Shopkeeper : VendorEntity
{
	private static Regex _buy = new Regex(@"^buy\s(\w.*?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	private static Regex _appraise = new Regex(@"^appraise\s(\w.*?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	private static Regex _showPrices = new Regex(@"^show\sprices$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

	private List<StockEntry> _stock = new List<StockEntry>();
	private Timer _restockTimer;

	[CommandProperty(AccessLevel.GameMaster)]
	public virtual TimeSpan RestockDelay { get; set; } = TimeSpan.FromHours(1.0);

	[CommandProperty(AccessLevel.GameMaster)]
	public Timer RestockTimer => _restockTimer;

	public Shopkeeper()
	{
	}

	protected override void OnDelete()
	{
		base.OnDelete();

		if (_restockTimer != null)
			_restockTimer.Stop();

		_restockTimer = null;
	}
	
	public override void GetInteractions(PlayerEntity source, List<InteractionEntry> entries)
	{
		entries.Add(BuyItemInteraction.Instance);
		entries.Add(BuyAllInteraction.Instance);
		entries.Add(AppraiseInteraction.Instance);

		if (_stock.Any(s => !s.IsEmpty))
		{
			entries.Add(ShowPricesInteraction.Instance);

			var sell = new InteractionSubmenu("Sell");

			foreach (var stock in _stock.Where(s => !s.IsEmpty))
				sell.Add(new SellStockInteraction(stock));

			entries.Add(sell);
		}

		entries.Add(InteractionSeparator.Instance);

		base.GetInteractions(source, entries);
	}

	public override void HandleOrder(OrderEventArgs args)
	{
		base.HandleOrder(args);

		if (args.Handled)
			return;

		var source = args.Source;
		var order = args.Order;

		/* example: buy all*/
		/* example: buy (item) */
		if (_buy.TryGetMatch(order, out var buyMatch))
		{
			args.Handled = true;

			if (AtCounter(source, out var counter))
				BuyItems(source, counter, buyMatch.Groups[1].Value);
			else
				this.SayNotAtCounter(source);
		}
		/* example: appraise gem */
		else if (_appraise.TryGetMatch(order, out var appraiseMatch))
		{
			args.Handled = true;

			if (AtCounter(source, out var counter))
			{
				var item = Segment.GetItemAt(counter, appraiseMatch.Groups[1].Value, (i) => i is Gold);

				if (item != null)
					Appraise(source, item);
				else
					SayTo(source, 6300263); /* Please put the item on the counter. */
			}
			else
			{
				this.SayNotAtCounter(source);
			}
		}
		/* example: show prices */
		else if (_showPrices.TryGetMatch(order, out _))
		{
			args.Handled = true;

			if (AtCounter(source, out _))
				ShowPrices(source);
			else
				this.SayNotAtCounter(source);
		}
		/* example: sell scales */
		else if (_sell.TryGetMatch(order, out var sellMatch))
		{
			args.Handled = true;

			if (AtCounter(source, out var counter))
			{
				/* Try to find the stock by name if index does not resolve. */
				if (!int.TryParse(sellMatch.Groups[1].Value, out var index))
				{
					var stockName = sellMatch.Groups[1].Value;
					var matchedStock = _stock.FirstOrDefault(st => st.RespondsTo(stockName));

					if (matchedStock != null)
						index = _stock.IndexOf(matchedStock) + 1;
				}

				var stock = default(StockEntry);

				if (index > 0 && index <= _stock.Count)
					stock = _stock[index - 1];

				SellStock(source, counter, stock);
			}
			else
			{
				this.SayNotAtCounter(source);
			}
		}
	}

	/// <summary>
	/// Buys the items on the counter that match the name, and pays for them in gold on the counter.
	/// </summary>
	private void BuyItems(PlayerEntity source, Point2D counter, string name)
	{
		var segment = Segment;

		var items = segment.GetItemsAt(counter, name, (i) => i is Gold).ToList();
		var totalValue = (uint)items.Sum(i => i.ActualPrice * i.Amount);

		if (totalValue > 0)
		{
			foreach (var item in items)
				item.Delete();

			source.PlaySound(10007);

			var gold = new Gold
			{
				Amount = totalValue,
			};

			gold.Move(counter, true, segment);

			SayTo(source, 6300350); /* Thank you for your business. */
		}
		else
		{
			SayTo(source, 6300264); /* There is nothing of value here. */
		}
	}

	/// <summary>
	/// Identifies the item and tells the player what it is worth.
	/// </summary>
	private void Appraise(PlayerEntity source, ItemEntity item)
	{
		item.Identified = true;

		var entries = new List<LocalizationEntry>();

		item.GetDescriptionPrefix(entries);
		item.GetDescription(entries);
		item.GetDescriptionSuffix(entries);

		var grammar = Name.IsPlural() ? "are" : "is";
		var actualPrice = item.ActualPrice;

		if (actualPrice > 0)
			entries.Add(new LocalizationEntry(6300261, item.Name, grammar, actualPrice.ToString()));
		else
			entries.Add(new LocalizationEntry(6300262, item.Name, grammar));

		SayTo(source, entries.ToArray());
	}

	/// <summary>
	/// Lists the stock that isn't empty, numbered by its place in the stock list.
	/// </summary>
	private void ShowPrices(PlayerEntity source)
	{
		/* We Iterate all the items in the stock to keep index values consistent with list. */
		if (_stock.Any(s => !s.IsEmpty))
		{
			SayTo(source, 6300382); /* I will sell to you .. */

			for (int i = 0; i < _stock.Count; i++)
			{
				var stock = _stock[i];

				/* Do not list empty entries. */
				if (!stock.IsEmpty)
					SayTo(source, 6300384, stock.Name.WithArticle(), stock.Cost.ToString(), (i + 1).ToString());
			}
		}
		else
		{
			SayTo(source, 6300381); /* I don't have anything of interest. */

			if (_stock.Any(s => !s.IsFull) && (_restockTimer is null || !_restockTimer.Running))
				Log.Warn($"[{Segment.Name}] Shopkeeper '{Name}' at {Location} not restocking.");
		}
	}

	/// <summary>
	/// Sells one of the stock to the player for the gold on the counter.
	/// </summary>
	private void SellStock(PlayerEntity source, Point2D counter, StockEntry stock)
	{
		if (stock is null || !_stock.Contains(stock) || stock.IsEmpty)
		{
			SayTo(source, 6300243); /* Are you trying to be funny? */
			return;
		}

		if (this.CountOnCounter<Gold>(counter) is 0)
		{
			this.SayPutCoins(source);
			return;
		}

		if (!this.ConsumeFromCounter<Gold>(counter, (uint)stock.Cost))
		{
			SayTo(source, 6300243); /* Are you trying to be funny? */
			return;
		}

		var item = Sell(stock, source);

		if (item != null)
		{
			source.PlaySound(10008);

			if (!item.Deleted)
				item.Move(counter, true, Segment);
				
			SayTo(source, 6300350); /* Thank you for your business. */
		}
	}

	private void QueueRestockTimer()
	{
		if (_restockTimer != null && _restockTimer.Running)
			_restockTimer.Stop();

		if (_stock.Any(s => !s.IsFull))
			_restockTimer = Timer.DelayCall(RestockDelay, Restock);
	}

	public ItemEntity Sell(StockEntry stock, PlayerEntity player)
	{
		var item = stock.Create();

		if (item != null)
		{
			if (stock.Purchased != null)
				stock.Purchased(player, item);
				
			OnSell(player, item);
		}

		stock.CurrentQuantity--;

		QueueRestockTimer();
			
		return item;
	}
		
	public virtual void OnSell(PlayerEntity player, ItemEntity item)
	{
	}

	public void Stock<T>(string name, int basePrice, int quantity = 1) where T : ItemEntity, new()
	{
		Stock(name, () => new T(), basePrice, default, quantity);
	}

	public void Stock(string name, Func<ItemEntity> create, int basePrice, Action<PlayerEntity, ItemEntity> purchased = default, int quantity = 1)
	{
		Stock(new StockEntry(name, create, basePrice, quantity, purchased));
	}

	public void Stock(StockEntry entry)
	{
		_stock.Add(entry);
	}

	public void RestockAll(bool maximum = false)
	{
		foreach (var stock in _stock)
		{
			Restock(stock);

			if (!maximum) 
				continue;
				
			while (!stock.IsFull)
				Restock(stock);
		}
	}

	public void Restock()
	{
		var stocks = _stock.Where(s => !s.IsFull).ToList();
			
		if (!stocks.Any())
			return;
			
		var stock = stocks.Random();

		if (stock != null)
			Restock(stock);
	}

	public void Restock(StockEntry stock)
	{
		if (!stock.IsFull && stock.AllowRestock)
			stock.CurrentQuantity++;
			
		QueueRestockTimer();
	}

	private sealed class SellStockInteraction : VendorInteraction
	{
		private readonly StockEntry _stock;

		public SellStockInteraction(StockEntry stock) : base(new LocalizationEntry(6500016, stock.Name, stock.Cost.ToString(), "gold"), range: 2) /* Sell: {0} ({1} {2}) */
		{
			_stock = stock;
		}

		protected override void OnClick(PlayerEntity source, VendorEntity vendor, Point2D counter)
		{
			if (vendor is Shopkeeper shopkeeper)
				shopkeeper.SellStock(source, counter, _stock);
		}
	}

	private sealed class ShowPricesInteraction : VendorInteraction
	{
		public static readonly ShowPricesInteraction Instance = new ShowPricesInteraction();
		
		private ShowPricesInteraction() : base("Show prices", range: 2)
		{
		}

		protected override void OnClick(PlayerEntity source, VendorEntity vendor, Point2D counter)
		{
			if (vendor is Shopkeeper shopkeeper)
				shopkeeper.ShowPrices(source);
		}
	}

	private sealed class AppraiseInteraction : InteractionEntry
	{
		public static readonly AppraiseInteraction Instance = new AppraiseInteraction();
		
		private AppraiseInteraction() : base("Appraise", range: 2)
		{
		}

		public override void OnClick(PlayerEntity source, WorldEntity target)
		{
			if (source is null || (target is not Shopkeeper shopkeeper))
				return;

			source.SendMessage(Color.LimeGreen, "Target the item you want appraised.");
			source.Target = new AppraiseItemTarget(shopkeeper);
		}
	}

	private sealed class BuyItemInteraction : InteractionEntry
	{
		public static readonly BuyItemInteraction Instance = new BuyItemInteraction();
		
		private BuyItemInteraction() : base("Buy", range: 2)
		{
		}

		public override void OnClick(PlayerEntity source, WorldEntity target)
		{
			if (source is null || (target is not Shopkeeper shopkeeper))
				return;

			source.SendMessage(Color.LimeGreen, "Target the item you want to sell.");
			source.Target = new BuyItemTarget(shopkeeper);
		}
	}

	private sealed class BuyAllInteraction : VendorInteraction
	{
		public static readonly BuyAllInteraction Instance = new BuyAllInteraction();
		
		private BuyAllInteraction() : base("Buy All", range: 2)
		{
		}

		protected override void OnClick(PlayerEntity source, VendorEntity vendor, Point2D counter)
		{
			if (vendor is Shopkeeper shopkeeper)
				shopkeeper.BuyItems(source, counter, "all");
		}
	}

	private sealed class BuyItemTarget : ItemTarget
	{
		private static bool IsEligibleTarget(PlayerEntity player, Point2D counter, ItemEntity target)
		{
			if (target is Gold)
				return false;

			if (target.Parent is null)
				return target.Location == counter;

			if (target.Parent is not PlayerEntity owner || owner != player)
				return false;

			return target.Container is Backpack;
		}
		
		private readonly Shopkeeper _shopkeeper;

		public BuyItemTarget(Shopkeeper shopkeeper)
		{
			_shopkeeper = shopkeeper;
		}

		protected override void OnTarget(MobileEntity source, ItemEntity target)
		{
			if (source is null || (source is not PlayerEntity player))
				return;

			if (_shopkeeper is null || _shopkeeper.Deleted)
				return;

			if (!_shopkeeper.AtCounter(source, out var counter))
			{
				if (_shopkeeper._counters.Any())
					_shopkeeper.SayTo(player, 6300236); /* Please step up to a counter. */
				else
					_shopkeeper.SayTo(player, 6300237); /* Please stand closer to me. */

				return;
			}

			if (!IsEligibleTarget(player, counter, target))
			{
				_shopkeeper.SayTo(player, 6300264); /* There is nothing of value here. */
				return;
			}

			var totalValue = target.ActualPrice * target.Amount;

			if (totalValue > 0)
			{
				target.Delete();
				
				player.PlaySound(10007);

				var gold = new Gold
				{
					Amount = totalValue,
				};

				gold.Move(counter, true, _shopkeeper.Segment);
				
				_shopkeeper.SayTo(player, 6300350); /* Thank you for your business. */

				player.SendMessage(Color.LimeGreen, "Target the item you want to sell.");
				player.Target = new BuyItemTarget(_shopkeeper);
			}
			else
			{
				_shopkeeper.SayTo(player, 6300264); /* There is nothing of value here. */
			}
		}
	}

	private sealed class AppraiseItemTarget : ItemTarget
	{
		private readonly Shopkeeper _shopkeeper;

		public AppraiseItemTarget(Shopkeeper shopkeeper)
		{
			_shopkeeper = shopkeeper;
		}

		protected override void OnTarget(MobileEntity source, ItemEntity target)
		{
			if (source is null || !(source is PlayerEntity player))
				return;

			if (_shopkeeper is null || _shopkeeper.Deleted)
				return;

			if (!_shopkeeper.AtCounter(source, out var counter))
			{
				if (_shopkeeper._counters.Any())
					_shopkeeper.SayTo(player, 6300236); /* Please step up to a counter. */
				else
					_shopkeeper.SayTo(player, 6300237); /* Please stand closer to me. */

				return;
			}

			_shopkeeper.Appraise(player, target);
		}
	}
}
