using System.IO;
using System.Linq;
using Kesmai.Server.Items;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public partial class Humanoid : CreatureEntity
	{
		public Humanoid()
		{
			Alignment = Alignment.Chaotic;
			CanSwim = true;
			AddStatus(new BreatheWaterStatus(this));
		}
		
		public override int GetDeathSound() => (IsFemale ? 83 : 79);
		public override int GetWarmSound() => (IsFemale ? 84 : 80);

		public override ItemEntity OnCorpseTanned()
		{
			return new LeatherJacket();
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
	}
}