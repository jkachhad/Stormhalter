using System.IO;
using System.Linq;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Ghoul : CreatureEntity, IUndead
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Ghoul"/> class.
	/// </summary>
	public Ghoul()
	{
		Name = "ghoul";
		Body = 77;

		Alignment = Alignment.Chaotic;
	}


	/// <summary>
	/// Gets the death sound.
	/// </summary>
	public override int GetNearbySound() => 116;
	public override int GetAttackSound() => 123;
	public override int GetDeathSound() => 6;

	public override Corpse GetCorpse() => default(Corpse);

	/// <inheritdoc/>
	public override AIBrain GetBrain() => AIBrain.FromWeapon(this, RightHand);
}
