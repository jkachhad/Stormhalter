using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Mummy : CreatureEntity, IUndead
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Mummy"/> class.
	/// </summary>
	public Mummy()
	{
		Name = "mummy";
		Body = 146;

		Alignment = Alignment.Chaotic;
	}

	/// <summary>
	/// Gets the death sound.
	/// </summary>
	public override int GetNearbySound() => 253;
	public override int GetAttackSound() => 254;
	public override int GetDeathSound() => 255;

	public override Corpse GetCorpse() => default(Corpse);

	/// <inheritdoc/>
	public override AIBrain GetBrain() => AIBrain.FromWeapon(this, RightHand);
}
