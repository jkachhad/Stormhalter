using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class TrollGuard : CreatureEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TrollGuard"/> class.
		/// </summary>
		public TrollGuard()
		{
			Name = "troll.gaurd";
			Body = 148;

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
		public override int GetDeathSound() => 267;

		public override int GetNearbySound() => 265;
		public override int GetAttackSound() => 266;

		public override ItemEntity OnCorpseTanned()
		{
			return new LeatherArmor();
		}
	}
}