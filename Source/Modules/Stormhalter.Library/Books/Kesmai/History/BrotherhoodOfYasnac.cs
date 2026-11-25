using Kesmai.Server;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Publications;

public class BrotherhoodOfYasnac : PublishedBook
{
	public BrotherhoodOfYasnac() : base("stormhalter.brotherhood-of-yasnac",
		new Page(
			new ParagraphPageElement("I am Zod, called the Mighty, and I am better with an axe than a quill. But I was the first to go down into the dark beneath Kesmai, and I carried out the scrolls its dead left behind, so the telling falls to me."),
			new ParagraphPageElement("Long ago the Brotherhood of Yasnac were the most respected magicians in Mu. Then they took up the Black Arts, and the Grand Order of Thaumaturges banished them from Mu forever. They sailed to Kesmai, at the far western tip of the islands.")
		),
		new Page(
			new CompositePageElement(new ItemPageElement(308)),
			new ParagraphPageElement("Kesmai belonged to pirates then. The brothers bought their welcome with spells and charms for raiding ships, and the pirates, who had never been offered magic before, decided it was a pirate's life for them."),
			new ParagraphPageElement("The brothers kept a fishing village on the surface for show and dug beneath it. The chambers became catacombs, and the catacombs went deeper every year, because the brothers liked the dark a little more every year.")
		),
		new Page(
			new ParagraphPageElement("Down there they bred new creatures out of old ones, and out of the occasional visitor. Vapors from the swamps did the rest. Many of the beasts you will meet below are their handiwork, and most of them kept the family temper."),
			new ParagraphPageElement("The brotherhood grew rich and proud, and showed off its monsters to every villain who came calling. But it never forgave Mu. My next book tells what that grudge cost them.")
		))
	{
		Title = "The Brotherhood of Yasnac";
		Author = "Zod the Mighty";
		Description = "The first of Zod the Mighty's histories of Kesmai: the dark brotherhood who dug the dungeon.";
		ItemId = 429;
		Quality = ItemQuality.Uncommon;
		Group = "Kesmai";
		Order = 2;
	}
}
