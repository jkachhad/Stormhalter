using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class BlindFearProtectionStatus : SpellStatus
{
	public override int SpellRemovedSound => 223;
		
	public BlindFearProtectionStatus(MobileEntity entity) : base(entity)
	{
	}

	protected override StatModifierSet GetStatModifiers(MobileEntity target)
	{
		var modifiers = base.GetStatModifiers(target);
		modifiers.Add(EntityStat.BlindProtection, 1);
		modifiers.Add(EntityStat.FearProtection, 1);
		return modifiers;
	}
		
	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		base.OnSourceRemoved(source);

		if (source is SpellSource && !_spellSources.Any())
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 541); /* The spell of [Protection from Blind and Fear] has worn off. */
		}
	}
}
