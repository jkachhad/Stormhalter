using System;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Spells;

/* Most creatures will just wander about aimlessly if they are blinded, and will be unable to attack. */
public class BlindStatus : SpellStatus
{
	private Timer _internalTimer;
	private int _rounds;
		
	public override bool Hidden => true;

	public BlindStatus(MobileEntity entity, int rounds) : base(entity)
	{
		_rounds = rounds;
	}

	public override void OnAcquire()
	{
		base.OnAcquire();
			
		_entity.SendLocalizedMessage(6300050);

		if (_rounds > 0)
			Refresh(_rounds);
	}

	public override void OnRemoved()
	{
		_entity.Delta(MobileDelta.Visibility);

		if (_internalTimer != null && _internalTimer.Running)
			_internalTimer.Stop();

		_internalTimer = null;
			
		base.OnRemoved();
	}

	private void OnTick()
	{
		if (!_entity.Deleted && _entity.IsAlive && _rounds > 0)
		{
			_rounds--;

			if (_rounds > 0)
				_internalTimer = Timer.DelayCall(_entity.GetRoundDelay(), OnTick);
			else
				_entity.RemoveStatus(this);
		}
		else
		{
			_entity.RemoveStatus(this);
		}
	}

	public void Refresh(int rounds)
	{
		if (_rounds > rounds)
			return;

		_rounds = rounds;
			
		if (_entity.Looking != default(SegmentTile))
			_entity.LookAt(default(SegmentTile));

		_entity.SendPacket(new EntityClearPacket());
		_entity.Delta(MobileDelta.Visibility);
			
		if (_internalTimer != null && _internalTimer.Running)
			_internalTimer.Stop();
			
		_internalTimer = Timer.DelayCall(TimeSpan.Zero, OnTick);
	}
}