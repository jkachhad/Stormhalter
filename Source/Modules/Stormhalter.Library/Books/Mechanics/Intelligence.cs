using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class SlowLearnerSpellbook : PublishedScroll
{
	public SlowLearnerSpellbook() : base("stormhalter.slow-learners-spellbook",
		new Page(
			new CompositePageElement(new TerrainPageElement(266), new BodyPageElement(188), new TerrainPageElement(266)),
			new ParagraphPageElement("In the land of Oakvael, there was a wizard named Magnus, known for his exceptional intelligence. His sharp mind let him master spells in mere days- an advantage he was always proud to mention. Magnus loved boasting about how quickly he learned, unlike most other wizards who took weeks or even months."),
			new ParagraphPageElement("One day, while cleaning his study, he found an old family spellbook titled \"Grimgar the Great's Guide to Wizarding.\" Eager to learn from his ancestor, Magnus opened it and began reading. But instead of advanced techniques, he found spells with painfully simple, clumsy instructions."),
			new ParagraphPageElement("Under \"Fireball,\" Grimgar had written, \"Imagine something hot.. real hot.. then wave your hands a lot. Keep waving 'til it works!\" The next spell, \"Levitate,\" read: \"Jump as high as you can and hope for the best.\""),
			new CompositePageElement(new FloorPageElement(135), new FloorPageElement(9), new FloorPageElement(214)),
			new ParagraphPageElement("Magnus groaned. \"How long did it take him to learn these?\" He skimmed through the book until he found a section called \"Wisdom Beyond Ages.\" Intrigued, he cast it, and a faint image of Grimgar appeared, grinning widely."),
			new CompositePageElement(new BodyPageElement(66)),
			new ParagraphPageElement("\"Ah, so you're reading my spellbook! Took me years to write it all down.\" Grimgar beamed. \"Years?\" Magnus asked in disbelief. Grimgar nodded. \"Aye! Some of 'em took months, but I stuck with it!\" Magnus snapped the book shut, muttering, \"If only he'd had more intelligence, might have saved himself decades.\""),
			new ParagraphPageElement("And with that, Magnus returned to his studies, grateful his intelligence would save him from ever following in Grimgar's slow footsteps.")
		))
	{
		Title = "A Slow Learner's Spellbook";
		Author = "Goldenrain";
		Description = "Missteps of a wizard who took years to learn what others mastered in days.";
		ItemId = 461;
	}
}
