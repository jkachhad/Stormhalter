using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public partial class SeaSerpent : AnimalEntity
	{
		public SeaSerpent()
		{
			Name = "serpent";
			Body = 8;

			Alignment = Alignment.Chaotic;
			AddStatus(new BreatheWaterStatus(this));
			CanSwim = true;
			CanWalk = false;
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
			return new LeatherArmor();
		}
	}
}