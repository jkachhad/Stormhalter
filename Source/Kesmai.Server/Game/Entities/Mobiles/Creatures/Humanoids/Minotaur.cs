using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Minotaur : CreatureEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Minotaur"/> class.
		/// </summary>
		public Minotaur()
		{
			Name = "minotaur";
			Body = 52;

			Alignment = Alignment.Chaotic;

			CanCharge = true;
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
		public override int GetDeathSound() => 110;
		public override int GetNearbySound() => 96;
		public override int GetAttackSound() => 103;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new LeatherArmor();
		}
	}
}
