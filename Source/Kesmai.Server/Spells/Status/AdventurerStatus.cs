using System;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class AdventurerStatus : SpellStatus
{
	public static void Refresh(MobileEntity entity)
	{
		if (!entity.GetStatus(typeof(AdventurerStatus), out var status))
		{
			entity.AddStatus(new AdventurerStatus(entity));
		}
		else
		{
			if (status is AdventurerStatus adventurerStatus)
				adventurerStatus.Refresh();
		}
	}
		
	private Timer _internalTimer;
		
	public override bool Hidden => true;
		
	public AdventurerStatus(MobileEntity entity) : base(entity)
	{
	}

	public override void OnAcquire()
	{
		Refresh();
	}
		
	public override void OnRemoved()
	{
		if (_internalTimer != null && _internalTimer.Running)
			_internalTimer.Stop();

		_internalTimer = null;
			
		base.OnRemoved();
	}
		
	public void Refresh()
	{
		if (_internalTimer != null && _internalTimer.Running)
			_internalTimer.Stop();
			
		_internalTimer = _entity.Facet.Schedule(TimeSpan.FromMinutes(3.0), OnTick);
	}
		
	private void OnTick()
	{
		_entity.RemoveStatus(this);
	}
}