using System;
using System.IO;
using System.Linq;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Merchant<T> : VendorEntity, IMerchant where T : ItemEntity, new()
{
	private InternalTimer _timer;

	public uint Cost { get; private set; }
		
	public int MinimumSupply { get; private set; }
	public int MaximumSupply { get; private set; }

	public Type ItemType { get; } = typeof(T);
		
	public Merchant(uint cost, int minimum = 1, int maximum = int.MaxValue)
	{
		Cost = cost;

		MinimumSupply = minimum;
		MaximumSupply = maximum;
	}

	public bool Sells(ItemEntity item)
	{
		return Sells(item.GetType());
	}
		
	public bool Sells(Type type)
	{
		return ItemType.IsAssignableFrom(type);
	}

	public override void OnEnterWorld()
	{
		base.OnEnterWorld();
			
		StartTimer();
	}

	public override void OnDepartWorld()
	{
		StopTimer();
			
		base.OnDepartWorld();
	}

	public T CreateItem()
	{
		return new T()
		{
			IsPurchased = true,
		};
	}

	private void StartTimer()
	{
		if (_timer != null)
			_timer.Stop();

		_timer = new InternalTimer(this);
		_timer.Start();
	}

	private void StopTimer()
	{
		if (_timer != null)
		{
			_timer.Stop();
			_timer = null;
		}
	}
		
	private class InternalTimer : FacetTimer
	{
		private Merchant<T> _entity;
			
		public InternalTimer(Merchant<T> entity) : base(entity.Facet, TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(3.0))
		{
			_entity = entity;
		}

		protected override void OnExecute()
		{
			if (!_entity.IsAlive)
			{
				_entity.StopTimer();
				return;
			}
				
			var segment = _entity.Segment;
			var items = segment.GetItemsAt(_entity.Location).ToList();
				
			var gold = items.OfType<Gold>().ToList();

			if (gold.Any())
			{
				var totalItems = items.OfType<T>().Count();
				var spawn = _entity.MinimumSupply - totalItems;

				if (totalItems > _entity.MaximumSupply)
				{
					return;
				}

				if (spawn < 1)
					spawn = 1;

				while (spawn > 0)
				{
					var totalGold = gold.Sum(g => g.Amount);
					var consumeAmount = _entity.Cost;
						
					if (totalGold < consumeAmount)
						break;

					if (_entity.ConsumeFromCollection(gold, consumeAmount))
						_entity.CreateItem().Move(_entity.Location, true, segment);

					spawn--;
				}
			}
		}
	}
}