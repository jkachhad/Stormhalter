using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Overlord : CreatureEntity, IUndead
	{
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Piercing || CreatureImmunity.Slashing || CreatureImmunity.Bashing || CreatureImmunity.Projectile || CreatureImmunity.Poison || CreatureImmunity.Magic;
		/// <summary>
		/// Initializes a new instance of the <see cref="Overlord"/> class.
		/// </summary>
		public Overlord()
		{
			Name = "overlord";
			Body = 90;

			Alignment = Alignment.Evil;
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
		public override int GetNearbySound() => 253;
		public override int GetAttackSound() => 254;
		public override int GetDeathSound() => 255;

		public override Corpse GetCorpse() => default(Corpse);
	}
}