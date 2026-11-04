using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge;

/// <summary>
/// Find and replace tiles: every placement of one component, in a selection or a whole region, swapped for another
/// (or taken out), in the same place in each tile's stack, as one undoable edit.
/// </summary>
public static class TileReplace
{
	/// <summary>What tells two placed components apart: the element a tile refers to the component with.</summary>
	public static string Key(IComponentProvider provider)
		=> provider.GetReferencingElement()?.ToString(SaveOptions.DisableFormatting) ?? provider.Name;

	/// <summary>A readable name: the provider's own, or its type and values (as "FloorComponent: ground 7").</summary>
	public static string Describe(IComponentProvider provider)
	{
		if (!string.IsNullOrWhiteSpace(provider.Name) && provider.Name != "(unknown)")
			return provider.Name;

		if (provider.GetReferencingElement() is not { } element)
			return provider.GetType().Name;

		var values = element.Elements().Where(e => !e.HasElements).Select(e => $"{e.Name.LocalName} {e.Value}");

		return $"{(string)element.Attribute("type") ?? element.Name.LocalName}: {string.Join(", ", values)}";
	}

	/// <summary>The tiles in the selection when it is in this region and not empty, otherwise the whole region's.</summary>
	public static List<SegmentTile> Tiles(SegmentRegion region, Selection selection)
	{
		if (selection is null || selection.Region != region || selection.SurfaceArea == 0)
			return region.GetTiles().ToList();

		var tiles = new List<SegmentTile>();
		var seen = new HashSet<(int, int)>();

		foreach (var rectangle in selection)
		for (var x = rectangle.Left; x < rectangle.Right; x++)
		for (var y = rectangle.Top; y < rectangle.Bottom; y++)
		{
			if (seen.Add((x, y)) && region.GetTile(x, y) is { } tile)
				tiles.Add(tile);
		}

		return tiles;
	}

	/// <summary>Each component placed on the tiles, with how many tiles have it, most used first.</summary>
	public static List<(string Key, string Name, int Count, IComponentProvider Sample)> Census(IEnumerable<SegmentTile> tiles)
	{
		var found = new Dictionary<string, (string Name, int Count, IComponentProvider Sample)>();

		foreach (var tile in tiles)
		foreach (var key in tile.Providers.Select(p => (Key: Key(p), Provider: p)).GroupBy(p => p.Key).Select(g => g.First()))
		{
			found[key.Key] = found.TryGetValue(key.Key, out var entry)
				? (entry.Name, entry.Count + 1, entry.Sample)
				: (Describe(key.Provider), 1, key.Provider);
		}

		return found.Select(f => (f.Key, f.Value.Name, f.Value.Count, f.Value.Sample))
			.OrderByDescending(f => f.Count).ThenBy(f => f.Name, StringComparer.OrdinalIgnoreCase).ToList();
	}

	/// <summary>
	/// Swaps every placement of the component <paramref name="findKey"/> on the tiles for <paramref name="replacement"/>,
	/// or takes it out when that is null. Each changed tile is captured in <paramref name="edit"/> first.
	/// </summary>
	/// <returns>How many tiles changed.</returns>
	public static int Replace(SegmentRegion region, IEnumerable<SegmentTile> tiles, string findKey, IComponentProvider replacement, MapEdit edit)
	{
		var changed = 0;

		foreach (var tile in tiles)
		{
			var matches = Enumerable.Range(0, tile.Providers.Count).Where(i => Key(tile.Providers[i]) == findKey).ToList();

			if (matches.Count == 0)
				continue;

			edit?.Capture(region, tile.X, tile.Y);

			// from the last, so the earlier indexes stay right; the new component takes the old one's place in the stack
			foreach (var index in Enumerable.Reverse(matches))
			{
				tile.Providers.RemoveAt(index);

				if (replacement is null)
					continue;

				var placed = new ObservableCollection<IComponentProvider>();
				replacement.AddComponent(placed);

				for (var i = 0; i < placed.Count; i++)
					tile.Providers.Insert(index + i, placed[i]);
			}

			tile.UpdateTerrain();
			changed++;
		}

		return changed;
	}
}
