using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Duck : AnimalEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Duck"/> class.
		/// </summary>
		public Duck()
		{
			Name = "duck";
			Body = 22;

			Alignment = Alignment.Lawful;
			CanFly = true;
		}

		/// <inheritdoc/>
		protected override void OnLoad()
		{
			_brain = new CombatAI(this);

			base.OnLoad();
		}

		public override int GetNearbySound() => 16;
		public override int GetAttackSound() => 28;
		public override int GetDeathSound() => 40;
	}
}