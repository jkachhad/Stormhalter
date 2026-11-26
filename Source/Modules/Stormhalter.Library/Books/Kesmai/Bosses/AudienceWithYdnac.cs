using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class AudienceWithYdnac : PublishedBook
{
	public AudienceWithYdnac() : base("stormhalter.audience-with-ydnac",
		new Page(
			new CompositePageElement(new BodyPageElement(48), new BodyPageElement(5)),
			new ParagraphPageElement("Everyone in Kesmai knows Ydnac never leaves his niche, so I went to him. His djinn, Abesh, saw me first and shouted \"Master! An intruder!\" I thought that unfair. I was the one who knocked."),
			new ParagraphPageElement("Ydnac answered with a stun spell. While I stood there unable to move, I learned the rest: my good sword passed through him like smoke, and my friend's fire and ice did nothing but ruffle his robes.")
		),
		new Page(
			new ParagraphPageElement("We came back the next day with weapons that glow blue, and those bit him. Spells to blind, frighten or stun him are wasted. Mostly he stuns you, but now and then he casts death instead, and that is the spell to fear. Keep your healers busy, and keep someone on Abesh."),
			new ParagraphPageElement("When he fell, he hissed, \"The Yasnaki must pay...\" It is the only thing anyone has ever heard him say. Never trust a thaumaturge with one line.")
		))
	{
		Title = "An Audience with Ydnac";
		Author = "Sera Blackthorn";
		Description = "A famed adventurer's account of Ydnac, the evil thaumaturge, and the djinn who answers his door.";
		ItemId = 441;
		Quality = ItemQuality.Rare;
		Group = "Kesmai";
		Order = 3;
	}
}
