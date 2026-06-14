using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Tiger : AnimalEntity
{
	public Tiger()
	{
		Name = "tiger";
		Body = 36;

		Alignment = Alignment.Chaotic;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
	}

	public override int GetNearbySound() => 15;
	public override int GetAttackSound() => 27;
	public override int GetDeathSound() => 39;
		
	public override ItemEntity OnCorpseTanned()
	{
		return new TigerJacket();
	}

	/// <inheritdoc/>
	public override AIBrain GetBrain() => new CombatAI(this);
}
