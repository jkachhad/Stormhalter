using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class GreatestTrialOfReginald : PublishedScroll
{
	public GreatestTrialOfReginald() : base("stormhalter.greatest-trial-of-reginald",
		new Page(
			new CompositePageElement(new BodyPageElement(151), new FloorPageElement(164)),
			new ParagraphPageElement("In the frozen land of Axe Glacier, Reginald, the self-proclaimed \"strongest knight in all the lands,\" found himself standing before an anvil. This very anvil could forge mighty weapons- the returning axe and the silver great axe. \"No doubt, only a knight of my strength could manage this,\" he muttered, glancing around as if someone might overhear and spread the word."),
			new CompositePageElement(new ItemPageElement(77), new ItemPageElement(125)),
			new ParagraphPageElement("He crouched down, gripped the anvil, and pulled with all his strength. The anvil didn't move an inch. Confused, he tried again, groaning louder this time for dramatic effect. Still nothing."),
			new CompositePageElement(new BodyPageElement(56)),
			new ParagraphPageElement("As he stood, baffled, an ice lizard slithered nearby, observing him. The creature let out a low, mocking snarl. Reginald scowled, drawing his sword. One quick swipe, and the lizard was done."),
			new CompositePageElement(new ItemPageElement(2), new ItemPageElement(43), new ItemPageElement(53)),
			new ParagraphPageElement("To his surprise, a small ring gleamed on the creature's claw. With a shrug, he slipped it on- and instantly felt a rush of strength. Testing his new power, he returned to the anvil, gripping it once more. This time, it lifted with ease. \"See?\" he grinned, even though no one was around. \"Strongest knight, as always.\" he shouted in the direction of the dead lizard."),
			new ParagraphPageElement("And with that, he hauled the anvil back to Vulcan, quietly grateful the lizard wouldn't live to tell the tale of his brief struggle.")
		))
	{
		Title = "The Greatest Trial of Reginald";
		Author = "Goldenrain";
		Description = "A tale of strength, anvil, and a very judgmental ice lizard.";
		ItemId = 461;
	}
}
