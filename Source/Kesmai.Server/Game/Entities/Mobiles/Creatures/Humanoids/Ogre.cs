using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Ogre : CreatureEntity
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Ogre"/> class.
	/// </summary>
	public Ogre()
	{
		Name = "ogre";
		Body = 11;

		Alignment = Alignment.Chaotic;
	}

	/// <inheritdoc/>
	public override void OnSpawn()
	{
		base.OnSpawn();
	}

	/// <summary>
	/// Gets the death sound.
	/// </summary>
	public override int GetDeathSound() => 163;
	public override int GetNearbySound() => 125;
	public override int GetAttackSound() => 144;
		
	public override ItemEntity OnCorpseTanned()
	{
		return new LeatherArmor();
	}

	/// <inheritdoc/>
	public override AIBrain GetBrain() => AIBrain.FromWeapon(this, RightHand);
}
