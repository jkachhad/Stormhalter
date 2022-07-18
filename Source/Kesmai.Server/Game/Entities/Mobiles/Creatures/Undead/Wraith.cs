using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Wraith : CreatureEntity, IUndead
	{
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Piercing;
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Slashing;
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Bashing;
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Projectile;
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Poison;
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Magic;
		public override CreatureWeakness Weakness { get; set; } = CreatureWeakness.Silver;
		/// <summary>
		/// Initializes a new instance of the <see cref="Wraith"/> class.
		/// </summary>
		public Wraith()
		{
			Name = "wraith";
			Body = 50;

			Alignment = Alignment.Chaotic;
			StunProtection = 100;
			VisibilityDistance = 0;
			HideDetection = 100;
			CanFly = true;
			AddStatus(new BreatheWaterStatus(this));
			AddStatus(new BlindFearProtectionStatus(this));
			AddImmunity(typeof(StunSpell));
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
		public override int GetDeathSound() => 2;
		public override int GetNearbySound() => 112;
		public override int GetAttackSound() => 119;

		public override Corpse GetCorpse() => default(Corpse);
	}
}