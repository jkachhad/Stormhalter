using System;
using System.Collections.Generic;
using System.Linq;

#if (Server)
using Color = System.Drawing.Color;
#else
using Color = Microsoft.Xna.Framework.Color;
#endif

#if Client
namespace Kesmai.Client
#elif Server
namespace Kesmai.Server
#else
namespace Kesmai.Shared
#endif
{ 
	public class ItemQuality
    {
	    private static Color FromArgb(byte a, byte r, byte g, byte b)
	    {
#if Server
			return Color.FromArgb(a, r, g, b);
#else
		    return Color.FromNonPremultiplied(r, g, b, a);
#endif
	    }
	    
	    /* Do not ever change the index values, can cause items to deserialize into wrong quality. */
	    public static ItemQuality Poor			= new (-1,	6301049,	"Poor",			FromArgb(0xFF, 0x9D, 0x9D, 0x9D));
		public static ItemQuality Common		= new (0,	6301050,	"Common",		FromArgb(0xFF, 0xFF, 0xFF, 0x00));
		public static ItemQuality Uncommon		= new (1,	6301051,	"Uncommon",		FromArgb(0xFF, 0x1E, 0xFF, 0x00));
		public static ItemQuality Rare			= new (2,	6301052,	"Rare",			FromArgb(0xFF, 0x00, 0x70, 0xDD));
		public static ItemQuality Epic			= new (3,	6301053,	"Epic",			FromArgb(0xFF, 0xA3, 0x35, 0xEE));
		public static ItemQuality Legendary		= new (4,	6301054,	"Legendary",	FromArgb(0xFF, 0xFF, 0x80, 0x00));
		public static ItemQuality Artifact		= new (5,	6301055,	"Artifact",		FromArgb(0xFF, 0xE6, 0xCC, 0x80));
		public static ItemQuality Mythical 		= new (6,	6301056,	"Mythical",		FromArgb(0xFF, 0xF9, 0x29, 0x07));

		public static Dictionary<int, ItemQuality> Qualities = new Dictionary<int, ItemQuality>()
		{
			[-1]	=	Poor,
			[0]		=	Common,
			[1]		=	Uncommon,
			[2]		=	Rare,
			[3]		=	Epic,
			[4]		=	Legendary,
			[5]		=	Artifact,
			[6]		=	Mythical,
		};

		public static ItemQuality GetQuality(int value)
		{
			if (Qualities.TryGetValue(value, out var quality))
				return quality;

			return ItemQuality.Poor;
		}
		
		public static ItemQuality GetQuality(string value) 
			=> Qualities.Values.FirstOrDefault(q => q.Name.Matches(value)) ?? ItemQuality.Poor;

		public static bool operator <(ItemQuality a, ItemQuality b)
		{
			return a.Value < b.Value;
		}

		public static bool operator >(ItemQuality a, ItemQuality b)
		{
			return a.Value > b.Value;
		}

		public static bool operator <=(ItemQuality a, ItemQuality b)
		{
			return a.Value <= b.Value;
		}

		public static bool operator >=(ItemQuality a, ItemQuality b)
		{
			return a.Value >= b.Value;
		}

		public static implicit operator int(ItemQuality quality)
		{
			return quality.Value;
		}

		public static implicit operator ItemQuality(int value)
		{
			return GetQuality(value);
		}

		private int _value;
		private int _localization;
		private string _name;
		private Color _color;

		public int Localization => _localization;

		public string Name => _name;

		public Color Color => _color;

		public int Value => _value;

		public ItemQuality(int value, int localization, string name, Color color)
		{
			_value = value;

			_localization = localization;
			_name = name;
			
			_color = color;
		}

		public override string ToString() => _name;
    }			
}
