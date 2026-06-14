using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Manticora : AnimalEntity
{
	public Manticora()
	{
		Name = "manticora";
		Body = 2;

		Alignment = Alignment.Chaotic;
		CanFly = true;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
	}

	public override int GetNearbySound() => 127;
	public override int GetAttackSound() => 146;
	public override int GetDeathSound() => 165;
		
	public override ItemEntity OnCorpseTanned()
	{
		return new ManticoraJacket();
	}

	/// <inheritdoc/>
	public override AIBrain GetBrain() => new CombatAI(this);
}
