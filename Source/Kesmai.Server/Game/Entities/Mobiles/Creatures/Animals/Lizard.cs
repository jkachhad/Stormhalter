using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Lizard : AnimalEntity
{
	public Lizard(IEntityProvider entityProvider, EntityDependencies entityDependencies) : base(entityProvider, entityDependencies)
	{
		Name = "lizard";
		Body = 56;

		Alignment = Alignment.Chaotic;

	}

	protected override void OnCreate()
	{
		base.OnCreate();

		if (_brain != null)
			return;

		_brain = new CombatAI(this);
	}

	public override int GetNearbySound() => 17;
	public override int GetAttackSound() => 29;
	public override int GetDeathSound() => 41;
		
	public override ItemEntity OnCorpseTanned()
	{
		return new LizardScales(_entityProvider, _entityDependencies);
	}
}
