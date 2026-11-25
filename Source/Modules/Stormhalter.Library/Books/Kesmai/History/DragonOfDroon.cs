using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class DragonOfDroon : PublishedBook
{
	public DragonOfDroon() : base("stormhalter.dragon-of-droon",
		new Page(
			new ParagraphPageElement("The Brotherhood of Yasnac never forgot that Mu had banished them. At last their Lord Emperor found the means of revenge. With the brothers of the Inner Circle, he would call up the Dragon of Droon and send it on to the gardens of Mu's Imperial Palace."),
			new ParagraphPageElement("The dragon would appear for an instant in their own Inner Temple first. It was meant to be a very short visit.")
		),
		new Page(
			new CompositePageElement(new BodyPageElement(92)),
			new ParagraphPageElement("After days of dark ceremony the chanting rose, and the dragon came, roaring and fighting the spell. The strain was too much for the weakest brother of the circle. His heart burst, and without the whole circle the last incantation could not be spoken. They were going to need a bigger circle."),
			new ParagraphPageElement("The dragon broke free. One sweep of its claw crushed the Lord Emperor, and it flung the others across the temple.")
		),
		new Page(
			new ParagraphPageElement("It flew into the laboratories and ate most of the experiments, and the creatures that escaped turned on the brothers who had made them. Then it came back to the Inner Temple, made its bed on the brotherhood's treasures, and killed everything sent to move it, even demons conjured for the job."),
			new ParagraphPageElement("The brothers gave up the deep halls one by one, and the monsters took them. The last few wrote their story on leather scrolls before the dark found them, and some, it is said, fled through a portal and broke it behind them."),
			new ParagraphPageElement("The dragon is down there still. The locals call her Daisy.")
		))
	{
		Title = "The Dragon of Droon";
		Author = "Zod the Mighty";
		Description = "The second of Zod the Mighty's histories: how the brotherhood called up a dragon, and lost everything to it.";
		ItemId = 428;
		Quality = ItemQuality.Uncommon;
		Group = "Kesmai";
		Order = 2;
	}
}
