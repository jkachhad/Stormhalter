using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Panda : AnimalEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Panda"/> class.
		/// </summary>
		public Panda()
		{
			Name = "panda";
			Body = 29;

			Alignment = Alignment.Lawful;
		}

		/// <inheritdoc/>
		protected override void OnLoad()
		{
			_brain = new CombatAI(this);

			base.OnLoad();
		}

		public override int GetNearbySound() => 18;
		public override int GetAttackSound() => 30;
		public override int GetDeathSound() => 42;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new BearJacket();
		}
	}
}