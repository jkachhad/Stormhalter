using System;
using System.IO;
using System.Linq;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Tailor : VendorEntity
	{
		private InternalTimer _timer;
		
		public Tailor()
		{
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
			private Tailor _entity;
			
			public InternalTimer(Tailor entity) : base(TimeSpan.Zero, entity.GetRoundDelay())
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
				var corpses = segment.GetItemsAt(_entity.Location)
										.OfType<Corpse>().Where(corpse => corpse.Owner is CreatureEntity)
										.ToList();

				foreach (var corpse in corpses)
				{
					corpse.Strip();
					
					var item = corpse.Tan();

					if (item != null)
						item.Move(_entity.Location, true, segment);
					
					corpse.Delete();
				}
			}
		}
	}
}