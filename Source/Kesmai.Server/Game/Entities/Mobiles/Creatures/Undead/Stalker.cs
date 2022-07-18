using System.IO;
using System.Linq;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{

	public partial class Stalker : CreatureEntity, IUndead
	{
	public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Piercing;
	public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Slashing;
	public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Bashing;
	public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Projectile;
	public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Poison;
	public override CreatureWeakness Weakness { get; set; } = CreatureWeakness.BlueGlowing;

		/// <summary>
		/// Initializes a new instance of the <see cref="Stalker"/> class.
		/// </summary>
		public Stalker()
		{
			Name = "stalker";
			Body = 57;

			Alignment = Alignment.Chaotic;
			DeathResistance = 100;
			IceProtection = 100;
			HideDetection = 100;
			StunProtection = 100;
			VisibilityDistance = 0;
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
			
			_brain = new CombatAI(this);
		}

		/// <summary>
		/// Gets the death sound.
		/// </summary>

		public override int GetNearbySound() => 125;
		public override int GetAttackSound() => 121;
		public override int GetDeathSound() => 182;

		public override Corpse GetCorpse() => default(Corpse);
	}
}