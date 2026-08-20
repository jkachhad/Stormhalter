using System;
using System.Drawing;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class ShieldStatus : SpellStatus
{
	/// <summary>
	/// Gets the maximum protection provided by shield status.
	/// </summary>
	public static readonly int MaximumProtection = 9;
	
	public override int SpellRemovedSound => 223;

	public ShieldStatus(MobileEntity entity) : base(entity)
	{
	}

	protected override StatModifierSet GetStatModifiers(MobileEntity target)
	{
		var modifiers = base.GetStatModifiers(target);

		if (Spells.Count > 0)
			modifiers.Add(EntityStat.Barrier, 3);

		return modifiers;
	}

	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		if (source is SpellSource && Spells.Count is 0)
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 552); /* The spell of [Shield] has worn off. */
		}
		
		base.OnSourceRemoved(source);
	}
}
