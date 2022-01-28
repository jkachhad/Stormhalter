using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Griffin : AnimalEntity
	{
		public Griffin()
		{
			Name = "griffin";
			Body = 25;

			Alignment = Alignment.Chaotic;

			CanFly = true;
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			_brain = new CombatAI(this);
		}

		public override int GetNearbySound() => 9;
		public override int GetAttackSound() => 21;
		public override int GetDeathSound() => 33;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new GriffinJacket();
		}
	}
}