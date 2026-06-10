using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Centaur : CreatureEntity
{
	public Centaur()
	{
		Name = "centaur";
		Body = 3;

		Alignment = Alignment.Chaotic;
	}


	public override int GetNearbySound() => 136;
	public override int GetAttackSound() => 155;
	public override int GetDeathSound() => 174;

	public override ItemEntity OnCorpseTanned()
	{
		return new LeatherArmor();
	}

	/// <inheritdoc/>
	public override AIBrain GetBrain() => AIBrain.FromWeapon(this, RightHand);
}
