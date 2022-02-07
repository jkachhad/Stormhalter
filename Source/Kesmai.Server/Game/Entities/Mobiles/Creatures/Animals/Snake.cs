using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public partial class Snake : AnimalEntity
	{
		public Snake()
		{
			Name = "snake";
			Body = 53;

			Movement = 2;
			
			CanSwim = true;
			AddStatus(new BreatheWaterStatus(this));

			Alignment = Alignment.Chaotic;
		}

		protected override void OnLoad()
		{
			Attacks = new CreatureAttackCollection
			{
				{ new CreatureAttack(8, 5, 15, "The snake strikes you.") },
			};
			
			Blocks = new CreatureBlockCollection
			{
				new CreatureBlock(10, "the armor"),
			};
			
			_brain = new CombatAI(this);

			base.OnLoad();
		}
		
		public override int GetNearbySound() => 17;
		public override int GetAttackSound() => 29;
		public override int GetDeathSound() => 41;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new LeatherArmor();
		}
	}
}