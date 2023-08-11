using System;
using Kesmai.Server.Miscellaneous;

namespace Kesmai.Server.Game;

public partial class PlayerEntity : MobileEntity
{
	/// <summary>
	/// Checks if this instance is dazed from the damage source.
	/// </summary>
	/// <returns>
	/// Returns a value representing the total number of seconds this entity is dazed.
	/// </returns>
	public override int CheckDaze(DamageType damageType, int damage, double baseChance)
	{
		var percentChange = (damage * 100.0) / Health;

		if (damageType == DamageType.Melee)
		{
			var paperdoll = Paperdoll;

			if (paperdoll != null)
				percentChange -= Stats[EntityStat.DazeProtection].Value;
		}

		var chance = baseChance + percentChange;

		if (chance > 1.0)
		{
			var adjustedChance = (int)Math.Ceiling((1.0 / 270.0) * Math.Pow(chance, 2));

			if (adjustedChance >= 1)
			{
				if (adjustedChance > 99)
					return 11;

				var roll = Utility.RandomBetween(1, 100);

				if (roll <= (int)adjustedChance)
				{
					if (percentChange >= 90.0)
						return 9;
					else if (percentChange >= 70.0)
						return 7;
					else if (percentChange >= 40.0)
						return 5;
					else
						return 3;
				}
			}
		}

		/* The base method returns 0 seconds. */
		return base.CheckDaze(damageType, damage, chance);
	}
}