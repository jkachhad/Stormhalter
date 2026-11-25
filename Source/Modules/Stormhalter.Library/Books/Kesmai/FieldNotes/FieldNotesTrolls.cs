using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class FieldNotesTrolls : PublishedScroll
{
	public FieldNotesTrolls() : base("stormhalter.field-notes-trolls",
		new Page(
			new ParagraphPageElement("Everyone knows that trolls grow back whatever you cut off, and run from fire. Everyone is wrong, at least about Kesmai's trolls. I checked, at great personal expense. Myth busted."),
			new CompositePageElement(new BodyPageElement(64)),
			new ParagraphPageElement("Kesmai's trolls carry big swords, wear chainmail and hit very hard. They heal no faster than anything else, and fire burns them like anyone. The deeper you go, the tougher they get."),
			new ParagraphPageElement("Don't confuse them with the elder troll who shakes the earth just below town. He is a story of his own.")
		))
	{
		Title = "Field Notes: Trolls";
		Author = "Wren Hollowell";
		Description = "A naturalist's notes on trolls, and two things everyone gets wrong about them.";
		ItemId = 455;
		Group = "Kesmai";
		Order = 4;
	}
}
