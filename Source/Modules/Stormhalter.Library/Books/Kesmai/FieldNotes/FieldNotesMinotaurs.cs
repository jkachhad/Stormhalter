using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class FieldNotesMinotaurs : PublishedScroll
{
	public FieldNotesMinotaurs() : base("stormhalter.field-notes-minotaurs",
		new Page(
			new ParagraphPageElement("A minotaur doesn't walk up to you. It charges in like a wrecking ball, and the first you know of it is a long black iron rod."),
			new CompositePageElement(new BodyPageElement(52)),
			new ParagraphPageElement("It swings the rod, gores you with its horns, which leaves you dazed, and now and then tramples you. Webs don't slow it, and it sees in the dark."),
			new ParagraphPageElement("Fight it with friends who can pull you up. If one drops its iron rod, keep it: they are well made.")
		))
	{
		Title = "Field Notes: Minotaurs";
		Author = "Wren Hollowell";
		Description = "A naturalist's notes on minotaurs, who never walk when they can charge.";
		ItemId = 457;
		Group = "Kesmai";
		Order = 4;
	}
}
