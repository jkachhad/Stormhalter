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
			AddStatus(new BreatheWaterStatus(this));
			CanSwim = true;
		}
		
		public override bool CanHide()
		{
			return true;
		}
		
		public override int GetNearbySound() => 138;
		public override int GetAttackSound() => 157;
		public override int GetDeathSound() => 176;
	}
}