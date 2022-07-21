using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Presence : CreatureEntity, IUndead
	{
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Piercing || CreatureImmunity.Slashing || CreatureImmunity.Bashing || CreatureImmunity.Projectile || CreatureImmunity.Poison;
		public override CreatureWeakness Weakness { get; set; } = CreatureWeakness.Silver;
		/// <summary>
		/// Initializes a new instance of the <see cref="Presence"/> class.
		/// </summary>
		public Presence()
		{
			Name = "presence";
			Body = 41;

			Alignment = Alignment.Chaotic;
			IceProtection = 100;
			HideDetection = 100;
			StunProtection = 100;
			VisibilityDistance = 1;
			CanFly = true;
			AddStatus(new BreatheWaterStatus(this));
			AddStatus(new BlindFearProtectionStatus(this));
			AddImmunity(typeof(StunSpell));
			AddImmunity(typeof(DeathSpell));
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
		public override int GetNearbySound() => 115;
		public override int GetAttackSound() => 122;
		public override int GetDeathSound() => 5;

		public override Corpse GetCorpse() => default(Corpse);
	}
}