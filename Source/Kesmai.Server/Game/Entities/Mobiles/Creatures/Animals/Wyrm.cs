using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public partial class Wyrm : AnimalEntity
	{
		public Wyrm()
		{
			Name = "wyrm";
			Body = 6;

			Alignment = Alignment.Chaotic;

			CanSwim = true;
			AddStatus(new BreatheWaterStatus(this));
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