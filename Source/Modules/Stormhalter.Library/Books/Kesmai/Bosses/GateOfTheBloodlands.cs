using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class GateOfTheBloodlands : PublishedBook
{
	public GateOfTheBloodlands() : base("stormhalter.gate-of-the-bloodlands",
		new Page(
			new CompositePageElement(new BodyPageElement(63)),
			new ParagraphPageElement("In the deepest halls beneath Kesmai, a demon named Emondeus guards the way to the Bloodlands. He has one thing to say to visitors: \"You are not worthy of entry into the Bloodlands! DIE!\" A simple \"you shall not pass\" would have done."),
			new ParagraphPageElement("Some say he is one of the demons the Brotherhood of Yasnac conjured to drive out their dragon, and that he has been in a mood ever since he lost.")
		),
		new Page(
			new ParagraphPageElement("He flies, he charges, and his claws, his head and his horns all knock you down and leave you dazed. He casts, too: fireballs, ice storms and, worst of all, death. Fire and ice can't hurt him in return, nor poison, and spells to stun, blind or frighten him are wasted."),
			new ParagraphPageElement("Spread out, keep your healers busy, and hit him with everything else you have. With his last breath he swore, \"My brethren are coming!\" They haven't yet. I keep a sword under my pillow, just in case.")
		))
	{
		Title = "At the Gate of the Bloodlands";
		Author = "Thane Garrow";
		Description = "A famed adventurer's account of Emondeus, the demon who guards the way to the Bloodlands.";
		ItemId = 432;
		Quality = ItemQuality.Rare;
		Group = "Kesmai";
		Order = 3;
	}
}
