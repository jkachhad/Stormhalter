using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class StunResistanceStatus : SpellStatus
{
	public override int SpellRemovedSound => 223;
		
	public StunResistanceStatus(MobileEntity entity) : base(entity)
	{
	}
		
	protected override StatModifierSet GetStatModifiers(MobileEntity target)
	{
		var modifiers = base.GetStatModifiers(target);
		modifiers.Add(EntityStat.StunResistance, 6);
		return modifiers;
	}
		
	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		base.OnSourceRemoved(source);

		if (source is SpellSource && Spells.Count is 0)
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 551); /* The spell of [Stun Resistance] has worn off. */
		}
	}
}
