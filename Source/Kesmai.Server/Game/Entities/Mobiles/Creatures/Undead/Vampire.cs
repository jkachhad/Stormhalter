using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Vampire : CreatureEntity, IUndead
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Vampire"/> class.
		/// </summary>
		public Vampire()
		{
			Name = "vampire";
			Body = 80;
			CanLoot = false;
			Alignment = Alignment.Chaotic;
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