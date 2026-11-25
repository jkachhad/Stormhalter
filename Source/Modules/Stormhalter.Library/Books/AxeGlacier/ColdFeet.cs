using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class ColdFeet : PublishedScroll
{
	public ColdFeet() : base("stormhalter.cold-feet",
		new Page(
			new ParagraphPageElement("To whoever finds this: I have found a land so cold that my ink froze, so I am writing this by the fire with my toes in a soup pot. The locals call it Axe Glacier."),
			new CompositePageElement(new TerrainPageElement(202), new BodyPageElement(248)),
			new ParagraphPageElement("An innkeeper told me winter is coming. I asked when it leaves. He laughed for a long time."),
			new ParagraphPageElement("I won't say how I got there. Pack something warm, and don't trust the geese.")
		))
	{
		Title = "Cold Feet";
		Author = "AseLaind";
		Description = "An explorer writes from somewhere very cold, and very slippery.";
		ItemId = 461;
		Group = "Axe Glacier";
		Order = 7;
	}
}
