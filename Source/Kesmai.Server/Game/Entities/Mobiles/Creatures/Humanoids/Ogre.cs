using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Ogre : CreatureEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Ogre"/> class.
		/// </summary>
		public Ogre()
		{
			Name = "ogre";
			Body = 11;

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
		public override int GetDeathSound() => 182;
		public override int GetNearbySound() => 144;
		public override int GetAttackSound() => 163;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new LeatherArmor();
		}
	}
}