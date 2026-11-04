using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;

namespace Kesmai.WorldForge;

/// <summary>
/// Searches a whole segment for a name, number or bit of text: entities, spawners and what they spawn, every script,
/// the other named things (locations, subregions, components, templates, treasures, brushes), and the tiles of every
/// region. A number matches as a whole number, so 34 finds body 34 but not 347.
/// </summary>
public static class SegmentSearch
{
	public sealed record Hit(string Kind, string Title, string Detail, ISegmentObject Target, SegmentRegion Region = null, int X = 0, int Y = 0);

	public static List<Hit> Find(Segment segment, string query, int tileLimit = 500)
	{
		var hits = new List<Hit>();
		query = (query ?? "").Trim();

		if (query.Length == 0)
			return hits;

		var number = Regex.IsMatch(query, @"^\d+$") ? new Regex($@"(?<![\d_]){query}(?![\d_])") : null;

		bool Matches(string text) => !string.IsNullOrEmpty(text)
			&& (number?.IsMatch(text) ?? text.Contains(query, StringComparison.OrdinalIgnoreCase));

		void Scripts(IEnumerable<Script> scripts, string owner, ISegmentObject target)
		{
			foreach (var script in scripts.Where(s => Matches(s.Body)))
			{
				var lines = script.Body.Split('\n');
				var at = Array.FindIndex(lines, Matches);
				var count = lines.Count(Matches);

				hits.Add(new Hit("Script", $"{owner}: {script.Name}",
					$"line {at + 1}: {lines[at].Trim()}" + (count > 1 ? $"  (+{count - 1} more lines)" : ""), target));
			}
		}

		foreach (var entity in segment.Entities)
		{
			if (Matches(entity.Name) || Matches(entity.Group) || Matches(entity.Notes))
				hits.Add(new Hit("Entity", entity.Name, string.IsNullOrEmpty(entity.Group) ? "" : $"group {entity.Group}", entity));

			Scripts(entity.Scripts, $"entity {entity.Name}", entity);
		}

		foreach (var spawner in segment.Spawns.Location.Cast<SegmentSpawner>().Concat(segment.Spawns.Region))
		{
			var place = spawner switch
			{
				LocationSegmentSpawner location => $"region {location.Region} ({location.X}, {location.Y})",
				RegionSegmentSpawner region => $"region {region.Region}",
				_ => "",
			};

			if (Matches(spawner.Name))
				hits.Add(new Hit("Spawner", spawner.Name, place, spawner));

			foreach (var entry in spawner.Entries.Where(e => e.SegmentEntity is { } entity && Matches(entity.Name)))
				hits.Add(new Hit("Spawner", spawner.Name, $"spawns {entry.SegmentEntity.Name} ({entry.Minimum}-{entry.Maximum}), {place}", spawner));

			Scripts(spawner.Scripts, $"spawner {spawner.Name}", spawner);
		}

		IEnumerable<(string Kind, IEnumerable<ISegmentObject> Items)> named =
		[
			("Location", segment.Locations.Cast<ISegmentObject>()),
			("Subregion", segment.Subregions.Cast<ISegmentObject>()),
			("Component", segment.Components.Cast<ISegmentObject>()),
			("Template", segment.Templates.Cast<ISegmentObject>()),
			("Treasure", segment.Treasures.Cast<ISegmentObject>()),
			("Brush", segment.Brushes.Cast<ISegmentObject>()),
			("Region", segment.Regions.Cast<ISegmentObject>()),
		];

		foreach (var (kind, items) in named)
		foreach (var item in items.Where(i => Matches(i.Name)))
			hits.Add(new Hit(kind, item.Name, item is SegmentRegion region ? $"region {region.ID}" : "", item));

		// tiles: any component on them, by its type, name or values (a wall id, a destination, a segment component's name)
		var tiles = 0;
		var more = new Dictionary<SegmentRegion, int>();

		foreach (var region in segment.Regions)
		foreach (var tile in region.GetTiles())
		{
			var provider = tile.Providers.FirstOrDefault(p => Matches(TileReplace.Key(p)) || Matches(p.Name));

			if (provider is null)
				continue;

			if (tiles++ < tileLimit)
				hits.Add(new Hit("Tile", $"region {region.ID} ({tile.X}, {tile.Y})", TileReplace.Describe(provider), region, region, tile.X, tile.Y));
			else
				more[region] = more.GetValueOrDefault(region) + 1;
		}

		foreach (var (region, count) in more)
			hits.Add(new Hit("Tile", $"region {region.ID}", $"{count} more tiles match here (only the first {tileLimit} tiles are listed)", region));

		return hits;
	}
}
