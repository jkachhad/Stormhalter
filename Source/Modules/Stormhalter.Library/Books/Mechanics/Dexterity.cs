using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class BrawlingWithScience : PublishedScroll
{
	public BrawlingWithScience() : base("stormhalter.punch-me-for-science",
		new Page(
			new CompositePageElement(new BodyPageElement(32), new TerrainPageElement(114), new BodyPageElement(49)),
			new ParagraphPageElement("Varek, a wiry fighter, found a platinum ring in the sand. Nobody could tell what it did, because nobody had thought to bring a thief to cast Identify."),
			new ParagraphPageElement("So they tested it scientifically, since the only difference between fooling around and science is writing it down. Varek put it on, and Tak, a martial artist with heavy fists, tried to kick him in the head."),
			new CompositePageElement(new ItemPageElement(19)),
			new ParagraphPageElement("Tak missed. Then he missed again. Varek ducked most of the kicks and landed more of his own punches than ever. Tak still connected now and then; science is never perfect."),
			new ParagraphPageElement("It was a ring of dexterity, which helps you hit and helps you dodge. The two parted with bruises and a deep respect for peer review.")
		))
	{
		Title = "Punch Me. For Science!";
		Author = "Goldenrain";
		Description = "Two brilliant warriors explore science by punching each other in the head.";
		ItemId = 461;
	}
}
