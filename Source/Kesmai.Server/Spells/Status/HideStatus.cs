using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Items;

namespace Kesmai.Server.Spells;

public class HideStatus : SpellStatus
{
	private Timer _internalTimer;
		
	private List<IWorldGroup> _detectors;
		
	public override int SpellRemovedSound => 221;
		
	public HideStatus(MobileEntity entity) : base(entity)
	{
		_detectors = new List<IWorldGroup>();
	}
		
	public bool IsDetected(MobileEntity beholder)
	{
		if (IsDetector(beholder))
			return true;
			
		var distance = _entity.GetDistanceToMax(beholder.Location);
			
		if (distance <= 1)
		{
			var paperdoll = _entity.Paperdoll;
				
			if (paperdoll != null && paperdoll.Robe is SpectreCloak)
				return (distance <= 0);
				
			return true;
		}

		return false;
	}

	protected override void OnSourceAdded(SpellStatusSource source)
	{
		OnInternalTick();
	}

	protected override void OnSourceRefreshed(SpellStatusSource source)
	{
		OnInternalTick();
	}

	public override void OnAcquire()
	{
		base.OnAcquire();

		if (!_entity.IsHiding)
			_entity.IsHiding = true;

		OnInternalTick();
	}

	public override void OnRemoved()
	{
		base.OnRemoved();

		if (_entity.Client != null)
			_entity.SendLocalizedMessage(Color.Magenta, 6300301); /* You are no longer hiding. */

		if (_entity.IsHiding)
			_entity.IsHiding = false;

		if (_internalTimer != null)
			_internalTimer.Stop();

		if (_entity.GetStatus(typeof(StalkerInTheShadowsStatus), out var stalkerStatus))
			_entity.RemoveStatus(stalkerStatus);

		_internalTimer = null;
	}

	private void OnInternalTick()
	{
		var previous = _detectors.ToList();
			
		var incoming = new List<IWorldGroup>();
		var departing = new List<IWorldGroup>();

		_detectors.Clear();
			
		if (_internalTimer != null && _internalTimer.Running)
			_internalTimer.Stop();

		var hide = _entity.GetHideStrength();
		var groups = _entity.GetGroupsInRange();

		var beheld = _entity.Group;

		foreach (var beholder in groups)
		{
			var distance = beholder.Location.GetDistanceToMax(beheld.Location);

			if (distance <= 1 || (beholder.InLOS(beheld) && beholder.CanSee(beheld)))
				continue;

			var detection = beholder.Leader.GetHideDetection();

			if (hide >= detection)
				continue;

			var chance = (5 * (detection - hide - 1) + 1) * 2;
			var detected = (chance > 100) || (Utility.RandomBetween(1, 100) < chance);

			var detectedPreviously = previous.Contains(beholder);
				
			if (detected)
			{
				_detectors.Add(beholder);

				if (!detectedPreviously)
					incoming.Add(beholder);
			}
			else
			{
				if (detectedPreviously)
					departing.Add(beholder);
			}
		}

		incoming.ForEach(group => group.OnEntityIncoming(_entity));
		departing.ForEach(group => group.OnEntityDeparting(_entity));

		_internalTimer = Timer.DelayCall(_entity.GetRoundDelay(), OnInternalTick);
	}

	public bool IsDetector(MobileEntity entity)
	{
		return _detectors.Contains(entity.Group);
	}
}