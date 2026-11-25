using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class LetterFromBeyondThePortals : PublishedScroll
{
	public LetterFromBeyondThePortals() : base("stormhalter.letter-from-beyond-the-portals",
		new Page(
			new CompositePageElement(new TerrainPageElement(266), new TerrainPageElement(99), new TerrainPageElement(266)),
			new ParagraphPageElement("To whoever finds this: I have stepped through a portal and found Oakvael, the land of the mighty trees. The oaks are so tall that the clouds have to go around."),
			new ParagraphPageElement("Its people live high in the branches, and many of the finest trainers in the lands have settled among them. Below, griffins circle and bears wander, and the caverns are even more dangerous than those beneath Kesmai."),
			new CompositePageElement(new BodyPageElement(25), new BodyPageElement(55)),
			new ParagraphPageElement("I won't say where the portal is. The town council knows, and will tell you in its own way."),
			new ParagraphPageElement("If you follow me, bring friends. It's dangerous to go alone.")
		))
	{
		Title = "A Letter from Beyond the Portals";
		Author = "AseLaind";
		Description = "An explorer writes home from a land of giant oaks, somewhere beyond Kesmai's portals.";
		ItemId = 461;
		Group = "Oakvael";
		Order = 2;
	}
}
