using System;
using System.Collections.Generic;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class FearStatus : SpellStatus
{
	private Timer _internalTimer;
	private int _rounds;
		
	public override bool Hidden => true;
		
	public FearStatus(MobileEntity entity, int rounds) : base(entity)
	{
		_rounds = rounds;
	}
		
	public override void OnAcquire()
	{
		base.OnAcquire();

		if (_entity.Looking != default(SegmentTile))
			_entity.LookAt(default(SegmentTile));

		_internalTimer = _entity.Facet.Schedule(TimeSpan.Zero, OnTick);
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
		if (_entity.Movement > 0 && !_entity.Deleted && _entity.IsAlive && _rounds > 0)
		{
			var facet = _entity.Facet;
			var segment = _entity.Segment;
			var location = _entity.Location;
				
			var possibleDirections = new List<Direction>()
			{
				Direction.None,
			};

			foreach (var direction in Direction.All.Except(possibleDirections))
			{
				var tile = segment.FindTile(location + direction);

				if (tile != null && tile.AllowsMovementPath())
					possibleDirections.Add(direction);
			}
				
			_entity.EmitSound((_entity.IsFemale ? 206 : 203), 3, 6);
				
			if (_entity.RequestPath(new[] { possibleDirections.Random() }))
				_entity.QueueMovementTimer();
				
			_rounds--;

			if (_rounds > 0)
				_internalTimer = facet.Schedule(_entity.GetRoundDelay(), OnTick);
			else
				_entity.RemoveStatus(this);
		}
		else
		{
			_entity.RemoveStatus(this);
		}
	}
}