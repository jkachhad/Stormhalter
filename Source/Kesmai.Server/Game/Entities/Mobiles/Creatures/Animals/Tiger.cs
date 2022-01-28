using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Tiger : AnimalEntity
	{
		public Tiger()
		{
			Name = "tiger";
			Body = 36;

			Alignment = Alignment.Chaotic;
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			_brain = new CombatAI(this);
		}

		public override int GetNearbySound() => 15;
		public override int GetAttackSound() => 27;
		public override int GetDeathSound() => 39;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new TigerJacket();
		}
	}
}