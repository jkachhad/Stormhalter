using System.IO;
using System.Linq;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Spectre : CreatureEntity, IUndead
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Spectre"/> class.
		/// </summary>
		public Spectre()
		{
			Name = "spectre";
			Body = 62;

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
		public override int GetDeathSound() => 4;
		public override int GetNearbySound() => 114;
		public override int GetAttackSound() => 121;

		public override Corpse GetCorpse() => default(Corpse);
	}
}