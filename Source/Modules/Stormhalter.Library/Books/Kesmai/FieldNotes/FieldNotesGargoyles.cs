using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class FieldNotesGargoyles : PublishedScroll
{
	public FieldNotesGargoyles() : base("stormhalter.field-notes-gargoyles",
		new Page(
			new ParagraphPageElement("Gargoyles look like statues until they move, and then they look like statues that fly. Don't blink. Water and drops don't stop them, and their stone flesh turns aside many blows."),
			new CompositePageElement(new BodyPageElement(76)),
			new ParagraphPageElement("They claw at you and batter you with their wings. Some of them shrug off a little of every spell, too."),
			new ParagraphPageElement("Now and then a gargoyle carries a silver dagger. Keep it: skeletons hate silver.")
		))
	{
		Title = "Field Notes: Gargoyles";
		Author = "Wren Hollowell";
		Description = "A naturalist's notes on gargoyles, the statues that fly.";
		ItemId = 460;
		Group = "Kesmai";
		Order = 4;
	}
}
