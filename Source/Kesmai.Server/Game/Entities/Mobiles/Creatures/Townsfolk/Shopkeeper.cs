using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Humanizer;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Items;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game
{
	[WorldForge]
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
		
		[WorldForge]
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

		public override void HandleOrder(OrderEventArgs args)
		{
			base.HandleOrder(args);

			if (args.Handled)
				return;
			
			var source = args.Source;
			var order = args.Order;
			
			var segment = Segment;
			
			/* example: buy all*/
			/* example: buy (item) */
			if (_buy.TryGetMatch(order, out var buyMatch))
			{
				args.Handled = true;
				
				if (AtCounter(source, out var counter))
				{
					var items = segment.GetItemsAt(counter, buyMatch.Groups[1].Value, (i) => i is Gold).ToList();
					var totalValue = (uint)items.Sum(i => i.ActualPrice * i.Amount);

					if (totalValue > 0)
					{
						foreach (var item in items)
							item.Delete();

						var gold = new Gold()
						{
							Amount = totalValue,
						};
						gold.Move(counter, true, Segment);
						
						SayTo(source, 6300350); /* Thank you for your business. */
					}
					else
					{
						SayTo(source, 6300264); /* There is nothing of value here. */
					}
				}
				else
				{
					if (_counters.Any())
						SayTo(source, 6300236); /* Please step up to a counter. */
					else
						SayTo(source, 6300237); /* Please stand closer to me. */
				}
			}
			/* example: appraise gem */
			else if (_appraise.TryGetMatch(order, out var appraiseMatch))
			{
				args.Handled = true;
				
				if (AtCounter(source, out var counter))
				{
					var item = segment.GetItemAt(counter, appraiseMatch.Groups[1].Value, (i) => i is Gold);

					if (item != default(ItemEntity))
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
					else
					{
						SayTo(source, 6300263); /* Please put the item on the counter. */
					}
				}
				else
				{
					if (_counters.Any())
						SayTo(source, 6300236); /* Please step up to a counter. */
					else
						SayTo(source, 6300237); /* Please stand closer to me. */
				}
			}
			else if (_showPrices.TryGetMatch(order, out var _))
			{
				args.Handled = true;
				
				if (AtCounter(source, out var _))
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

						if (_stock.Any(s => !s.IsFull) && (_restockTimer == null || !_restockTimer.Running))
							Log.Warn($"[{Segment.Name}] Shopkeeper '{Name}' at {Location} not restocking.");
					}
				}
				else
				{
					if (_counters.Any())
						SayTo(source, 6300236); /* Please step up to a counter. */
					else
						SayTo(source, 6300237); /* Please stand closer to me. */
				}
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

					if (stock != null && stock.CurrentQuantity > 0)
					{
						var gold = Segment.GetItemsAt(counter).OfType<Gold>().ToList();

						if (gold.Any())
						{
							if (ConsumeFromLocation<Gold>(counter, (uint)stock.Cost))
							{
								var item = Sell(stock, source);

								if (item != null)
								{
									if (!item.Deleted)
										item.Move(counter, true, Segment);
									
									SayTo(source, 6300350); /* Thank you for your business. */
								}
							}
							else
							{
								SayTo(source, 6300243); /* Are you trying to be funny? */
							}
						}
						else
						{
							if (_counters.Any())
								SayTo(source, 6300246); /* Please put some coins on the counter. */
							else
								SayTo(source, 6300247); /* Please put some coins on the ground. */
						}
					}
					else
					{
						SayTo(source, 6300243); /* Are you trying to be funny? */
					}
				}
				else
				{
					if (_counters.Any())
						SayTo(source, 6300236); /* Please step up to a counter. */
					else
						SayTo(source, 6300237); /* Please stand closer to me. */
				}
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

		[WorldForge]
		public void Stock<T>(string name, int basePrice, int quantity = 1) where T : ItemEntity, new()
		{
			Stock(name, () => new T(), basePrice, default, quantity);
		}

		[WorldForge]
		public void Stock(string name, Func<ItemEntity> create, int basePrice, Action<PlayerEntity, ItemEntity> purchased = default, int quantity = 1)
		{
			Stock(new StockEntry(name, create, basePrice, quantity, purchased));
		}

		[WorldForge]
		public void Stock(StockEntry entry)
		{
			_stock.Add(entry);
		}

		[WorldForge]
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

		[WorldForge]
		public void Restock()
		{
			var stocks = _stock.Where(s => !s.IsFull).ToList();
			
			if (!stocks.Any())
				return;
			
			var stock = stocks.Random();

			if (stock != null)
				Restock(stock);
		}
		
		[WorldForge]
		public void Restock(StockEntry stock)
		{
			if (!stock.IsFull && stock.AllowRestock)
				stock.CurrentQuantity++;
			
			QueueRestockTimer();
		}
	}
}