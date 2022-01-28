using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Boar : AnimalEntity
	{
		public Boar()
		{
			Name = "boar";
			Body = 39;

			Alignment = Alignment.Chaotic;
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			_brain = new CombatAI(this);
		}

		public override int GetNearbySound() => 12;
		public override int GetAttackSound() => 24;
		public override int GetDeathSound() => 36;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new LeatherJacket();
		}
	}
}