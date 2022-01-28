using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Hyena : AnimalEntity
	{
		public Hyena()
		{
			Name = "hyena";
			Body = 27;

			Alignment = Alignment.Chaotic;
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			_brain = new CombatAI(this);
		}

		public override int GetNearbySound() => 143;
		public override int GetAttackSound() => 162;
		public override int GetDeathSound() => 181;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new HyenaJacket();
		}
	}
}