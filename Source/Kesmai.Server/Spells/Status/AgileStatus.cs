using System;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Items;

namespace Kesmai.Server.Spells;

/// <summary>
/// Gives a player the Dexterity of the Dexterity rings they wear. Each worn ring is an item source, and together the
/// rings add at most <see cref="MaximumBonus"/> Dexterity, so wearing several rings doesn't stack beyond it. The
/// status has no icon.
/// </summary>
public class AgileStatus : SpellStatus
{
	/// <summary>
	/// The most Dexterity that the rings add together.
	/// </summary>
	public const int MaximumBonus = 2;

	/// <inheritdoc />
	public override bool Hidden => true;

	public AgileStatus(MobileEntity entity) : base(entity)
	{
	}

	/// <inheritdoc />
	protected override StatModifierSet GetStatModifiers(MobileEntity target)
	{
		var modifiers = base.GetStatModifiers(target);
		var bonus = Items.Keys.OfType<DexterityRing>().Sum(ring => ring.BonusDexterity);

		if (bonus > 0)
			modifiers.Add(EntityStat.Dexterity, Math.Min(bonus, MaximumBonus));

		return modifiers;
	}
}
