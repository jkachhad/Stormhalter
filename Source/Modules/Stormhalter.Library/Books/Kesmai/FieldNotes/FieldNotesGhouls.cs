using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class FieldNotesGhouls : PublishedScroll
{
	public FieldNotesGhouls() : base("stormhalter.field-notes-ghouls",
		new Page(
			new ParagraphPageElement("You will not see a ghoul coming. They creep through the deep halls and only show themselves when they are close enough to touch, which is also when they start touching. If you see dead people, it is already too late."),
			new CompositePageElement(new BodyPageElement(77)),
			new ParagraphPageElement("They scratch with their claws, and their bite poisons you. In the deepest halls they swim as well."),
			new ParagraphPageElement("Carry mugwort for the poison and keep your friends close. If something is next to you that wasn't there a moment ago, it's a ghoul.")
		))
	{
		Title = "Field Notes: Ghouls";
		Author = "Wren Hollowell";
		Description = "A naturalist's notes on ghouls, who are only seen up close.";
		ItemId = 453;
		Group = "Kesmai";
		Order = 4;
	}
}
