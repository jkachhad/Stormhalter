using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class ElderTroll : CreatureEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ElderTroll"/> class.
		/// </summary>
		public ElderTroll()
		{
			Name = "elder.troll";
			Body = 79;

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
		public override int GetDeathSound() => 109;
		public override int GetNearbySound() => 95;
		public override int GetAttackSound() => 102;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new TrollVest();
		}
	}
}