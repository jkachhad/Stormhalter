using System;
using System.Collections.Generic;
using System.Linq;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge.Editor;
#if (CanImport)

public class WallConversion : ConversionPass
{
	private List<int> _targets;

	/// <summary>
	/// Initializes a new instance of the <see cref="WallConversion"/> class.
	/// </summary>
	public WallConversion(List<int> targets)
	{
		_targets = targets;
	}

	/// <inheritdoc />
	public void Process(ISegmentImporter importer, IImportedRegion convertibleRegion, SegmentRegion convertedRegion)
	{
		var matches = convertedRegion.GetTiles(tile => true);

		/* First Pass */
		foreach (var tile in matches)
		{
			var staticComponents = tile.GetComponents<StaticComponent>(
				component => _targets.Contains(component.Static)).ToList();

			if (staticComponents.Count() > 2)
				throw new Exception("Encountered multiple wall candidates.");
				
			foreach (var component in staticComponents)
			{
				tile.ReplaceComponent(component, new WallComponent(component.Static, GetDestroyedId(component.Static), GetRuinsId(component.Static)));
			}
		}
			
		/* Second Pass to catch corner walls as indestructible */
		foreach (var tile in matches)
		{
			if (tile.OfType<WallComponent>().Count() > 1)
			{
				var north = convertedRegion.GetTile(tile, Direction.North);
				var west = convertedRegion.GetTile(tile, Direction.West);

				var northIndestructible = north != null && north.OfType<WallComponent>().Any(wall => wall.IsIndestructible);
				var westIndestructible = west != null && west.OfType<WallComponent>().Any(wall => wall.IsIndestructible);

				if (northIndestructible || westIndestructible)
				{
					foreach (var wall in tile.OfType<WallComponent>())
						wall.IsIndestructible = true;

					tile.UpdateTerrain();
				}
			}
		}

		var pillars = new List<int>()
		{
			32, 33, 34, 143, 155, 159, 262, 355, 357, 359, 366, 407, 409, 417, 443, 463, 480
		};

		bool isPillar(WallComponent wall)
		{
			return pillars.Contains(wall.Wall);
		}
			
		/* Third Pass to catch pillars */
		foreach (var tile in matches)
		{
			if (tile.OfType<WallComponent>().All(isPillar))
			{
				var south = convertedRegion.GetTile(tile, Direction.South);
				var east = convertedRegion.GetTile(tile, Direction.East);

				var southIndestructible = south != null && south.OfType<WallComponent>().Any(wall => wall.IsIndestructible);
				var eastIndestructible = east != null && east.OfType<WallComponent>().Any(wall => wall.IsIndestructible);

				if (southIndestructible && eastIndestructible)
				{
					foreach (var wall in tile.OfType<WallComponent>())
						wall.IsIndestructible = true;

					tile.UpdateTerrain();
				}
			}
		}
	}

	public int GetDestroyedId(int wall)
	{
		if (wall >= 25 && wall <= 30)
			return 38 + (wall - 25);
		if (wall >= 32 && wall <= 34)
			return 0;
		if (wall >= 139 && wall <= 140)
			return wall + 2;
		if (wall >= 145 && wall <= 146)
			return wall + 2;
		if (wall >= 151 && wall <= 152)
			return wall + 2;
		if (wall >= 157 && wall <= 158)
			return wall + 3;
		if (wall >= 223 && wall <= 224)
			return wall + 3;
		if (wall >= 258 && wall <= 259)
			return wall + 2;
		if (wall >= 333 && wall <= 334)
			return 342 + (wall - 333);
		if (wall >= 335 && wall <= 336)
			return 344 + (wall - 335);
		if (wall >= 346 && wall <= 347)
			return 338 + (wall - 346);
			
		return 0;
	}

	public int GetRuinsId(int wall)
	{
		switch (wall)
		{
			case 25:
			case 26: return 44;
				
			case 27:
			case 28: return 45;
				
			case 29:
			case 30:
			case 139:
			case 140: 
			case 151:
			case 152: return 46;
				
			case 157:
			case 158: return 170;
		}
			
		return 0;
	}
}

#endif