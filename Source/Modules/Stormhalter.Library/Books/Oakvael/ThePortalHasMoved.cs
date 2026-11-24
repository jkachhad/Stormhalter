using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class ThePortalHasMoved : PublishedScroll
{
	public ThePortalHasMoved() : base("stormhalter.portal-has-moved",
		new Page(
			new ParagraphPageElement("NOTICE. The portal to Oakvael no longer stands in the ring of oaks east of town. The council apologizes to everyone who spent the morning chanting at a tree."),
			new ParagraphPageElement("It now lies deep beneath Kesmai, in the halls of the dragon the locals call Daisy. West of her lair, look for grass where no grass should grow, inside a ring of stone pillars."),
			new CompositePageElement(new BodyPageElement(92), new FloorPageElement(13), new TerrainPageElement(328)),
			new ParagraphPageElement("At its heart is a marble star in a green ring. Step onto it and use it."),
			new ParagraphPageElement("The council posted this notice in plain view, on the door of the dragon's lair, beneath a sign saying \"Beware of the Dragon.\" If you are reading it anywhere else, an orc has taken it down again.")
		))
	{
		Title = "The Portal Has Moved";
		Author = "The Kesmai Town Council";
		Description = "A notice from Kesmai's council about where the portal to Oakvael went.";
		ItemId = 461;
	}
}
