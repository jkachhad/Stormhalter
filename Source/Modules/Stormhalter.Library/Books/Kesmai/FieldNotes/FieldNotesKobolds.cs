using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class FieldNotesKobolds : PublishedScroll
{
	public FieldNotesKobolds() : base("stormhalter.field-notes-kobolds",
		new Page(
			new ParagraphPageElement("Kobolds are the first monsters most adventurers meet beneath Kesmai, and the first most adventurers beat. I once watched one try to frighten a rat. The rat won."),
			new CompositePageElement(new BodyPageElement(20)),
			new ParagraphPageElement("They roam the first halls below town in packs, swinging maces and wearing leather armor a size too big. One pack has moved into the old library, where they mostly chew the books."),
			new ParagraphPageElement("Kobolds are weak and have no tricks at all, so they are the place to learn how fighting works. Wax on, wax off. A tailor can turn their hides into leather armor.")
		))
	{
		Title = "Field Notes: Kobolds";
		Author = "Wren Hollowell";
		Description = "A naturalist's notes on kobolds, the smallest monsters beneath Kesmai.";
		ItemId = 456;
		Group = "Kesmai";
		Order = 4;
	}
}
