using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Yeti : CreatureEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yeti"/> class.
		/// </summary>
		public Yeti()
		{
			Name = "yeti";
			Body = 67;

			Alignment = Alignment.Evil;
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
		public override int GetDeathSound() => 175;
		public override int GetNearbySound() => 137;
		public override int GetAttackSound() => 156;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new PolarBearJacket();
		}
	}
}