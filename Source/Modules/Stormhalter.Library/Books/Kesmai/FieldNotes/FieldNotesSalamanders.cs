using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class FieldNotesSalamanders : PublishedScroll
{
	public FieldNotesSalamanders() : base("stormhalter.field-notes-salamanders",
		new Page(
			new ParagraphPageElement("The salamander is a lizard that never learned not to play with fire. It lights a wall of flame right where you are standing, then strolls through it, because fire can't hurt it. Some lizards just want to watch the world burn."),
			new CompositePageElement(new BodyPageElement(40)),
			new ParagraphPageElement("It claws, it lashes with its tail, and its skin smolders. Even its corpse won't burn. Every one I have found carried a fiery ruby, which may be why adventurers keep visiting."),
			new ParagraphPageElement("Its fire won't catch on water, so fight it with your feet wet. A tailor can make its scales into a vest that takes a little of the sting out of fire.")
		))
	{
		Title = "Field Notes: Salamanders";
		Author = "Wren Hollowell";
		Description = "A naturalist's notes on salamanders, who set fires and walk through them.";
		ItemId = 458;
		Group = "Kesmai";
		Order = 4;
	}
}
