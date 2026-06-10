using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Scorpion : AnimalEntity
{
	public Scorpion()
	{
		Name = "scorpion";
		Body = 91;

		Alignment = Alignment.Chaotic;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
	}

	public override int GetNearbySound() => 11;
	public override int GetAttackSound() => 23;
	public override int GetDeathSound() => 35;

	/// <inheritdoc/>
	public override AIBrain GetBrain() => new CombatAI(this);
}
