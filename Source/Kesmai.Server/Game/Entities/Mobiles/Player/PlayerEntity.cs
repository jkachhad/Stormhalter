using System;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous;

namespace Kesmai.Server.Game
{
	public partial class PlayerEntity : MobileEntity
	{
		/// <summary>
		/// Calculates the attack bonus for this instance when attacking with the 
		/// specified <see cref="ItemEntity"/>
		/// </summary>
		public override double CalculateAttackBonus(ItemEntity item, MobileEntity defender)
		{
			var skill = Skill.Hand;
			var skillLevel = 0d;

			var weaponBonus = 0d;

			if (item is IWeapon weapon)
			{
				skill = weapon.Skill;
				weaponBonus = weapon.GetAttackBonus(this, defender);
			}

			if (skill != Skill.Hand)
				skillLevel = GetSkillLevel(skill);
			else
				skillLevel = GetSkillLevel(Skill.Hand) * 1.2 - 2.0;

			/* Dexterity */
			var dexterityBonus = 0.0d;
			var dexterity = Stats.Dexterity;

			if (dexterity > 17)
				dexterityBonus = (dexterity - 17);
			
			return skillLevel + weaponBonus + dexterityBonus;
		}
		
				/// <summary>
		/// Calculates the melee damage for the specified item.
		/// </summary>
		public (int Minimum, int Maximum) CalculateMeleeDamageRange(ItemEntity item, MobileEntity defender)
		{
			var baseMinimumDamage = 1;
			var baseMaximumDamage = 4;
			
			var hitAdds = Stats.StrengthAdds;
			
			var skill = Skill.Hand;
			var skillFactor = 5.0;
			
			if (item != default(ItemEntity))
			{
				if (item is Weapon weapon)
				{
					baseMinimumDamage = weapon.MinimumDamage;
					baseMaximumDamage = weapon.MaximumDamage;
				
					hitAdds += (int)weapon.GetAttackBonus(this, defender);

					skill = weapon.Skill;
					skillFactor = 10.0;
				}
				else if (item is Gauntlets gauntlets)
				{
					if (RightHand != null)
						return (0, 0);
				
					baseMinimumDamage = gauntlets.MinimumDamage;
					baseMaximumDamage = gauntlets.MaximumDamage;
					
					hitAdds += (int)gauntlets.Penetration;
				}
				else
				{
					return (0, 0);
				}
			}
			
			var skillLevel = GetSkillLevel(skill);
			var skillMultiplier = 1.0 + (skillLevel / skillFactor);
			
			var minimumDamage = (int)((baseMinimumDamage + hitAdds) * skillMultiplier);
			var maximumDamage = (int)((baseMaximumDamage + hitAdds) * skillMultiplier);

			return (Math.Max(1, minimumDamage), Math.Max(1, maximumDamage));
		}

		public override int CalculateJumpkickDamage(ItemEntity item, MobileEntity defender)
		{
			var minimumDamage = 0;
			var maximumDamage = 0;
			
			var encumbrancePenalty = 2 * Encumbrance;
			var handSkill = GetSkillLevel(Skill.Hand);
			
			var effectiveSkill = handSkill - encumbrancePenalty;

			if (item is Boots boots)
				effectiveSkill -= 2;

			var skillMultiplier = (Math.Max(0, effectiveSkill) * 0.5) + 2.0;
			
			minimumDamage = (int)((Stats.StrengthAdds + 1) * skillMultiplier);
			maximumDamage = (int)((Stats.StrengthAdds + 5) * skillMultiplier);
			
			return Utility.RandomBetween(Math.Max(1, minimumDamage), Math.Max(1, maximumDamage));
		}

		public override int CalculateProjectileDamage(ItemEntity item, MobileEntity defender)
		{
			if (item is ProjectileWeapon weapon)
			{
				var baseMinimumDamage = weapon.MinimumDamage;
				var baseMaximumDamage = weapon.MaximumDamage;

				var hitAdds = Stats.DexterityAdds + weapon.GetAttackBonus(this, defender);

				var skillLevel = GetSkillLevel(Skill.Bow);
				var skillMultiplier = (skillLevel * 0.1 + 1.0);

				var minimumDamage = (int)((baseMinimumDamage + hitAdds) * skillMultiplier);
				var maximumDamage = (int)((baseMaximumDamage + hitAdds) * skillMultiplier);
				
				return Utility.RandomBetween(Math.Max(1, minimumDamage), Math.Max(1, maximumDamage));
			}

			return 0;
		}

		/// <summary>
		/// Calculates the throw damage for the specified item.
		/// </summary>
		public override int CalculateThrowDamage(ItemEntity item, MobileEntity defender)
		{
			if (item is IWeapon weapon)
			{
				var hitAdds = 0;

				if ((weapon.Flags & WeaponFlags.Throwable) != 0)
					hitAdds += Stats.StrengthAdds + (int)weapon.GetAttackBonus(this, defender);

				if (Stats.BaseStrength >= 18)
					hitAdds++;

				var skillLevel = GetSkillLevel(weapon.Skill);
				var skillMultiplier = (skillLevel * 0.1 + 1.0);

				var minimumDamage = (int)((weapon.MinimumDamage + hitAdds) * skillMultiplier);
				var maximumDamage = (int)((weapon.MaximumDamage + hitAdds) * skillMultiplier);
				
				return Utility.RandomBetween(Math.Max(1, minimumDamage), maximumDamage);
			}

			return 0;
		}
	}
}