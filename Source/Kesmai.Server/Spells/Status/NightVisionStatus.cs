using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class NightVisionStatus : SpellStatus
{
	public override int SpellRemovedSound => 221;
		
	[WorldForge]
	public NightVisionStatus(MobileEntity entity) : base(entity)
	{
	}
		
	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		base.OnSourceRemoved(source);

		if (source is SpellSource && !_spellSources.Any())
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 536); /* The spell of [Night Vision] has worn off. */
		}
	}

	public override void OnAcquire()
	{
		base.OnAcquire();
			
		_entity.Delta(MobileDelta.Visibility);
	}

	public override void OnRemoved()
	{
		_entity.Delta(MobileDelta.Visibility);
			
		base.OnRemoved();
	}
}