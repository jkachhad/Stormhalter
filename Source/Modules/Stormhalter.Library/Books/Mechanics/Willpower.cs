using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class SlipperyWill : PublishedScroll
{
	public SlipperyWill() : base("stormhalter.slippery-will",
		new Page(
			new CompositePageElement(new BodyPageElement(49), new ItemPageElement(71), new BodyPageElement(93)),
			new ParagraphPageElement("A famous martial artist had beaten every drake in the land but one. He found it on the ice of Axe Glacier, and his crystal gauntlets soon had it on its last legs."),
			new CompositePageElement(new FloorPageElement(17), new ItemPageElement(131)),
			new ParagraphPageElement("Then the drake roared, and his nerve went. He leapt for a finishing jumpkick, his boots met the ice, and the ice won. He landed on his head, and the drake had lunch."),
			new CompositePageElement(new FloorPageElement(105), new TerrainPageElement(325), new TerrainPageElement(325), new BodyPageElement(66)),
			new ParagraphPageElement("Willpower keeps your nerve. It helps you shrug off fear, stuns and blindness, take less harm from death and lightning spells, and keep moving across ice, fire and webs. He had very little of it. He used to be an adventurer like you, until he took a patch of ice to the head.")
		))
	{
		Title = "Slippery Will";
		Author = "Goldenrain";
		Description = "Low willpower turns a sure victory into a slip on the ice.";
		ItemId = 461;
		Group = "Basics";
		Order = 1;
	}
}
