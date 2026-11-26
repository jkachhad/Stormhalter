using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class FieldNotesWyverns : PublishedScroll
{
	public FieldNotesWyverns() : base("stormhalter.field-notes-wyverns",
		new Page(
			new ParagraphPageElement("The wyvern has wings, a tail and a mouthful of teeth, and no idea what the wings are for. I have watched a great many of them, and not one has ever flown. Wings? Where they're going, they don't need wings."),
			new CompositePageElement(new BodyPageElement(65)),
			new ParagraphPageElement("They hunt the upper halls of the dungeon, and the dark doesn't trouble them. A wyvern rakes with its claws, bites, and whips you with its tail, and its scales turn aside many blows."),
			new ParagraphPageElement("Wyverns can't fly, so they can only come at you on foot. A tailor can make their scales into a vest that takes a little off every kind of blow.")
		))
	{
		Title = "Field Notes: Wyverns";
		Author = "Wren Hollowell";
		Description = "A naturalist's notes on wyverns, and their wings, which don't work.";
		ItemId = 459;
		Group = "Kesmai";
		Order = 4;
	}
}
