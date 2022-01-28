using System.IO;
using System.Linq;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Peacekeeper : Humanoid
	{
		public Peacekeeper()
		{
			Alignment = Alignment.Lawful;
			
			CanLoot = false;
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