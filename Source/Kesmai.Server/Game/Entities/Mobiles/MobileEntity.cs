using Kesmai.Server.Items;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public abstract partial class MobileEntity : WorldEntity
	{
		/// <summary>
		/// Calculates the damage mitigation against the specified item.
		/// </summary>
		public int CalculateDamageMitigation(ItemEntity item)
		{
			var damageMitigation = 0;

			/* Calculate armor bonus from chest pieces. */
			var paperdoll = Paperdoll;

			if (paperdoll is not null)
			{
				var armor = paperdoll.Armor;

				if (armor != null)
					damageMitigation += armor.GetArmorBonus(item);
			}

			/* Calculate armor bonus from held items. */
			var leftHand = LeftHand;
			var rightHand = RightHand;
			
			var leftWeapon = leftHand as IWeapon;
			var rightWeapon = rightHand as IWeapon;
			
			if (leftHand is Shield leftShield)
			{
				/* Shields in the left hand apply mitigation. */
				damageMitigation += leftShield.GetShieldBonus(item);
				
				/* If used in combination with a 1-handed weapon, it can also mitigate. */
				if (rightWeapon != null && !rightWeapon.Flags.HasFlag(WeaponFlags.TwoHanded))
					damageMitigation += rightWeapon.GetWeaponBonus(item);
			}
			else if (leftWeapon != null && !leftWeapon.Flags.HasFlag(WeaponFlags.TwoHanded))
			{
				/* Only one handed weapons can mitigate in the left hand.  */
				damageMitigation += leftWeapon.GetWeaponBonus(item);
			}
			else if (leftHand == null && rightWeapon != null)
			{
				/* Right handed weapons always mitigate if left hand empty, 1h or 2h. */
				damageMitigation += rightWeapon.GetWeaponBonus(item);
			}

			return damageMitigation;
		}
		
		/// <summary>
		/// Checks if the <see cref="ShieldPenetration"/> value can penetrate the shield for this instance.
		/// </summary>
		/// <returns>Returns true if the attack is not blocked by a shield buff.</returns>
		[WorldForge]
		public virtual bool CheckShieldPenetration(ShieldPenetration penetration)
		{
			if (GetStatus<ShieldStatus>() is ShieldStatus status && status.Protection > 0)
			{
				if (penetration > ShieldPenetration.None)
				{
					var penetrationDice = 0;
	
					switch (penetration)
					{
						case ShieldPenetration.VeryHeavy: penetrationDice = 90; break; // Very Heavy
						case ShieldPenetration.Heavy: penetrationDice = 45; break; // Heavy
						case ShieldPenetration.Medium: penetrationDice = 36; break; // Medium
						case ShieldPenetration.Light: penetrationDice = 18; break; // Light
						case ShieldPenetration.VeryLight: penetrationDice = 13; break; // Very Light
					}
	
					return Utility.RandomBetween(1, penetrationDice) > status.Protection;
				}

				return false;
			}

			return true;
		}
	}
}
