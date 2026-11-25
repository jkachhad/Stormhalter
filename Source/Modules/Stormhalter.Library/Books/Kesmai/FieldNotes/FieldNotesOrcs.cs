using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class FieldNotesOrcs : PublishedScroll
{
	public FieldNotesOrcs() : base("stormhalter.field-notes-orcs",
		new Page(
			new ParagraphPageElement("Orcs are everywhere: in the woods around town, in every hall below it, and in my dreams. In the first halls below town they shout \"Taag the gaz!\", which means \"Kill the intruder!\" I was the intruder. One does not simply walk past an orc."),
			new CompositePageElement(new BodyPageElement(17)),
			new ParagraphPageElement("Many orc bands bring archers who keep their distance. Deeper down they bring spellcasters too: some throw fireballs and don't mind fire themselves, and others tangle you in webs and pelt you with magic."),
			new ParagraphPageElement("Orcs run when a fight turns against them, so finish what you start, and deal with the archers first.")
		))
	{
		Title = "Field Notes: Orcs";
		Author = "Wren Hollowell";
		Description = "A naturalist's notes on orcs, who are everywhere.";
		ItemId = 451;
		Group = "Kesmai";
		Order = 4;
	}
}
