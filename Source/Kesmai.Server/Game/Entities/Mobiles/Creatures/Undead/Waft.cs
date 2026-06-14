using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Waft : CreatureEntity, IUndead
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Waft"/> class.
	/// </summary>
	public Waft()
	{
		Name = "waft";
		Body = 41;

		Alignment = Alignment.Chaotic;
	}


	/// <summary>
	/// Gets the death sound.
	/// </summary>
	public override int GetDeathSound() => 5;
	public override int GetNearbySound() => 115;
	public override int GetAttackSound() => 122;

	public override Corpse GetCorpse() => default(Corpse);

	/// <inheritdoc/>
	public override AIBrain GetBrain() => AIBrain.FromWeapon(this, RightHand);
}
