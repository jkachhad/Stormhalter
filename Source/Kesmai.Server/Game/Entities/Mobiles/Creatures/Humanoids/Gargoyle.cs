using System.IO;
using System.Linq;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Gargoyle : CreatureEntity
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Gargoyle"/> class.
	/// </summary>
	public Gargoyle()
	{
		Name = "gargoyle";
		Body = 76;

		Alignment = Alignment.Chaotic;

		CanFly = true;
	}


	/// <summary>
	/// Gets the death sound.
	/// </summary>
	public override int GetDeathSound() => 107;
	public override int GetNearbySound() => 93;
	public override int GetAttackSound() => 100;

	public override ItemEntity OnCorpseTanned()
	{
		return new LeatherArmor();
	}

	/// <inheritdoc/>
	public override AIBrain GetBrain() => AIBrain.FromWeapon(this, RightHand);
}
