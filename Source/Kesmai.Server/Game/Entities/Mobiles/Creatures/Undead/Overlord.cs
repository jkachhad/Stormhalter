using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Overlord : CreatureEntity, IUndead
	{
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
		public override int GetNearbySound() => 256;
		public override int GetAttackSound() => 257;
		public override int GetDeathSound() => 258;

		public override Corpse GetCorpse() => default(Corpse);
	}
}