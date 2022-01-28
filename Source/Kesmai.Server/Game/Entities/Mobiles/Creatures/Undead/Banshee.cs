using System.IO;
using System.Linq;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Banshee : CreatureEntity, IUndead
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Banshee"/> class.
		/// </summary>
		public Banshee()
		{
			Name = "banshee";
			Body = 12;

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
		public override int GetNearbySound() => 128;
		public override int GetAttackSound() => 147;
		public override int GetDeathSound() => 166;

		public override Corpse GetCorpse() => default(Corpse);
	}
}