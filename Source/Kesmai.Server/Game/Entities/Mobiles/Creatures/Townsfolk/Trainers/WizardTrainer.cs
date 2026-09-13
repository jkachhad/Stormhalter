using System.IO;

namespace Kesmai.Server.Game;

public partial class WizardTrainer : SpellTrainer
{
	public override Profession Profession => Profession.Wizard;
		
	public WizardTrainer(IEntityProvider entityProvider, EntityDependencies entityDependencies)
		: base(entityProvider, entityDependencies)
	{
	}
}
