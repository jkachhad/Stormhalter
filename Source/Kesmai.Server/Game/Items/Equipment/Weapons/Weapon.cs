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
		public virtual double GetAttackBonus(MobileEntity attacker, MobileEntity defender)
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
		
		/// <summary>
		/// Protection Bonus vs. Melee Damage Types
		/// </summary>
		public int GetMeleeBonus(ItemEntity item, WeaponFlags flags)
		{
			var protectionBonus = 0;
			var itemProtections = new List<int>();

			if ((flags & WeaponFlags.Piercing) != 0)
				itemProtections.Add(PiercingProtection);

			if ((flags & WeaponFlags.Slashing) != 0)
				itemProtections.Add(SlashingProtection);

			if ((flags & WeaponFlags.Bashing) != 0)
				itemProtections.Add(BashingProtection);

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
			var flags = WeaponFlags.Bashing;

			if (item is IWeapon weapon)
				flags = weapon.Flags;

			var bonusBlock = 0;

			if ((flags & WeaponFlags.Projectile) != 0)
			{
				return ProjectileProtection;
			}
			else
			{
				bonusBlock += GetMeleeBonus(item, flags);
			}

			return bonusBlock + BaseArmorBonus;
		}
		
		/// <inheritdoc/>
		/// <remarks>
		/// Weapons only provide a blocking bonus against other weapons. Two-handed weapons
		/// should only provide a benefit if equipped in the right-hand.
		/// Used for mitigation calculation. Weapon BaseArmorBonus does not help mitigate.
		/// </remarks>
		public int GetWeaponBonus(ItemEntity item)
		{
			var flags = WeaponFlags.Bashing;

			if (item is IWeapon weapon)
				flags = weapon.Flags;

			var weaponBonus = 0;

			if ((flags & WeaponFlags.Projectile) != 0)
			{
				weaponBonus += ProjectileProtection;
			}
			else
			{
				weaponBonus += GetMeleeBonus(item, flags);
			}

			return weaponBonus;
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