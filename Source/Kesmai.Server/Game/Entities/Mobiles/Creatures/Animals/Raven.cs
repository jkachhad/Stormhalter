using System.IO;

namespace Kesmai.Server.Game;

public partial class Raven : AnimalEntity
{
	public Raven()
	{
		Name = "raven";
		Body = 71;

		Alignment = Alignment.Chaotic;

		VisibilityDistance = 0;

		CanFly = true;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
	}

	public override int GetNearbySound() => 141;
	public override int GetAttackSound() => 160;
	public override int GetDeathSound() => 179;

	/// <inheritdoc/>
	public override AIBrain GetBrain() => new CombatAI(this);
}
