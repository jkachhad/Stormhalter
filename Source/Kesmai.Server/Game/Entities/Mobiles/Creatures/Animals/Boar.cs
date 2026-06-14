using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Boar : AnimalEntity
{
	public Boar()
	{
		Name = "boar";
		Body = 39;

		Alignment = Alignment.Chaotic;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
	}

	public override int GetNearbySound() => 12;
	public override int GetAttackSound() => 24;
	public override int GetDeathSound() => 36;
		
	public override ItemEntity OnCorpseTanned()
	{
		return new LeatherJacket();
	}

	/// <inheritdoc/>
	public override AIBrain GetBrain() => new CombatAI(this);
}
