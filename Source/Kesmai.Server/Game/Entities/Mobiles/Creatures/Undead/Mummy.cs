using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Mummy : CreatureEntity, IUndead
	{
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Piercing;
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Slashing;
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Bashing;
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Projectile;
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Poison;
		public override CreatureWeakness Weakness { get; set; } = CreatureWeakness.BlueGlowing;
		/// <summary>
		/// Initializes a new instance of the <see cref="Mummy"/> class.
		/// </summary>
		public Mummy()
		{
			Name = "mummy";
			Body = 146;

			Alignment = Alignment.Chaotic;
			DeathResistance = 100;
			IceProtection = 100;
			HideDetection = 100;
			StunProtection = 100;
			AddStatus(new BreatheWaterStatus(this));
			AddStatus(new BlindFearProtectionStatus(this));
			AddImmunity(typeof(StunSpell));
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
		public override int GetNearbySound() => 250;
		public override int GetAttackSound() => 251;
		public override int GetDeathSound() => 252;

		public override Corpse GetCorpse() => default(Corpse);
	}
}