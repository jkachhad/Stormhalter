using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class FireProtectionStatus : SpellStatus
{
	public override int SpellRemovedSound => 223;
		
	public FireProtectionStatus(MobileEntity entity) : base(entity)
	{
	}

	protected override StatModifierSet GetStatModifiers(MobileEntity target)
	{
		var modifiers = base.GetStatModifiers(target);
		modifiers.Add(EntityStat.FireProtection, 20);
		return modifiers;
	}
		
	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		base.OnSourceRemoved(source);

		if (source is SpellSource && _spellSources.Count is 0)
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 543); /* The spell of [Protection from Fire] has worn off. */
		}
	}
}
