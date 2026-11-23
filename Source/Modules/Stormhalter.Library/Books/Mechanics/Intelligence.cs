using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class SlowLearnerSpellbook : PublishedScroll
{
	public SlowLearnerSpellbook() : base("stormhalter.slow-learners-spellbook",
		new Page(
			new CompositePageElement(new TerrainPageElement(266), new BodyPageElement(188), new TerrainPageElement(266)),
			new ParagraphPageElement("Magnus of Oakvael was a very intelligent wizard, and he mentioned it often. He mastered new spells in days, while his classmates took weeks."),
			new ParagraphPageElement("One day he found the spellbook of his ancestor, Grimgar the Great. Under Fireball it read: \"Think of something hot. Wave your hands. Keep waving.\" Under Levitate: \"Jump. Hope.\""),
			new CompositePageElement(new FloorPageElement(135), new FloorPageElement(9), new FloorPageElement(214)),
			new CompositePageElement(new BodyPageElement(66)),
			new ParagraphPageElement("Grimgar's ghost rose from the last page, beaming. \"Took me forty years to learn those!\" Magnus learned them all before supper."),
			new ParagraphPageElement("Intelligence decides how fast wizards and thieves learn magic, and how much mana they gain each level. It does nothing for anyone else, which Magnus finds very sad.")
		))
	{
		Title = "A Slow Learner's Spellbook";
		Author = "Goldenrain";
		Description = "Missteps of a wizard who took years to learn what others mastered in days.";
		ItemId = 461;
	}
}
