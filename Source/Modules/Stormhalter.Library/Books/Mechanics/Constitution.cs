using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class PorridgeAndPastries : PublishedScroll
{
	public PorridgeAndPastries() : base("stormhalter.porridge-and-pastries",
		new Page(
			new CompositePageElement(new BodyPageElement(32), new BodyPageElement(151)),
			new ParagraphPageElement("Bram and Oswin trained together and leveled together. At each new level, Bram felt a rush of vigor. Oswin felt a faint tingle, mostly in one elbow."),
			new ParagraphPageElement("Bram was raised on porridge and long marches; Oswin on pastries, naps and second breakfast. Oswin died a great deal."),
			new CompositePageElement(new FloorPageElement(105), new TerrainPageElement(325), new BodyPageElement(66)),
			new ParagraphPageElement("Each time, he waited for the ghods to raise him, and each time they kept a little of his health, stamina and constitution as their fee."),
			new ParagraphPageElement("Constitution decides how many hit points you gain each level. If you die, let a friend resurrect you: the ghods charge, and friends don't.")
		))
	{
		Title = "Porridge and Pastries";
		Author = "Anonymous";
		Description = "Two friends level up together, but only one of them gets the hit points.";
		ItemId = 461;
	}
}
