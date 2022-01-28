using System;
using System.IO;
using System.Linq;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Merchant<T> : VendorEntity, IMerchant where T : ItemEntity, new()
	{
		private InternalTimer _timer;

		public uint Cost { get; private set; }
		
		public int MinimumSupply { get; private set; }
		public int MaximumSupply { get; private set; }

		public Type ItemType { get; } = typeof(T);

		[WorldForge]
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

		protected override void OnLoad()
		{
			base.OnLoad();
			
			StartTimer();
		}

		protected override void OnUnload()
		{
			StopTimer();
			
			base.OnUnload();
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
		
		private class InternalTimer : Timer
		{
			private Merchant<T> _entity;
			
			public InternalTimer(Merchant<T> entity) : base(TimeSpan.Zero, entity.GetRoundDelay())
			{
				_entity = entity;

				Priority = TimerPriority.OneSecond;
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
}