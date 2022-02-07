using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public partial class Lurker : AnimalEntity
	{
		public Lurker()
		{
			Name = "lurker";
			Body = 161;

			Alignment = Alignment.Chaotic;

			AddStatus(new BreatheWaterStatus(this));
			CanSwim = true;
			CanWalk = false;
			VisibilityDistance = 0;
			CanCharge = true;
		}
		
		protected override void OnCreate()
		{
			base.OnCreate();

			if (_brain != null)
				return;

			_brain = new CombatAI(this);
		}

		public override int GetNearbySound() => 259;
		public override int GetAttackSound() => 260;
		public override int GetDeathSound() => 261;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new LeatherArmor();
		}
	}
}