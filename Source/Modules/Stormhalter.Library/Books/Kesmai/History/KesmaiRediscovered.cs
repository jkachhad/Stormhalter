using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class KesmaiRediscovered : PublishedBook
{
	public KesmaiRediscovered() : base("stormhalter.kesmai-rediscovered",
		new Page(
			new ParagraphPageElement("With the brotherhood gone, the pirates sailed off for safer waters and the fishing folk went after them. The forest swallowed the village, and for ages no ship came to Kesmai on purpose."),
			new ParagraphPageElement("Then came the Age of Exploration. The first settlers found a pleasant climate and very unpleasant wildlife, and the wildlife won the early rounds. The settlers kept coming anyway, cleared the woods, and built the town that stands there now.")
		),
		new Page(
			new ParagraphPageElement("I was among the first to go down into the dungeon. I came back with treasure, a great many bruises and the brotherhood's scrolls, which is how you know any of this. Word of the treasure spread, and adventurers have been arriving ever since."),
			new ParagraphPageElement("My little sister AseLaind came later, to map the portals the Ancients left behind. Her letters come from lands of forest, ice and fire. She boldly goes where no one has gone before, and she never writes often enough.")
		),
		new Page(
			new ParagraphPageElement("Many have followed us down, and many did not come back. Their swords are said to lie in Daisy's hoard, which grows with every hero who tries to empty it."),
			new ParagraphPageElement("If you go below, go armed, go with friends, and write down what you find. Somebody has to finish this history, and I would rather it wasn't me.")
		))
	{
		Title = "Kesmai Rediscovered";
		Author = "Zod the Mighty";
		Description = "The last of Zod the Mighty's histories: how Kesmai was settled again, and who went down first.";
		ItemId = 410;
		Quality = ItemQuality.Uncommon;
		Group = "Kesmai";
		Order = 2;
	}
}
