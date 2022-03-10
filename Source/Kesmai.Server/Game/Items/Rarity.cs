using System.Drawing;

namespace Kesmai.Server.Items
public static class Rarity
{
	public static string GetRarity(int rarity.Clamp(0,5))
	{
		var rareText = "Common";
		switch (rarity)
		{
			case 0:
				break;

			case 1:
				rareText = "Uncommon";
				break;
			
			case 2:
				rareText = "Rare";
				break;
			
			case 3:
				rareText = "Epic";
				break;

			case 4:
				rareText = "Legendary";
				break;

			case 5:
				rareText = "Mythical";
				break;
		}
		return rareText;
	}
	
	public static Color GetRarityColor(int rarity.Clamp(0,5))
	{		
		var hue = Color.Transparent;
		switch (rarity)
		{
			case 0:
				break;

			case 1:
				hue = Color.FromArgb(0x781eff00);
				break;
			
			case 2:
				hue = Color.FromArgb(0x780070dd);
				break;

			case 3:
				hue = Color.FromArgb(0x78a335ee);
				break;

			case 4:
				hue = Color.FromArgb(0x78ff8000);
				break;

			case 5:
				hue = Color.FromArgb(0x78dc143c);
				break;
		}
		return hue;
	}
}			
