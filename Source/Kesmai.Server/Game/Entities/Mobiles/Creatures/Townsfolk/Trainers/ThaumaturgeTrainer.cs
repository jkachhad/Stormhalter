using System.IO;

namespace Kesmai.Server.Game
{
	public partial class ThaumaturgeTrainer : SpellTrainer
	{
		public override Profession Profession => Profession.Thaumaturge;
		
		public ThaumaturgeTrainer() : base()
		{
		}
	}
}