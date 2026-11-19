using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class AWiseCheat : PublishedScroll
{
	public AWiseCheat() : base("stormhalter.wise-cheat",
		new Page(
			new CompositePageElement(new BodyPageElement(219)),
			new ParagraphPageElement("In the halls of Kesmai's library, a certain Thaum seemed to absorb knowledge faster than anyone else. While others spent days poring over texts, he would grasp the concepts in mere hours. His peers marveled at his intellect—though most regretted it when he started explaining every detail of ancient Kesmai history."),
			new ParagraphPageElement("His secret? A hidden stash of Wisdom Elixirs—potions that heightened his intellect and allowed him to process information with remarkable efficiency. Whenever he needed to decipher complex texts or understand ancient magic, he would drink one, and the knowledge would flow effortlessly into his mind."),
			new CompositePageElement(new ItemPageElement(274), new ItemPageElement(274), new ItemPageElement(274)),
			new ParagraphPageElement("Unfortunately for him, wisdom and charm weren't the same thing. His attempts at courting often turned into lengthy lectures about obscure historical treaties or the etymology of spell names. Most women fled long before he reached the footnotes.")
		))
	{
		Title = "A Wise Cheat";
		Author = "Goldenrain";
		Description = "A thaum learns quickly but bores everyone with endless history lectures.";
		ItemId = 461;
	}
}
