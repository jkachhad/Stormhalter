using System;
using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Items;

namespace Kesmai.Server.Spells;

public class StrengthSpellStatus : SpellStatus
{
	public override int SpellRemovedSound => 223;

	public StrengthSpellStatus(MobileEntity entity) : base(entity)
	{
	}

	protected override StatModifierSet GetStatModifiers(MobileEntity target)
	{
		var modifiers = base.GetStatModifiers(target);

		if (_spellSources.Count > 0)
			modifiers.Add(EntityStat.Strength, 6);

		return modifiers;
	}

	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		if (source is SpellSource && _spellSources.Count is 0)
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 553); /* The spell of [Strength] has worn off. */
		}

		base.OnSourceRemoved(source);
	}
}
