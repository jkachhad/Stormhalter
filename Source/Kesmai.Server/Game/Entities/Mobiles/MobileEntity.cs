using Kesmai.Server.Items;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

public abstract partial class MobileEntity : WorldEntity
{
	/// <summary>
	/// Calculates the damage modifier against the specified item.
	/// </summary>
	public int CalculateDamageModifier(ItemEntity item)
	{
		var damageModifier = 0;

		/* Calculate armor bonus from chest pieces. */
		var paperdoll = Paperdoll;
			
		if (paperdoll is null)
			return 0;
			
		var armor = paperdoll.Armor;
			
		if (armor != null)
			damageModifier += armor.GetArmorBonus(item);

		return damageModifier;
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