using System;
using System.Collections.Generic;
using Kesmai.Server.Engines.Quests;
using Kesmai.Server.Engines.Spawners;
using Kesmai.Server.Game;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Segments
{
	public partial class Oakvael : Segment
 	{
	    public static bool Enabled = true;

	    public Oakvael(int index, SegmentCache cache, Facet facet) : base(index, cache, facet)
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