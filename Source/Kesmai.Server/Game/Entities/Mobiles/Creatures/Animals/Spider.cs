using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Spider : AnimalEntity
	{
		public Spider()
		{
			Name = "spider";
			Body = 23;

			Alignment = Alignment.Chaotic;
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			_brain = new CombatAI(this);
		}

		public override int GetNearbySound() => 241;
		public override int GetAttackSound() => 242;
		public override int GetDeathSound() => 243;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new LeatherJacket();
		}
	}
}