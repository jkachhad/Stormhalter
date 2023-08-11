using System;
using Kesmai.Server.Game;

namespace Kesmai.Server.Segments;

public partial class Annwn : Segment
{
	public static bool Enabled = true;

	public Annwn(int index, SegmentCache cache, Facet facet) : base(index, cache, facet)
	{
	}

	protected override void OnSpawn()
	{
		if (!Enabled)
			return;

		Respawn();
	}
}