using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

/// <summary>
/// Adds the client properties shared by items that implement <see cref="IWeapon"/> or <see cref="IArmored"/>.
/// </summary>
/// <remarks>
/// Values equal to their default are omitted; the client treats a missing property as zero or none.
/// </remarks>
public static class ItemPropertySetExtensions
{
	public static void SetWeaponProperties(this ItemPropertySet properties, IWeapon weapon)
	{
		// the skill identifier, not the implicit int conversion (which is a zero-based index).
		if (weapon.Skill is { } skill)
			properties.Set(ItemPropertyId.WeaponSkill, skill.Id);

		if (weapon.MinimumDamage != 0)
			properties.Set(ItemPropertyId.MinimumDamage, weapon.MinimumDamage);

		if (weapon.MaximumDamage != 0)
			properties.Set(ItemPropertyId.MaximumDamage, weapon.MaximumDamage);

		if (weapon.BaseAttackBonus != 0)
			properties.Set(ItemPropertyId.BaseAttackBonus, weapon.BaseAttackBonus);

		if (weapon.Flags != WeaponFlags.None)
			properties.Set(ItemPropertyId.WeaponFlags, weapon.Flags);

		if (weapon.Penetration != ShieldPenetration.None)
			properties.Set(ItemPropertyId.Penetration, weapon.Penetration);

		if (weapon.MaxRange != 0)
			properties.Set(ItemPropertyId.MaximumRange, weapon.MaxRange);
	}

	public static void SetArmorProperties(this ItemPropertySet properties, IArmored armored)
	{
		if (armored.BaseArmorBonus != 0)
			properties.Set(ItemPropertyId.BaseArmorBonus, armored.BaseArmorBonus);

		if (armored.SlashingProtection != 0)
			properties.Set(ItemPropertyId.SlashingProtection, armored.SlashingProtection);

		if (armored.PiercingProtection != 0)
			properties.Set(ItemPropertyId.PiercingProtection, armored.PiercingProtection);

		if (armored.BashingProtection != 0)
			properties.Set(ItemPropertyId.BashingProtection, armored.BashingProtection);

		if (armored.ProjectileProtection != 0)
			properties.Set(ItemPropertyId.ProjectileProtection, armored.ProjectileProtection);
	}
}
