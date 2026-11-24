using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class MissingPrincessIanta : PublishedScroll
{
	public MissingPrincessIanta() : base("stormhalter.missing-princess-ianta",
		new Page(
			new ParagraphPageElement("MISSING: Princess Ianta of Leng, last seen admiring a waterfall. She answers to her name, and sometimes to \"Ianta, come home this instant.\""),
			new ParagraphPageElement("Travelers in Oakvael: beside the portal home to Kesmai, ringed in blue, stands one ringed in yellow. It leads to Leng."),
			new CompositePageElement(new FloorPageElement(603), new TerrainPageElement(315), new BodyPageElement(86)),
			new ParagraphPageElement("Leng is a land of sand. Sandwyrms hunt beneath it, so walk without rhythm. A troll guards our bridge, so walk around him."),
			new ParagraphPageElement("Bring Ianta home to her mother and you will have the thanks of the Queen. Track sand across her floors and you will not.")
		))
	{
		Title = "Missing: Princess Ianta";
		Author = "The Queen of Leng";
		Description = "A royal plea for a wandering princess, with directions to a land of sand.";
		ItemId = 461;
	}
}
