using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class FeatherFallStatus : SpellStatus
{
	public override int SpellRemovedSound => 223;
		
	public FeatherFallStatus(MobileEntity entity) : base(entity)
	{
	}
		
	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		base.OnSourceRemoved(source);

		if (source is SpellSource && !_spellSources.Any())
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 514); /* The spell of [Feather Fall] has worn off. */
		}
	}
}