using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class SandInEverything : PublishedScroll
{
	public SandInEverything() : base("stormhalter.sand-in-everything",
		new Page(
			new ParagraphPageElement("To whoever finds this: I have been somewhere so hot that the scorpions wear hats. The locals call it Leng."),
			new CompositePageElement(new FloorPageElement(603), new BodyPageElement(91)),
			new ParagraphPageElement("There is sand in my boots, my pack and my porridge. I don't like sand. It's coarse and rough and irritating, and it gets everywhere."),
			new ParagraphPageElement("I won't say how I got there. Some portals are best found by accident.")
		))
	{
		Title = "Sand in Everything";
		Author = "AseLaind";
		Description = "An explorer writes from somewhere very hot, and very sandy.";
		ItemId = 461;
		Group = "Leng";
		Order = 3;
	}
}
