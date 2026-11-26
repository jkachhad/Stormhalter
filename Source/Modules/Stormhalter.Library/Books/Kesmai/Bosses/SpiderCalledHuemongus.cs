using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class SpiderCalledHuemongus : PublishedBook
{
	public SpiderCalledHuemongus() : base("stormhalter.spider-called-huemongus",
		new Page(
			new CompositePageElement(new BodyPageElement(147)),
			new ParagraphPageElement("In the deepest halls beneath Kesmai lives a spider the size of a cottage. Its name is Huemongus, and it greets visitors with \"Kl'Dr'kk'iz,\" which I am fairly sure is not a greeting. Friendly neighborhood spider, it is not."),
			new ParagraphPageElement("It swims, it sees in the dark, and its bite is poisoned, so pack mugwort before you go.")
		),
		new Page(
			new ParagraphPageElement("Spells to stun, blind or frighten it do nothing at all. Our blows kept sliding off what I can only call a very hairy and moist carapace, a phrase I will never be able to forget."),
			new ParagraphPageElement("We won in the end by sending the poisoned back to drink their cures and keeping the healthy ones in front. It was worth the trouble. Huemongus had been sitting on a fortune in gold, and I still don't know where a spider keeps its purse.")
		))
	{
		Title = "The Spider Called Huemongus";
		Author = "Mira Quickfoot";
		Description = "A famed adventurer's account of Huemongus, the giant spider in the deepest halls beneath Kesmai.";
		ItemId = 433;
		Quality = ItemQuality.Rare;
		Group = "Kesmai";
		Order = 3;
	}
}
