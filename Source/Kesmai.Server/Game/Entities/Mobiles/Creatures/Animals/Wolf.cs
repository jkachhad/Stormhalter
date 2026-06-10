using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Wolf : AnimalEntity
{
	public Wolf()
	{
		Name = "wolf";
		Body = 24;

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
		return new WolfJacket();
	}

	/// <inheritdoc/>
	public override AIBrain GetBrain() => new CombatAI(this);
}
