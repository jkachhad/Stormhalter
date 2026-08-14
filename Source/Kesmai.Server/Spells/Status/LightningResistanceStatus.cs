using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class LightningResistanceStatus : SpellStatus
{
	public override int SpellRemovedSound => 223;
		
	public LightningResistanceStatus(MobileEntity entity) : base(entity)
	{
	}
		
	protected override StatModifierSet GetStatModifiers(MobileEntity target)
	{
		var modifiers = base.GetStatModifiers(target);
		modifiers.Add(EntityStat.LightningResistance, 6);
		return modifiers;
	}
		
	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		base.OnSourceRemoved(source);

		if (source is SpellSource && _spellSources.Count is 0)
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 550); /* The spell of [Lightning Resistance] has worn off. */
		}
	}
}
