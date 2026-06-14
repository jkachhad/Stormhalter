using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class TrollGuard : CreatureEntity
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TrollGuard"/> class.
	/// </summary>
	public TrollGuard()
	{
		Name = "troll.gaurd";
		Body = 148;

		Alignment = Alignment.Chaotic;
	}


	/// <summary>
	/// Gets the death sound.
	/// </summary>
	public override int GetNearbySound() => 265;
	public override int GetAttackSound() => 266;
	public override int GetDeathSound() => 267;

	public override ItemEntity OnCorpseTanned()
	{
		return new LeatherArmor();
	}

	/// <inheritdoc/>
	public override AIBrain GetBrain() => AIBrain.FromWeapon(this, RightHand);
}
