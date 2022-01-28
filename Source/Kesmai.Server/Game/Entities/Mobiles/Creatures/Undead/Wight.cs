using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public partial class Wight : CreatureEntity, IUndead
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Wight"/> class.
		/// </summary>
		public Wight()
		{
			Name = "wight";
			Body = 61;

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
		public override int GetDeathSound() => 3;
		public override int GetNearbySound() => 113;
		public override int GetAttackSound() => 120;

		public override Corpse GetCorpse() => default(Corpse);
	}
}