using System.IO;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public partial class Frog : AnimalEntity
	{
		public Frog()
		{
			Name = "frog";
			Body = 7;
			
			Alignment = Alignment.Chaotic;
			
			CanSwim = true;
			
			AddStatus(new BreatheWaterStatus(this));
		}

		public override int GetNearbySound() => 138;
		public override int GetAttackSound() => 157;
		public override int GetDeathSound() => 176;
	}
}