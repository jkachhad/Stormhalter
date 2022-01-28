using System.IO;

using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous;

namespace Kesmai.Server.Game
{
	public partial class Swordmaster : CreatureEntity, IUndead
	{
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Piercing;
		public override CreatureWeakness Weakness { get; set; } = CreatureWeakness.Silver;

		/// <summary>
		/// Initializes a new instance of the <see cref="Swordmaster"/> class.
		/// </summary>
		public Swordmaster()
		{
			Name = "swordmaster";
			Body = 163;

			Alignment = Alignment.Chaotic;
		}
		
		public override void OnSpawn()
		{
			base.OnSpawn();

			if (RightHand is ProjectileWeapon)
				_brain = new RangedAI(this);
			else
				_brain = new CombatAI(this);
		}

		/// <summary>
		/// Gets the death sound.
		/// </summary>
		public override int GetNearbySound() => 259;
		public override int GetAttackSound() => 260;
		public override int GetDeathSound() => 261;

		public override Corpse GetCorpse() => default(Corpse);
	}
}
