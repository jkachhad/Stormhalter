namespace Kesmai.Server.Game
{
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
	}
}
