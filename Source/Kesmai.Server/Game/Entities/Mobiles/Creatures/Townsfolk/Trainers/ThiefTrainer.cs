using System;
using System.IO;
using System.Linq;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class ThiefTrainer : SpellTrainer
{
	public override Profession Profession => Profession.Thief;
		
	public ThiefTrainer(IEntityProvider entityProvider, EntityDependencies entityDependencies)
		: base(entityProvider, entityDependencies)
	{
	}
}
