using Kesmai.Server.Game;

namespace Kesmai.Server.Items
{
	public partial interface IArmored
	{
		/// <summary>
		/// Gets the blocking provided by this <see cref="IArmored"/> against a specified <see cref="ItemEntity"/>.
		/// </summary>
		int CalculateBlockingBonus(ItemEntity item);
		
		/// <summary>
		/// Gets the damage mitigation provided by this <see cref="IArmored"/> against a specified <see cref="ItemEntity"/>.
		/// </summary>
		int CalculateMitigationBonus(ItemEntity item);
		
		/// <summary>
		/// Gets the base armor bonus provided by this <see cref="IArmored"/> against all attack types.
		/// </summary>
		int BaseArmorBonus { get; }

		/// <summary>
		/// Gets the damage mitigation provided against slashing attacks.
		/// </summary>
		int SlashingMitigation { get; }

		/// <summary>
		/// Gets the damage mitigation provided against piercing attacks.
		/// </summary>
		int PiercingMitigation { get; }

		/// <summary>
		/// Gets the damage mitigation provided against bashing attacks.
		/// </summary>
		int BashingMitigation { get; }

		/// <summary>
		/// Gets the damage mitigation provided against projectile attacks.
		/// </summary>
		int ProjectileMitigation { get; }
	}
}