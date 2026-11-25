using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class FieldNotesSkeletons : PublishedScroll
{
	public FieldNotesSkeletons() : base("stormhalter.field-notes-skeletons",
		new Page(
			new ParagraphPageElement("I once watched a hero stab a skeleton for an hour. His rapier went in between the ribs and out the other side, and the skeleton kept hitting him. Skeletons are bad to the bone, and bone is all there is."),
			new CompositePageElement(new BodyPageElement(51)),
			new ParagraphPageElement("Thrusting weapons, such as rapiers, daggers and spears, pass right through them unless the blade is silver. Swords that cut, maces, hammers and arrows all work. The joke is that most skeletons carry rapiers themselves."),
			new ParagraphPageElement("Some carry bows, and deeper down some cast spells. A skeleton leaves no body, so whatever it carried falls to the floor. A thaumaturge's spell to turn the undead hurts them.")
		))
	{
		Title = "Field Notes: Skeletons";
		Author = "Wren Hollowell";
		Description = "A naturalist's notes on skeletons, and the weapons that go straight through them.";
		ItemId = 452;
		Group = "Kesmai";
		Order = 4;
	}
}
