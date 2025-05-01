using System;
using System.Drawing;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class StalkerInTheShadowsStatus : SpellStatus
{
	private Timer _internalTimer;
		
	public StalkerInTheShadowsStatus(MobileEntity entity) : base(entity)
	{
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
			
		if (_internalTimer != null)
			_internalTimer.Stop();
			
		_internalTimer = null;
	}

	public void OnDamageTaken()
	{
		if (_internalTimer != null && _internalTimer.Running)
			return;
			
		_internalTimer = _entity.Facet.Schedule(2, OnTick);
	}

	public void OnFinishMovement()
	{
		if (_internalTimer != null)
			_internalTimer.Stop();
			
		_internalTimer = null;
	}
		
	private void OnTick()
	{
		if (_entity.GetStatus(typeof(HideStatus), out var hideStatus))
			_entity.RemoveStatus(hideStatus);
			
		_entity.RemoveStatus(this);
	}
}