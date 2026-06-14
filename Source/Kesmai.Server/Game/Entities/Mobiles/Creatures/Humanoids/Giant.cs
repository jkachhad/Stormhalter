using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Giant : CreatureEntity
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Giant"/> class.
	/// </summary>
	public Giant()
	{
		Name = "giant";
		Body = 82;

		Alignment = Alignment.Chaotic;
	}

	/// <summary>
	/// Gets the death sound.
	/// </summary>
	public override int GetDeathSound() => 79;
	public override int GetNearbySound() => 197;
	public override int GetAttackSound() => 0; // TODO

	public override ItemEntity OnCorpseTanned()
	{
		return new LeatherJacket();
	}

	/// <inheritdoc/>
	public override AIBrain GetBrain() => AIBrain.FromWeapon(this, RightHand);
}
