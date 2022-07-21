using System.IO;
using System.Linq;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Spectre : CreatureEntity, IUndead
	{
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Piercing || CreatureImmunity.Slashing || CreatureImmunity.Bashing || CreatureImmunity.Projectile || CreatureImmunity.Poison || CreatureImmunity.Magic;
		public override CreatureWeakness Weakness { get; set; } = CreatureWeakness.BlueGlowing;
		/// <summary>
		/// Initializes a new instance of the <see cref="Spectre"/> class.
		/// </summary>
		public Spectre()
		{
			Name = "spectre";
			Body = 62;

			Alignment = Alignment.Chaotic;
			StunProtection = 100;
			VisibilityDistance = 0;
			HideDetection = 100;
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
		public override int GetDeathSound() => 4;
		public override int GetNearbySound() => 114;
		public override int GetAttackSound() => 121;

		public override Corpse GetCorpse() => default(Corpse);
	}
}