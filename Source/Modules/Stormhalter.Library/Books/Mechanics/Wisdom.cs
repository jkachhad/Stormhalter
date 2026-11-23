using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class AWiseCheat : PublishedScroll
{
	public AWiseCheat() : base("stormhalter.wise-cheat",
		new Page(
			new CompositePageElement(new BodyPageElement(219)),
			new ParagraphPageElement("A certain thaum in Kesmai's library learned his spells in hours while his classmates took days. His secret was a stash of wisdom potions, each one making him a little wiser for good."),
			new CompositePageElement(new ItemPageElement(274), new ItemPageElement(274), new ItemPageElement(274)),
			new ParagraphPageElement("Wisdom made him a quick study and gave him more mana each time he gained a level. It did not make him charming. His dates tended to leave during his lecture on ancient treaties. \"One does not simply skip the footnotes,\" he would call after them."),
			new ParagraphPageElement("Wisdom is to thaumaturges what intelligence is to wizards and thieves: faster magic learning and more mana each level. It does nothing for conversation.")
		))
	{
		Title = "A Wise Cheat";
		Author = "Goldenrain";
		Description = "A thaum learns quickly but bores everyone with endless history lectures.";
		ItemId = 461;
	}
}
