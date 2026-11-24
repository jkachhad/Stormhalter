using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class TheYasnakiMustPay : PublishedScroll
{
	public TheYasnakiMustPay() : base("stormhalter.yasnaki-must-pay",
		new Page(
			new ParagraphPageElement("Kesmai's dragon was the Yasnaki's idea. They called it up to eat their enemies across the sea, and it ate them instead. Serves them right."),
			new CompositePageElement(new BodyPageElement(92), new BodyPageElement(192)),
			new ParagraphPageElement("The ones who ran fled through a door in the frozen land and broke it behind them. Cowards. The kobolds on the other side have mended it. Take the grey ring on the ice below Kesmai to Axe Glacier, climb to Chaos Town, and use the star ringed in purple. Their Overlord hides at the very bottom."),
			new CompositePageElement(new TerrainPageElement(329), new TerrainPageElement(330), new BodyPageElement(90)),
			new ParagraphPageElement("I would go myself, but I never leave my niche."),
			new CompositePageElement(new BodyPageElement(48)),
			new ParagraphPageElement("Revenge is a dish best served cold. Luckily, the way there is freezing. Give them my regards.")
		))
	{
		Title = "The Yasnaki Must Pay";
		Author = "Ydnac";
		Description = "An evil thaumaturge's grudge against the Yasnaki, and where to find them.";
		ItemId = 461;
	}
}
