using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class TrollWhoShakesTheEarth : PublishedBook
{
	public TrollWhoShakesTheEarth() : base("stormhalter.troll-who-shakes-the-earth",
		new Page(
			new CompositePageElement(new BodyPageElement(79)),
			new ParagraphPageElement("When the ground shudders under Kesmai and flames rise in the dungeon, the elder troll is back. He lives in the first halls beneath town, so he is the first great monster most of us meet, and the first to teach us humility."),
			new ParagraphPageElement("He greets visitors with \"You will not escape me!\" and wizards with \"Filthy Wizzzarrrd!\" Our wizard took it personally and set him on fire. He stood in the flames as if to say, this is fine. Her ice only annoyed him.")
		),
		new Page(
			new ParagraphPageElement("His spiked club knocks you flat and leaves your head ringing, so fight him in turns and keep a friend ready to pull you up. Don't count on water to save you: he swims, breathes it, and sees in the dark as well as you see at noon."),
			new ParagraphPageElement("When he finally fell, he cursed me: \"The bloodlands will claim you...\" The curse left me blind and reeling for a moment, so don't stand over him gloating. I learned that the hard way, while my friends picked up his ram skull.")
		))
	{
		Title = "The Troll Who Shakes the Earth";
		Author = "Brannoc the Bold";
		Description = "A famed adventurer's account of the elder troll in the halls just below town.";
		ItemId = 422;
		Quality = ItemQuality.Rare;
		Group = "Kesmai";
		Order = 3;
	}
}
