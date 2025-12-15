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

	protected override void OnSourceAdded(SpellStatusSource source)
	{
		base.OnSourceAdded(source);

		if (source is SpellSource spellSource && _spellSources.Count is 1)
			_entity.Stats[EntityStat.Strength].Add(+6, ModifierType.Constant);
	}

	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		if (source is SpellSource spellSource && _spellSources.Count is 0)
		{
			_entity.Stats[EntityStat.Strength].Remove(+6, ModifierType.Constant);
				
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 553); /* The spell of [Strength] has worn off. */
		}

		base.OnSourceRemoved(source);
	}
}