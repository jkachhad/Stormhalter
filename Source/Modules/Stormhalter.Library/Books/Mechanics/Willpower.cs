using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class SlipperyWill : PublishedScroll
{
	public SlipperyWill() : base("stormhalter.slippery-will",
		new Page(
			new CompositePageElement(new BodyPageElement(49), new ItemPageElement(71), new BodyPageElement(93)),
			new ParagraphPageElement("A martial artist of great renown had bested every drake in the land, save one. It made him anxious that it was his last challenge. Deep in the frozen wastes of Axe Glacier, he faced the drake. His crystal gauntlets shimmering with a blue glow as his strikes drove the beast to the brink of defeat. Victory seemed certain, for the beast was near death."),
			new CompositePageElement(new FloorPageElement(17), new ItemPageElement(131)),
			new ParagraphPageElement("But as he prepared to end the battle with a daring jumpkick, it hit for little damage. Fate turned against him and his boots skidded on the slick ice. Yet, even that wasn't the cause of his undoing. It was his anxiety- a lack of conference- but most probably low willpower, which left him vulnerable to the icy dread radiating from the drake and ensured his clumsy fall."),
			new ParagraphPageElement("He hit the ice hard, the sharp crack of his skull muffled by the chilling wind. Before he could rise, he heard the sound of teeth gnashing, then the sharp agony of the drake feasting."),
			new CompositePageElement(new FloorPageElement(105), new TerrainPageElement(325), new TerrainPageElement(325), new BodyPageElement(66)),
			new ParagraphPageElement("Suddenly, his vision blurred, and a golden aura enveloped him. The drake faded, replaced by a narrow, glowing path stretching into the unknown.")
		))
	{
		Title = "Slippery Will";
		Author = "Goldenrain";
		Description = "Lack of willpower turns victory turns to defeat.";
		ItemId = 461;
	}
}
