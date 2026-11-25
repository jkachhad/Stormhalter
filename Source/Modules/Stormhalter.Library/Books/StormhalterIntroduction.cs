using System.Drawing;
using System.Collections.Generic;
using Kesmai.Server;
using Kesmai.Server.Engines.Gumps;
using Kesmai.Server.Engines.Library;
using Image = Kesmai.Server.Engines.Gumps.Image;

namespace Kesmai.Publications;

public class StormhalterIntroduction : PublishedBook
{
	public StormhalterIntroduction() : base("stormhalter.introduction",
		/* a composite page allows ui elements to be laid out in any manner. */
		new CompositePage(() =>
		{
		    var list = new List<GumpControl>()
		    {
			    /* show a centered image. */
			    new Image()
			    {
				    Texture = @"UI/Textures/Books/Image-Round-Dragon",
				    Width = 225, Height = 225,
				    HorizontalAlignment = HorizontalAlignment.Center,
			    },
		    
			    /* display the tile and author under. */
			    new TextBlock()
			    {
				    Text = "Stormhalter",
				    HorizontalAlignment = HorizontalAlignment.Center,
				
				    FontSize = 36, Font = "King",

				    Margin = new Rectangle(0, 30, 0, 0),
				},
			    new TextBlock()
			    {
				    Text = "Written by Luxx",
				    HorizontalAlignment = HorizontalAlignment.Center,
				
				    FontSize = 16, Font = "RobotoBold",
			    
				    Margin = new Rectangle(0, 20, 0, 0)
			    },
		    };

		    return list;
		}),
	
		/* a new page with more structured elements. */
		new Page(
			/* paragraphs are laid out and wrapped. */
			new ParagraphPageElement("Stormhalter is a passion project that began as a remake of Legends of Kesmai, with the goal of building a singular community. Expanding on the original game, it is a persistent world that will be updated and expand over time. Efforts are directed on creating a game that is both nostalgic and new, with a focus on community and respectful gameplay."),
			new ParagraphPageElement("Our project aims to introduce new players and provide a welcoming environment, regardless of their experience with Island of Kesmai, Legends of Kesmai, or Lands of Kesmai. If you are a veteran player, you will find a familiar experience with new content and challenges."),
			new ParagraphPageElement("The game is built by the community and we welcome feedback and suggestions. We are excited to see the game grow and evolve with the help of our players. Please join us on Discord and feel free to contribute to the Wiki.")
		))
	{
		Title = "Stormhalter";
		Author = "Luxx";
		ItemId = 411;
		Quality = ItemQuality.Epic;
		Group = "Basics";
		Order = 0;
	}
}
