namespace Kesmai.Server.Game
{
	public abstract partial class MobileEntity : WorldEntity
	{
		/// <summary>
		/// Calculates the damage modifier against the specified item.
		/// </summary>
		public int CalculateDamageModifier(ItemEntity item)
		{
			var paperdoll = Paperdoll;
			
			if (paperdoll is null)
				return 0;
			
			var damageModifier = 0;
			
			var armor = paperdoll.Armor;

			if (armor != null)
			{
				var armorBonus = armor.GetArmorBonus(item);

				if (armorBonus > 0)
					damageModifier = armorBonus;
			}

			return damageModifier;
		}
	}
}
