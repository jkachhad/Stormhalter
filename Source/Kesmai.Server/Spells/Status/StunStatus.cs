using System;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class StunStatus : SpellStatus
{
	private Timer _internalTimer;
		
	private int _ticks;
	private TimeSpan _delay;
		
	public override bool Hidden => true;

	public int Ticks => _ticks;
	public TimeSpan Delay => _delay;

	public StunStatus(MobileEntity entity, int ticks, TimeSpan delay) : base(entity)
	{
		_ticks = ticks;
		_delay = delay;
	}

	public override void OnAcquire()
	{
		base.OnAcquire();

		var facet = _entity.Facet;
		var rounds = _ticks * 2.0 / 3.0;
		var duration = facet.TimeSpan.FromRounds(rounds) + _delay;
			
		_entity.QueueRoundTimer(duration);

		if (_entity.Spell != null && _entity.Spell.AllowInterrupt)
			_entity.Fizzle();
			
		_internalTimer = Timer.DelayCall(TimeSpan.Zero, 
			_entity.Facet.TimeSpan.FromRounds(2.0 / 3.0), _ticks, OnTick);

		Announce();
	}

	public override void OnRemoved()
	{
		// TODO: Should we still do this? Or is there a better mechanism.
		// What if the player is stunned, but has a round lock from action?
		_entity.ClearRound();

		if (_internalTimer != null && _internalTimer.Running)
			_internalTimer.Stop();

		_internalTimer = null;
			
		base.OnRemoved();
	}

	private void Announce()
	{
		var beholders = _entity.GetBeholdersInVisibility().OfType<PlayerGroup>();
				
		foreach (var player in beholders.SelectMany(g => g.Members))
			player.SendLocalizedMessage(6300053, _entity.Name);
	}

	private void OnTick()
	{
		if (_entity != null)
		{
			if (!_entity.Deleted && _entity.IsAlive && _ticks > 0)
			{
				/* Remove any fear effect. */
				if (_entity.GetStatus<FearStatus>() is FearStatus fearStatus)
					_entity.RemoveStatus(fearStatus);
					
				/* Remove any stun effect. */
				if (_entity.GetStatus<DazeStatus>() is DazeStatus dazeStatus)
					_entity.RemoveStatus(dazeStatus);
					
				if (_entity is PlayerEntity)
					_entity.SendLocalizedMessage(6300052);

				_ticks--;

				if (_ticks <= 0)
					_entity.RemoveStatus(this);
			}
			else
			{
				_entity.RemoveStatus(this);
			}
		}
	}
}