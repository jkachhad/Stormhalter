using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Bat : AnimalEntity
	{
		public Bat()
		{
			Name = "bat";
			Body = 222;

			Alignment = Alignment.Chaotic;
			CanFly = true;
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			_brain = new CombatAI(this);
		}

		public override int GetNearbySound() => 271;
		public override int GetAttackSound() => 272;
		public override int GetDeathSound() => 273;

		public override ItemEntity OnCorpseTanned()
		{
			return new LeatherArmor();
		}
	}
}