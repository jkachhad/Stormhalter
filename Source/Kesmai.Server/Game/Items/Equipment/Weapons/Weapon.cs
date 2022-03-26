using System;
using System.Collections.Generic;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items
{
	public abstract partial class Weapon : ItemEntity, IWeapon, IArmored, IWieldable
	{
		/// <summary>
		/// Gets the attack bonus provided by this <see cref="IWeapon" /> for <see cref="MobileEntity" />.
		/// </summary>
		/// <remarks>
		/// Attack bonus provided by weapons is dependent on weapon skill, <see cref="PlayerEntity.Dexterity"/>, 
		/// and <see cref="BaseAttackBonus"/>.
		/// </remarks>
		public virtual double CalculateAttackBonus(MobileEntity attacker, MobileEntity defender)
		{
			var attackBonus = BaseAttackBonus;

			/* Enchanted weapons from a Knight have additional attack bonus based on the level of the knight. */
			if (IsEnchanted)
			{
				if (attacker is PlayerEntity player)
					attackBonus += ((player.Level - 5) / 3).Clamp(1, 5);
				else
					attackBonus += 1;
			}

			/*
			 * "The Black Broadsword (BBS for short) is the best weapon in the game. It hits hard and blocks well.
			 * 	Though the Silver Greataxe can hit harder at times, the BBS has a harder hitting average than the
			 * 	Greataxe. The BBS has two advantages. First, it blocks well, and the BBS is "lawful" so it gains
			 * 	one extra damage add against evil crits like dragons and drakes."
			 * 
			 */
			if (defender != null)
			{
				if (Flags.HasFlag(WeaponFlags.Lawful) && defender.Alignment == Alignment.Evil)
					attackBonus += 1;
			}

			return attackBonus;
		}

		/// <inheritdoc/>
		public int CalculateMitigationBonus(ItemEntity item)
		{
			var itemProtections = new List<int>();

			/* Determine the type of weapon attacking us. For hands, we use bashing damage. */
			var flags = WeaponFlags.Bashing;

			if (item is IWeapon weapon)
				flags = weapon.Flags;
			
			/* Determine mitigation. */
			var protectionBonus = 0;
			
			if ((flags & WeaponFlags.Projectile) != 0)
				itemProtections.Add(ProjectileMitigation);

			if ((flags & WeaponFlags.Piercing) != 0)
				itemProtections.Add(PiercingMitigation);

			if ((flags & WeaponFlags.Slashing) != 0)
				itemProtections.Add(SlashingMitigation);

			if ((flags & WeaponFlags.Bashing) != 0)
				itemProtections.Add(BashingMitigation);

			if (itemProtections.Any())
				protectionBonus += itemProtections.Min();

			return protectionBonus;
		}

		/// <inheritdoc/>
		/// <remarks>
		/// Weapons only provide a blocking bonus against other weapons. Two-handed weapons
		/// should only provide a benefit if equipped in the right-hand. Used for DR calculations.
		/// Weapon BaseArmorBonus does not help block projectiles.
		/// </remarks>
		public override int CalculateBlockingBonus(ItemEntity item)
		{
			return BaseArmorBonus;
		}

		/// <summary>
		/// Calculates the fumble chance as a percent.
		/// </summary>
		public override double CalculateFumbleChance(MobileEntity entity)
		{
			// Skill Level = 3 =>  ((3 + 1)^2) * 10 = 160 => 1 / 160;
			// Skill Level = 4 =>  ((4 + 1)^2) * 10 = 250 => 1 / 250;
			return 1 / (10 * Math.Pow(entity.GetSkillLevel(Skill) + 1, 2));
		}

		public virtual void OnWield(MobileEntity entity)
		{
		}
		
		public virtual void OnUnwield(MobileEntity entity)
		{
		}
	}
}