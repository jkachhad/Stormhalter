using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class BrawlingWithScience : PublishedScroll
{
	public BrawlingWithScience() : base("stormhalter.punch-me-for-science",
		new Page(
			new CompositePageElement(new BodyPageElement(32), new TerrainPageElement(114), new BodyPageElement(49)),
			new ParagraphPageElement("It began when one of them, Varek, a wiry fighter, found something glinting in the sand-- a platinum ring, etched with faintly shimmering runes. He shared a look with Tak, a martial-artist with heavy fists."),
			new ParagraphPageElement("Neither knew what the ring did; they also failed to bring a thief in their party, because a simple Identify spell could tell-- I digress! They decided to test it. Tak would use a newly learned circular kicking technique against Varek's head, and Varek would-- he would-- he had no skills, he was just a fighter."),
			new ParagraphPageElement("They each took turns slipping the ring onto their fingers, but there was no magical chime or hum to signal their newly gained power."),
			new CompositePageElement(new ItemPageElement(19)),
			new ParagraphPageElement("As Varek wore the ring, his movements grew more precise. He now dodged Tak's attacks with ease, and Tak, despite his usual dexterity, struggled to even graze him. It didn't make Varek invincible; Tak landed a few hits, but they were fewer and farther between."),
			new ParagraphPageElement("Varek seemed to notice, too, his confidence building as he found openings he hadn't seen before. Tak was visibly frustrated, fighting a shadow that danced just out of reach."),
			new ParagraphPageElement("At the end of the match, Varek slipped the ring off, handing it to Tak. The two exchanged respectful nods, still unsure of the ring's power, but only bound by the thrill of roundhouse kicks for science.")
		))
	{
		Title = "Punch Me. For Science!";
		Author = "Goldenrain";
		Description = "Two brilliant warriors explore science by punching each other in the head.";
		ItemId = 461;
	}
}
