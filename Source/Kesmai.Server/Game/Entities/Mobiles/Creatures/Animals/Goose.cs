using System.IO;

namespace Kesmai.Server.Game
{
	public partial class Goose : AnimalEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Goose"/> class.
		/// </summary>
		public Goose()
		{
			Name = "goose";
			Body = 248;

			Alignment = Alignment.Chaotic;
			CanFly = true;
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			_brain = new CombatAI(this);
		}

		public override int GetNearbySound() => 16;
		public override int GetAttackSound() => 28;
		public override int GetDeathSound() => 40;
	}
}