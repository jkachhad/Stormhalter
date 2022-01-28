using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Wraith : CreatureEntity, IUndead
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Wraith"/> class.
		/// </summary>
		public Wraith()
		{
			Name = "wraith";
			Body = 50;

			Alignment = Alignment.Chaotic;
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
		public override int GetDeathSound() => 2;
		public override int GetNearbySound() => 112;
		public override int GetAttackSound() => 119;

		public override Corpse GetCorpse() => default(Corpse);
	}
}