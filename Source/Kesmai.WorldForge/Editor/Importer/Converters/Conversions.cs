using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kesmai.WorldForge.Editor
{
#if (CanImport)

	public static class Conversions
	{
		public static List<ConversionPass> Structural = new List<ConversionPass>();
		public static List<ConversionPass> Environmental = new List<ConversionPass>();
		public static List<ConversionPass> Magical = new List<ConversionPass>();

		public static List<ConversionPass> All = new List<ConversionPass>();

		static Conversions()
		{
			var groundNormal = new List<int>()
			{
				1, 2, 3, 4, 5, 6, 12, 13, 14, 15, 16, 18, 21, 23, 24, 89, 90, 91,
				115, 116, 175, 176, 178, 179, 182, 183, 184, 185, 186, 196,
				252, 264, 265, 277, 289,
				301, 325, 341, 369, 375, 387, 388, 393,
				406, 419, 445, 492, 493
			};

			var groundSlow = new List<int>()
			{
				18, 105, 106, 107, 108, 109, 110, 121, 122, 205,
			};

			var groundObstacles = new List<int>()
			{
			};

			var ice = new List<int>()
			{
				17, 20
			};

			var water = new List<int>()
			{
				22, 177, 180, /* 209, */ 374
			};

			var webs = new List<int>()
			{
				131
			};

			var ruins = new List<int>()
			{
				44, 45, 46, 170, 380, 398, 432, 465,
			};

			var walls = new List<int>()
			{
				25, 26, // Dungeon wall
				27, 28, // Town wall
				29, 30, // Temple wall
				31, 32, 33, 34, // Wall posts (upper left)

				104, 139, 140, 143, 168,

				145, 146, 149,
				151, 152, 155,
				157, 158, 159,
				193,
				210,
				223, 224, 225,
				258, 259, 262,
				271, 272, 273, 274, 275, 276,
				283, 284, 285, 286, 287, 288,
				295, 296, 297, 298, 299, 300,

				313,

				333, 334,
				335, 336,
				346, 347,
				355, 357, 359,
				362, 363, 366,
				371,
				381, 382,
				383, 384,
				407, 409,
				413, 414, 417,
				428, 429, 430, 431, 443,
				446, 447, 448,
				459, 460, 463,
				470, 471, 480,
				474, 475,
				482, 483,
				486, 487,
			};

			var obstructions = new List<int>()
			{
				19, 164, 266, 403,
			};

			var doors = new List<int>()
			{
				35, 36,
				62, 63, // Dungeon doors
				64, 65, // Town doors
				66, 67, // Temple doors
				229, 230,
				311, 312,
				378, 379,
				453, 454,
				494, 495,
				504, 505,
			};

			var counters = new List<int>()
			{
				86, 87, 96, 97, 248, 249,
			};

			var altars = new List<int>()
			{
				113, 138, 163, 169, 174,
				278, 290, 302, 314, 324, 327, 368
			};

			var trash = new List<int>()
			{
				88
			};

			var sky = new List<int>()
			{
				9,
			};

			var stairs = new List<int>()
			{
				123, 124, 125, 126, 127, 128, 129, 130,
				253, 254,
			};

			var shafts = new List<int>()
			{
				8, 181,
			};

			var ropes = new List<int>()
			{
				237, 238, // up
				241, // down

				// duplicate art, different offset
				255, 256, 257, // down
				512, 513, 514 // up
			};

			var portals = new List<int>()
			{
				315, 316, 337, 328, 329
			};

			var lockers = new List<int>()
			{
				119, 120
			};

			var trees = new List<int>()
			{
				99, 98, 100,
				/*101, 102, 103,*/
				199, 200, 201,
				247,
				370, 372,
			};

			var fire = new List<int>()
			{
				135,
			};

			Structural.Add(new CounterConversion(counters));
			Structural.Add(new AltarConversion(altars));
			Structural.Add(new TrashConversion(trash));

			Structural.Add(new GroundConversion(groundNormal, 1));
			Structural.Add(new GroundConversion(groundSlow, 2));
			Structural.Add(new GroundConversion(groundObstacles, 3));
			
			Structural.Add(new IceConversion(ice));
			Structural.Add(new WebConversion(webs));
			Structural.Add(new WaterConversion(water));

			Structural.Add(new RuinsConversion(ruins));
			Structural.Add(new WallConversion(walls));
			Structural.Add(new ObstructionConversion(obstructions));

			Structural.Add(new DoorConversion(doors));

			Environmental.Add(new LockersConversion(lockers));
			Environmental.Add(new TreeConversion(trees));

			All.AddRange(Structural);
			All.AddRange(Environmental);
			All.AddRange(Magical);
		}
	}
	
#endif
}
