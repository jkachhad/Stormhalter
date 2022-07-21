using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Vampire : CreatureEntity, IUndead
	{
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Piercing || CreatureImmunity.Slashing || CreatureImmunity.Bashing || CreatureImmunity.Projectile || CreatureImmunity.Poison;
		public override CreatureWeakness Weakness { get; set; } = CreatureWeakness.Silver;
		/// <summary>
		/// Initializes a new instance of the <see cref="Vampire"/> class.
		/// </summary>
		public Vampire()
		{
			Name = "vampire";
			Body = 80;
			CanLoot = false;
			Alignment = Alignment.Chaotic;
			IceProtection = 100;
			HideDetection = 100;
			StunProtection = 100;
			VisibilityDistance = 0;
			CanFly = true;
			AddStatus(new BreatheWaterStatus(this));
			AddStatus(new BlindFearProtectionStatus(this));
			AddImmunity(typeof(StunSpell));
			AddImmunity(typeof(DeathSpell));
			AddImmunity(typeof(PoisonCloudSpell));
		}

		/// <inheritdoc/>
		public override void OnSpawn()
		{
			base.OnSpawn();

			if (_brain != null)
				return;

			if (RightHand is ProjectileWeapon)
				_brain = new RangedAI(this);
			else
				_brain = new CombatAI(this);
		}

		/// <summary>
		/// Gets the death sound.
		/// </summary>
		public override int GetNearbySound() => 132;
		public override int GetAttackSound() => 151;
		public override int GetDeathSound() => 170;

		public override Corpse GetCorpse() => default(Corpse);
	}
}