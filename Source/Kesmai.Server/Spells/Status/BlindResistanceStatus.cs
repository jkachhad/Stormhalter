using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class BlindResistanceStatus : SpellStatus
{
	public override int SpellRemovedSound => 223;
		
	public BlindResistanceStatus(MobileEntity entity) : base(entity)
	{
	}
		
	protected override StatModifierSet GetStatModifiers(MobileEntity target)
	{
		var modifiers = base.GetStatModifiers(target);
		modifiers.Add(EntityStat.BlindResistance, 6);
		return modifiers;
	}
		
	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		base.OnSourceRemoved(source);

		if (source is SpellSource && _spellSources.Count is 0)
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 547); /* The spell of [Blind Resistance] has worn off. */
		}
	}
}
