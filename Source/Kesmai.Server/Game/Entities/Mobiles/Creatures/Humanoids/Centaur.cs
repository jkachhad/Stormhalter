using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Centaur : CreatureEntity
	{
		public Centaur()
		{
			Name = "centaur";
			Body = 3;

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

		public override int GetNearbySound() => 136;
		public override int GetAttackSound() => 155;
		public override int GetDeathSound() => 174;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new LeatherArmor();
		}
	}
}