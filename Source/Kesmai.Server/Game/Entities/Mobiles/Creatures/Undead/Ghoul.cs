using System.IO;
using System.Linq;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Ghoul : CreatureEntity, IUndead
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Ghoul"/> class.
		/// </summary>
		public Ghoul()
		{
			Name = "ghoul";
			Body = 77;

			Alignment = Alignment.Chaotic;
			/* https://www.themonstersknow.com/undead-tactics-ghouls-ghasts/ - Nightvision add */
			AddStatus(new NightVisionStatus(this));
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
		public override int GetNearbySound() => 116;
		public override int GetAttackSound() => 123;
		public override int GetDeathSound() => 6;

		public override Corpse GetCorpse() => default(Corpse);
	}
}