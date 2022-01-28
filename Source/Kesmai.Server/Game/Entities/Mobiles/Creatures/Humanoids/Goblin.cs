using System.IO;

using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous;

namespace Kesmai.Server.Game
{
	public partial class Goblin : CreatureEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Goblin"/> class.
		/// </summary>
		public Goblin()
		{
			Name = "goblin";
			Body = 31;

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
		public override int GetDeathSound() => 104;
		public override int GetNearbySound() => 90;
		public override int GetAttackSound() => 97;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new LeatherArmor();
		}
	}
}