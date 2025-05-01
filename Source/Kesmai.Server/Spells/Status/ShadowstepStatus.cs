using System;
using System.Drawing;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class ShadowstepStatus : SpellStatus
{
	private Timer _internalTimer;
	private int _rounds;
		
	public int AttackPenalty => 200;

	public ShadowstepStatus(MobileEntity entity, int rounds) : base(entity)
	{
		_rounds = rounds;
	}
		
	public override void OnAcquire()
	{
		_internalTimer = _entity.Facet.Schedule(_rounds, OnTick);
	}
		
	public override void OnRemoved()
	{
		if (_internalTimer != null && _internalTimer.Running)
			_internalTimer.Stop();

		_internalTimer = null;
			
		base.OnRemoved();
	}
		
	private void OnTick()
	{
		_entity.RemoveStatus(this);
	}
}