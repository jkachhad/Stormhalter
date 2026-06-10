using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class BlackWolf : AnimalEntity
{
	public BlackWolf()
	{
		Name = "wolf";
		Body = 4;

		Alignment = Alignment.Chaotic;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
	}

	public override int GetNearbySound() => 14;
	public override int GetAttackSound() => 26;
	public override int GetDeathSound() => 38;
		
	public override ItemEntity OnCorpseTanned()
	{
		// TODO: Does this wolf produce a different type of jacket?
		return new WolfJacket();
	}

	/// <inheritdoc/>
	public override AIBrain GetBrain() => new CombatAI(this);
}
