using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Sandwyrm : AnimalEntity
	{
		public Sandwyrm()
		{
			Name = "sandwyrm";
			Body = 86;

			Alignment = Alignment.Chaotic;
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			if (_brain != null)
				return;

			_brain = new CombatAI(this);
		}

		public override int GetNearbySound() => 134;
		public override int GetAttackSound() => 153;
		public override int GetDeathSound() => 172;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new LeatherArmor();
		}
	}
}