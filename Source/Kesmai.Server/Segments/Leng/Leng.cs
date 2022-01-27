using System;
using System.Collections.Generic;
using Kesmai.Server.Engines.Spawners;
using Kesmai.Server.Game;
using Kesmai.Server.Miscellaneous;

namespace Kesmai.Server.Segments
{
	public partial class Leng : Segment
	{
		public static bool Enabled = true;
		
		public Leng(int index, SegmentCache cache, Facet facet) : base(index, cache, facet)
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