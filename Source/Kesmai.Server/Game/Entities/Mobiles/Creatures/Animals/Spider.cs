using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Spider : AnimalEntity
{
	public Spider()
	{
		Name = "spider";
		Body = 23;

		Alignment = Alignment.Chaotic;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
	}

	public override int GetNearbySound() => 244;
	public override int GetAttackSound() => 245;
	public override int GetDeathSound() => 246;
		
	public override ItemEntity OnCorpseTanned()
	{
		return new LeatherJacket();
	}

	/// <inheritdoc/>
	public override AIBrain GetBrain() => new CombatAI(this);
}
