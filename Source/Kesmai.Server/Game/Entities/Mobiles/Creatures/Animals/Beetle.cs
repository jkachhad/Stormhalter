using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Beetle : AnimalEntity
	{
		public Beetle()
		{
			Name = "beetle";
			Body = 43;

			Alignment = Alignment.Chaotic;
			
			VisibilityDistance = 0;

			CanFly = true;
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			_brain = new CombatAI(this);
		}

		public override int GetNearbySound() => 139;
		public override int GetAttackSound() => 158;
		public override int GetDeathSound() => 177;
	}
}