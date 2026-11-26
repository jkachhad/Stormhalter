using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class GemFromDaisy : PublishedBook
{
	public GemFromDaisy() : base("stormhalter.gem-from-daisy",
		new Page(
			new CompositePageElement(new BodyPageElement(92)),
			new ParagraphPageElement("Every knight in training is sent, sooner or later, to take a gem from the dragon the locals call Daisy. When I reached her halls she looked me over and said, \"Did Cudyl send you, knight? The gem is mine!\" Everyone else gets \"Ahhh. Another adventurer has come to perish.\" I preferred mine."),
			new ParagraphPageElement("She flies, she charges across her hall before you can finish a prayer, and her breath is fire.")
		),
		new Page(
			new ParagraphPageElement("Her claws, her tail and her wings each knock you down and leave you dazed, so bring friends to pull you up. Don't waste fire or ice on her, nor poison, nor spells to blind, frighten or stun: she shrugs them all off, and her scales turn aside plenty of good blows. Steel, patience and a great deal of balm got us through."),
			new ParagraphPageElement("When she fell, the gem was waiting among her treasure, beside the swords of better knights than me. I took a bigger sword home as well. Size matters not, I am told, but it helps.")
		))
	{
		Title = "A Gem from Daisy";
		Author = "Sir Aldric";
		Description = "A famed knight's account of Daisy, the dragon deep beneath Kesmai, and the gem every knight is sent for.";
		ItemId = 424;
		Quality = ItemQuality.Rare;
		Group = "Kesmai";
		Order = 3;
	}
}
