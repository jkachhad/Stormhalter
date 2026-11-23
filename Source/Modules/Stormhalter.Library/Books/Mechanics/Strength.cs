using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class GreatestTrialOfReginald : PublishedScroll
{
	public GreatestTrialOfReginald() : base("stormhalter.greatest-trial-of-reginald",
		new Page(
			new CompositePageElement(new BodyPageElement(151), new FloorPageElement(164)),
			new ParagraphPageElement("Reginald, self-proclaimed strongest knight in all the lands, came to Axe Glacier to carry an anvil home to Vulcan. He gripped it, heaved, and groaned loudly for effect. The anvil did not move."),
			new CompositePageElement(new BodyPageElement(56)),
			new ParagraphPageElement("An ice lizard snickered, \"Do you even lift?\" One swing later it had stopped snickering, and Reginald found a ring of strength on its claw."),
			new CompositePageElement(new ItemPageElement(2), new ItemPageElement(43), new ItemPageElement(53)),
			new ParagraphPageElement("He slipped it on, and the anvil came up like a loaf of bread. \"Strongest knight, as always,\" he told the lizard, who was in no state to argue."),
			new ParagraphPageElement("Strength decides how much you can carry, and the heavier your load, the more each step tires you. Rings and spells of strength raise it, and a great deal of it makes your blows hurt more.")
		))
	{
		Title = "The Greatest Trial of Reginald";
		Author = "Goldenrain";
		Description = "A tale of strength, an anvil, and a very judgmental ice lizard.";
		ItemId = 461;
	}
}
