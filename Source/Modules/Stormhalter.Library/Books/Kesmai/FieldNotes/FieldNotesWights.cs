using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class FieldNotesWights : PublishedScroll
{
	public FieldNotesWights() : base("stormhalter.field-notes-wights",
		new Page(
			new ParagraphPageElement("Wights haunt the cemetery east of town and the halls below it. Some say a wight's touch drains your life away. It doesn't. A wight curses you instead, from across the room, with a staff in its hand and a grudge in its heart."),
			new CompositePageElement(new BodyPageElement(61)),
			new ParagraphPageElement("They see in the dark, and like everything undead they leave no body behind, so whatever they carried falls to the floor. A thaumaturge's spell to turn the undead hurts them. Who you gonna call?"),
			new ParagraphPageElement("A wight's curse reaches you from a distance, so keep healing ready and close in quickly.")
		))
	{
		Title = "Field Notes: Wights";
		Author = "Wren Hollowell";
		Description = "A naturalist's notes on wights, who curse rather than drain.";
		ItemId = 454;
		Group = "Kesmai";
		Order = 4;
	}
}
