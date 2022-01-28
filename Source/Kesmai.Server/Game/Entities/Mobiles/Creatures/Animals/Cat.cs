using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Cat : AnimalEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Cat"/> class.
		/// </summary>
		public Cat()
		{
			Name = "cat";
			Body = 28;

			Alignment = Alignment.Lawful;
		}

		/// <inheritdoc/>
		protected override void OnLoad()
		{
			_brain = new CombatAI(this);

			base.OnLoad();
		}

		public override int GetNearbySound() => 140;
		public override int GetAttackSound() => 159;
		public override int GetDeathSound() => 178;
	}
}