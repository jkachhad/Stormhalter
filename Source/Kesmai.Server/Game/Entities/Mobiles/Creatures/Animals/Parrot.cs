using System.IO;

namespace Kesmai.Server.Game
{
	public partial class Parrot : AnimalEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Parrot"/> class.
		/// </summary>
		public Parrot()
		{
			Name = "parrot";
			Body = 73;

			Alignment = Alignment.Lawful;
			
			CanFly = true;
		}

		/// <inheritdoc/>
		protected override void OnLoad()
		{
			_brain = new CombatAI(this);

			base.OnLoad();
		}

		public override int GetNearbySound() => 280;
		public override int GetAttackSound() => 281;
		public override int GetDeathSound() => 282;
	}
}