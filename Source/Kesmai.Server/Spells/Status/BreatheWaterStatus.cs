using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class BreatheWaterStatus : SpellStatus
{
	public override int SpellRemovedSound => 221;

	public BreatheWaterStatus(MobileEntity entity) : base(entity)
	{
	}

	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		base.OnSourceRemoved(source);

		if (source is SpellSource && !_spellSources.Any())
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 504); /* The spell of [Breathe Water] has worn off. */
		}
	}

	public override void OnAcquire()
	{
		base.OnAcquire();
			
		_entity.StopWaterTimer();
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
			
		var (_, water) = _entity.GetComponentInLocation<Water>();

		if (water != null)
			_entity.QueueWaterTimer(water.Depth);
	}
}