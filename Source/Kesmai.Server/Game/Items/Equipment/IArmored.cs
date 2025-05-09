using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public interface IArmored
{
	/// <summary>
	/// Gets the base armor bonus provided by this <see cref="IArmored"/> against all attack types.
	/// </summary>
	int BaseArmorBonus { get; }

	/// <summary>
	/// Gets the protection provided against slashing attacks.
	/// </summary>
	int SlashingProtection { get; }

	/// <summary>
	/// Gets the protection provided against piercing attacks.
	/// </summary>
	int PiercingProtection { get; }

	/// <summary>
	/// Gets the protection provided against bashing attacks.
	/// </summary>
	int BashingProtection { get; }

	/// <summary>
	/// Gets the protection provided against projectile attacks.
	/// </summary>
	int ProjectileProtection { get; }
	
	/// <summary>
	/// Triggered when this weapon blocks an attack from the specified attacker.
	/// </summary>
	/// <param name="attacker">The hostile entity.</param>
	void OnBlock(MobileEntity attacker);
}