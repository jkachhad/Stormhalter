using Kesmai.Server.Game;

namespace Kesmai.Server.Segments
{
	public partial class Blackburrow : Segment
	{
		public static bool Enabled = true;
		
		public Blackburrow(int index, SegmentCache cache, Facet facet) : base(index, cache, facet)
		{
		}
		
		protected override void OnSpawn()
		{
			if (!Enabled)
				return;

			Respawn();
		}
	}
}