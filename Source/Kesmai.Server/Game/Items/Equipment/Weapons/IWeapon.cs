using System;
using Kesmai.Server.Game;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public interface IWeapon
{
	/// <summary>
	/// Gets the skill utilized by this <see cref="IWeapon"/> during combat.
	/// </summary>
	Skill Skill { get; }

	/// <summary>
	/// Gets the base attack bonus value for this <see cref="IWeapon"/>.
	/// </summary>
	int BaseAttackBonus { get; }

	/// <summary>
	/// Invoked when this <see cref="IWeapon"/> is used to attack a <see cref="MobileEntity"/>.
	/// </summary>
	/// <param name="attacker">The attacking entity.</param>
	/// <param name="defender">The defending entity.</param>
	void OnHit(MobileEntity attacker, MobileEntity defender);
		
	/// <summary>
	/// Gets the attack bonus provided by this <see cref="IWeapon"/> for <see cref="MobileEntity"/>.
	/// </summary>
	double GetAttackBonus(MobileEntity attacker, MobileEntity defender);

	/// <summary>
	/// Gets the swing delay for this <see cref="IWeapon"/> for <see cref="MobileEntity"/>.
	/// </summary>
	TimeSpan GetSwingDelay(MobileEntity entity);

	/// <summary>
	/// Gets the multiplier for skill gain awarded per weapon swing.
	/// </summary>
	double GetSkillMultiplier();

	/// <summary>
	/// Gets the weapon flags.
	/// </summary>
	WeaponFlags Flags { get; }

	/// <summary>
	/// Gets the minimum damage for this <see cref="IWeapon"/>.
	/// </summary>
	int MinimumDamage { get; }

	/// <summary>
	/// Gets the maximum damage for this <see cref="IWeapon"/>.
	/// </summary>
	int MaximumDamage { get; }

	/// <summary>
	/// Gets the penetration value for this <see cref="IWeapon"/>.
	/// </summary>
	ShieldPenetration Penetration { get; }

	/// <summary>
	/// Gets the maximum range at which this weapon can be used.
	/// </summary>
	int MaxRange { get; }
		
	/// <summary>
	/// Gets the <see cref="Poison"/> applied to this <see cref="IWeapon"/>.
	/// </summary>
	Poison Poison { get; set; }
		
	/// <summary>
	/// Gets a value determining if the specified <see cref="Alignment"/> can use this weapon.
	/// </summary>
	public bool CanUse(Alignment alignment)
	{
		var flags = Flags;

		if ((flags.HasFlag(WeaponFlags.Lawful) && alignment != Alignment.Lawful) ||
		    (flags.HasFlag(WeaponFlags.Neutral) && alignment != Alignment.Neutral) ||
		    (flags.HasFlag(WeaponFlags.Chaotic) && alignment != Alignment.Chaotic && alignment != Alignment.Evil))
			return false;

		return true;
	}
}