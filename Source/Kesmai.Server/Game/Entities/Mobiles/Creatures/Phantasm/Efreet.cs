using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Efreet : CreatureEntity, IPhantasm
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Efreet"/> class.
		/// </summary>
		public Efreet()
		{
			Name = "efreet";
			Body = 1;

			Alignment = Alignment.Lawful;

			Health = MaxHealth = 5;
		}

		protected override void OnLoad()
		{
			_brain = new CombatAI(this);

			base.OnLoad();
		}
		
		public override int GetNearbySound() => 130;
		public override int GetAttackSound() => 149;
		public override int GetDeathSound() => 168;

		public override Corpse GetCorpse() => default(Corpse);
	}
}
