using System;
using System.IO;
using System.Linq;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Tailor : VendorEntity
{
	private InternalTimer _timer;
		
	public Tailor()
	{
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
		private Tailor _entity;
			
		public InternalTimer(Tailor entity) : base(entity.Facet, TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(3.0))
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