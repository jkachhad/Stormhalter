using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class MindTheGap : PublishedScroll
{
	public MindTheGap() : base("stormhalter.mind-the-gap",
		new Page(
			new CompositePageElement(new TerrainPageElement(266), new ItemPageElement(131)),
			new ParagraphPageElement("Welcome to Oakvael! Our town is built high in the branches of a great oak, safe from the beasts below."),
			new ParagraphPageElement("For years our only trouble was the ground, which townsfolk kept meeting suddenly. So we made boots for walking on air, and founded this society."),
			new ParagraphPageElement("Peyton and Karin guard the portal and keep the wildlife off newcomers. Find the rope and climb up to town. Leave the griffins alone."),
			new CompositePageElement(new BodyPageElement(25)),
			new ParagraphPageElement("And mind the gap. The ground is further down than it looks.")
		))
	{
		Title = "Mind the Gap";
		Author = "The AirWalkers Society";
		Description = "A welcome from the people who live in Oakvael's great oak, and keep falling out of it.";
		ItemId = 461;
		Group = "Oakvael";
		Order = 5;
	}
}
