using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Sabertooth : AnimalEntity
{
	public Sabertooth()
	{
		Name = "sabertooth";
		Body = 59;
			
		Alignment = Alignment.Chaotic;
		CanCharge = true;
	}

	protected override void OnCreate()
	{
		base.OnCreate();

		_brain = new CombatAI(this);
	}

	public override int GetNearbySound() => 347;
	public override int GetAttackSound() => 346;
	public override int GetDeathSound() => 348;

	public override ItemEntity OnCorpseTanned()
	{
		return new TigerJacket();
	}
}