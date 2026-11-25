using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class StillWaitingForMyAnvil : PublishedScroll
{
	public StillWaitingForMyAnvil() : base("stormhalter.still-waiting-for-my-anvil",
		new Page(
			new ParagraphPageElement("To whoever finds this in Oakvael: I am Vulcan, smith of Axe Glacier. A knight named Reginald once promised to carry me an anvil. I am still waiting."),
			new CompositePageElement(new TerrainPageElement(164), new TerrainPageElement(202), new BodyPageElement(67)),
			new ParagraphPageElement("If you see him, point him to the portal ringed in grey, beside the one home to Kesmai. You may come too. Bring a coat."),
			new CompositePageElement(new TerrainPageElement(329), new FloorPageElement(615)),
			new ParagraphPageElement("The glacier is the coldest place in the known lands. The ice is slippery, the yeti is grumpy, and the drake is worse."),
			new ParagraphPageElement("Silvermoon and I forge fine weapons for anyone who brings us the right materials. As for Reginald: I will not let it go.")
		))
	{
		Title = "Still Waiting for My Anvil";
		Author = "Vulcan";
		Description = "A smith of Axe Glacier would like his anvil, and gives directions to the coldest place in the lands.";
		ItemId = 461;
		Group = "Axe Glacier";
		Order = 4;
	}
}
