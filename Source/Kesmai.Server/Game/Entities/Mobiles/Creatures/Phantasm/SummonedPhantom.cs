using System.IO;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public partial class SummonedPhantom : Phantom
	{
		public override bool CanOrderFollow => true;
		public override bool CanOrderAttack => true;
		public override bool CanOrderCarry => true;

		public SummonedPhantom()
		{
			Summoned = true;
			
			Health = MaxHealth = 250;
			BaseDodge = 24;
			Movement = 3;
			MagicProtection = 10;
			
			Attacks = new CreatureAttackCollection
			{
				{ new CreatureBasicAttack(14) },
			};

			Blocks = new CreatureBlockCollection
			{
				new CreatureBlock(12, "a hand"),
			};
			
			AddStatus(new NightVisionStatus(this));
			
			CanFly = true;
		}
		
		protected override void OnLoad()
		{
			base.OnLoad();
			
			_brain = new CombatAI(this);
		}
	}
}