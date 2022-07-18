using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Waft : CreatureEntity, IUndead
	{
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Piercing;
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Slashing;
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Bashing;
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Projectile;
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Poison;
		public override CreatureWeakness Weakness { get; set; } = CreatureWeakness.Silver;
		/// <summary>
		/// Initializes a new instance of the <see cref="Waft"/> class.
		/// </summary>
		public Waft()
		{
			Name = "waft";
			Body = 41;

			Alignment = Alignment.Chaotic;
			DeathResistance = 100;
			IceProtection = 100;
			HideDetection = 100;
			StunProtection = 100;
			VisibilityDistance = 2;
			CanFly = true;
			AddStatus(new BreatheWaterStatus(this));
			AddStatus(new BlindFearProtectionStatus(this));
			AddImmunity(typeof(StunSpell));
			AddImmunity(typeof(PoisonCloudSpell));
		}

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
		public override int GetDeathSound() => 5;
		public override int GetNearbySound() => 115;
		public override int GetAttackSound() => 122;

		public override Corpse GetCorpse() => default(Corpse);
	}
}