using Kesmai.Server.Items;

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
			if (item is Weapon weapon)
			{
				var attackBonus = weapon.GetAttackBonus(this, defender) + GetSkillLevel(weapon.Skill);

				var dexterity = Stats.Dexterity;

				if (dexterity > 17)
					attackBonus += (dexterity - 17);

				return attackBonus;
			}

			var skillLevel = GetSkillLevel(Skill.Hand);

			var hindrance = CalculateHindrance();
			var hindrancePenalty = (hindrance.Total * skillLevel) * 0.25;

			if (item != null)
			{
				if (item is Gauntlets gauntlets)
				{
					skillLevel += (int)gauntlets.Penetration;
				}
				else
				{
					return 0;
				}
			}

			var skillBonus = (skillLevel * 1.2) - 2.0;

			return (skillBonus - hindrancePenalty);
		}
	}
}