using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Rockwyrm : AnimalEntity
{
	public Rockwyrm()
	{
		Name = "rockwyrm";
		Body = 95;

		Alignment = Alignment.Chaotic;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
	}

	public override int GetNearbySound() => 126;
	public override int GetAttackSound() => 145;
	public override int GetDeathSound() => 164;

	/// <inheritdoc/>
	public override AIBrain GetBrain() => new CombatAI(this);
}
