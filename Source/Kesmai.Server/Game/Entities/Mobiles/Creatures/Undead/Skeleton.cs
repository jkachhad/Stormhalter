using System.IO;

using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous;

namespace Kesmai.Server.Game
{
	public partial class Skeleton : CreatureEntity, IUndead
	{
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Piercing || CreatureImmunity.Poison;
		public override CreatureWeakness Weakness { get; set; } = CreatureWeakness.Silver;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Skeleton"/> class.
		/// </summary>
		public Skeleton()
		{
			Name = "skeleton";
			Body = 51;

			Alignment = Alignment.Chaotic;
			IceProtection = 100;
			HideDetection = 100;
			StunProtection = 100;

			AddStatus(new BreatheWaterStatus(this));
			AddStatus(new BlindFearProtectionStatus(this));
			AddImmunity(typeof(StunSpell));
			AddImmunity(typeof(DeathSpell));
			AddImmunity(typeof(PoisonCloudSpell));
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
		public override int GetDeathSound() => 1;
		public override int GetNearbySound() => 111;
		public override int GetAttackSound() => 118;

		public override Corpse GetCorpse() => default(Corpse);
	}
}
