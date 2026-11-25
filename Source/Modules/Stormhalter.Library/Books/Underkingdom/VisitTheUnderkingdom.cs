using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class VisitTheUnderkingdom : PublishedScroll
{
	public VisitTheUnderkingdom() : base("stormhalter.visit-the-underkingdom",
		new Page(
			new ParagraphPageElement("VISIT THE UNDERKINGDOM! Friendly kobolds! Wyverns in every color! A drake who wanders right into town! Act now!"),
			new CompositePageElement(new BodyPageElement(134), new BodyPageElement(20), new BodyPageElement(177)),
			new ParagraphPageElement("Getting here is easy. In Axe Glacier, climb up to Chaos Town, in the middle of the Chaos Temple, and use the star ringed in purple. Coming from Oakvael? Take the grey ring to the glacier first."),
			new CompositePageElement(new TerrainPageElement(330), new BodyPageElement(133)),
			new ParagraphPageElement("Please don't hit the kobolds. They're the nice ones. The Troll King is not: he keeps a princess in his jail and his treasure behind a locked door."),
			new ParagraphPageElement("Lucky for you, I sell keys. But wait, there's more: I also sell a second key, for when you lose the first one. Ask for Googog in the jungle.")
		))
	{
		Title = "Visit the Underkingdom!";
		Author = "Googog";
		Description = "A jungle merchant's flyer for a kingdom of friendly kobolds and unfriendly trolls.";
		ItemId = 461;
		Group = "Underkingdom";
		Order = 5;
	}
}
