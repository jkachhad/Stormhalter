using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class PoisonProtectionStatus : SpellStatus
{
	public override int SpellRemovedSound => 221;
		
	public PoisonProtectionStatus(MobileEntity entity) : base(entity)
	{
	}
		
	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		base.OnSourceRemoved(source);

		if (source is SpellSource && !_spellSources.Any())
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 584); /* The spell of [Protection from Poison] has worn off. */
		}
	}
}