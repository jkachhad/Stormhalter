using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Ninja : CreatureEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Ninja"/> class.
		/// </summary>
		public Ninja()
		{
			Name = "ninja";
			Body = 81;

			Alignment = Alignment.Chaotic;
		}

		/// <inheritdoc/>
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
		public override int GetDeathSound() => 79;
		public override int GetNearbySound() => 0;
		public override int GetAttackSound() => 0;
	}
}