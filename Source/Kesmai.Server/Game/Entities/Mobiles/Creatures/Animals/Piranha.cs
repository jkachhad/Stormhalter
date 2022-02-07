using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public partial class Piranha : AnimalEntity
	{
		public Piranha()
		{
			Name = "piranha";
			Body = 13;

			Alignment = Alignment.Chaotic;
			AddStatus(new BreatheWaterStatus(this));
			CanSwim = true;
			CanWalk = false;
			VisibilityDistance = 0;
			CanCharge = true;
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			if (_brain != null)
				return;

			_brain = new CombatAI(this);
		}
		
		public override int GetNearbySound() => 2001;
		public override int GetDeathSound() => 182;
	}
}