using System.IO;
using System.Linq;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous;

namespace Kesmai.Server.Game
{
	public partial class Orc : CreatureEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Orc"/> class.
		/// </summary>
		public Orc()
		{
			Name = "orc";
			Body = 17;

			Alignment = Alignment.Chaotic;
		}

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
		public override int GetDeathSound() => 105;
		public override int GetNearbySound() => 91;
		public override int GetAttackSound() => 98;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new LeatherArmor();
		}
	}
}
