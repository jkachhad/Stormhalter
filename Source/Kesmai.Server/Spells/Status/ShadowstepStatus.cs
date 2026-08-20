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
		
	protected override void OnAcquire()
	{
		_internalTimer = Timer.DelayCall(_entity.Facet.TimeSpan.FromRounds(_rounds), 
			OnTick);
	}
		
	protected override void OnRemoved()
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