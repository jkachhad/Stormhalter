using System.Collections.Generic;
using System.Linq;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Editor;

/// <summary>
/// Scans a segment for map problems that are easy to miss in the editor and only show up in game.
/// </summary>
public static class SegmentProblemChecker
{
	/// <summary>
	/// Deepest nesting of segment components, templates and brushes followed when reading a tile.
	/// </summary>
	private const int MaxProviderNesting = 8;

	public static List<SegmentProblem> Check(Segment segment)
	{
		var problems = new List<SegmentProblem>();

		if (segment is null)
			return problems;

		var regionsById = new Dictionary<int, SegmentRegion>();

		foreach (var region in segment.Regions)
		{
			// regions are saved as "<id>.xml", so a second region with the same ID overwrites the first.
			if (!regionsById.TryAdd(region.ID, region))
			{
				problems.Add(new SegmentProblem(ProblemSeverity.Error,
					$"Regions '{regionsById[region.ID].Name}' and '{region.Name}' share ID {region.ID}; only one is saved.",
					region));
			}
		}

		var terrain = new List<TerrainComponent>();

		foreach (var region in segment.Regions)
		{
			foreach (var tile in region.GetTiles())
			{
				terrain.Clear();

				foreach (var provider in tile.Providers)
					CollectTerrain(provider, terrain, 0);

				CheckTeleporters(problems, regionsById, region, tile, terrain);
				CheckStatics(problems, region, tile, terrain);
			}
		}

		foreach (var location in segment.Locations)
			CheckPlacement(problems, regionsById, location, "Location", location.Region, location.X, location.Y);

		foreach (var spawner in segment.Spawns.Location)
			CheckPlacement(problems, regionsById, spawner, "Location spawner", spawner.Region, spawner.X, spawner.Y);

		foreach (var spawner in segment.Spawns.Region)
			CheckRegionSpawner(problems, regionsById, spawner);

		return problems
			.OrderByDescending(problem => problem.Severity)
			.ToList();
	}

	private static void CheckTeleporters(List<SegmentProblem> problems, Dictionary<int, SegmentRegion> regionsById,
		SegmentRegion region, SegmentTile tile, List<TerrainComponent> terrain)
	{
		foreach (var teleporter in terrain.OfType<TeleportComponent>())
		{
			var dx = teleporter.DestinationX;
			var dy = teleporter.DestinationY;
			var destinationId = teleporter.DestinationRegion;

			if (destinationId == 0)
			{
				problems.Add(new SegmentProblem(ProblemSeverity.Error,
					"Teleporter has no destination region.", region, tile.X, tile.Y));
				continue;
			}

			// a zero value isn't saved, and the server only reads a destination with all three values present.
			if (dx == 0 || dy == 0)
			{
				problems.Add(new SegmentProblem(ProblemSeverity.Error,
					$"Teleporter leads to ({dx}, {dy}); a 0 coordinate isn't saved, so the server ignores this teleporter.",
					region, tile.X, tile.Y));
				continue;
			}

			if (!regionsById.TryGetValue(destinationId, out var destination))
			{
				problems.Add(new SegmentProblem(ProblemSeverity.Error,
					$"Teleporter leads to region {destinationId}, which doesn't exist.", region, tile.X, tile.Y));
				continue;
			}

			if (destination.GetTile(dx, dy) is null)
			{
				problems.Add(new SegmentProblem(ProblemSeverity.Error,
					$"Teleporter leads to ({dx}, {dy}) in '{destination.Name}', which has no tile.", region, tile.X, tile.Y));
			}
		}
	}

	private static void CheckStatics(List<SegmentProblem> problems, SegmentRegion region, SegmentTile tile,
		List<TerrainComponent> terrain)
	{
		if (!terrain.OfType<StaticComponent>().Any())
			return;

		// the same rule the statics filter uses to colour a static red.
		var supported = terrain.Any(component => component is FloorComponent or ObstructionComponent
			or CounterComponent or AltarComponent || component is WallComponent { IsIndestructible: true });

		if (!supported)
		{
			problems.Add(new SegmentProblem(ProblemSeverity.Warning,
				"Static with nothing under it: no floor, indestructible wall, obstruction, counter or altar.",
				region, tile.X, tile.Y));
		}
	}

	private static void CheckPlacement(List<SegmentProblem> problems, Dictionary<int, SegmentRegion> regionsById,
		ISegmentObject source, string kind, int regionId, int x, int y)
	{
		if (!regionsById.TryGetValue(regionId, out var region))
		{
			problems.Add(new SegmentProblem(ProblemSeverity.Error,
				$"{kind} '{source.Name}' is in region {regionId}, which doesn't exist.", source));
			return;
		}

		if (region.GetTile(x, y) is null)
		{
			problems.Add(new SegmentProblem(ProblemSeverity.Warning,
				$"{kind} '{source.Name}' is on an empty tile.", region, x, y, source));
		}
	}

	private static void CheckRegionSpawner(List<SegmentProblem> problems, Dictionary<int, SegmentRegion> regionsById,
		RegionSegmentSpawner spawner)
	{
		if (!regionsById.TryGetValue(spawner.Region, out var region))
		{
			problems.Add(new SegmentProblem(ProblemSeverity.Error,
				$"Region spawner '{spawner.Name}' is in region {spawner.Region}, which doesn't exist.", spawner));
			return;
		}

		foreach (var inclusion in spawner.Inclusions.Where(bounds => bounds.IsValid))
		{
			if (HasAnyTile(region, inclusion))
				continue;

			problems.Add(new SegmentProblem(ProblemSeverity.Warning,
				$"Region spawner '{spawner.Name}' has an area with no tiles: ({inclusion.Left}, {inclusion.Top}) to ({inclusion.Right}, {inclusion.Bottom}).",
				region, inclusion.Left, inclusion.Top, spawner));
		}
	}

	private static bool HasAnyTile(SegmentRegion region, SegmentBounds bounds)
	{
		for (var y = bounds.Top; y <= bounds.Bottom; y++)
		for (var x = bounds.Left; x <= bounds.Right; x++)
		{
			if (region.GetTile(x, y) != null)
				return true;
		}

		return false;
	}

	/// <summary>
	/// Adds the terrain components the provider can place, looking inside segment components, templates and brushes.
	/// </summary>
	private static void CollectTerrain(IComponentProvider provider, List<TerrainComponent> terrain, int depth)
	{
		if (provider is TerrainComponent terrainComponent)
		{
			terrain.Add(terrainComponent);
			return;
		}

		// the limit guards against wrappers that contain themselves.
		if (provider is null || depth >= MaxProviderNesting)
			return;

		// brushes yield themselves from GetComponents; what they place comes from their weighted entries.
		var wrapped = provider is SegmentBrush brush
			? brush.Entries.Where(entry => entry.Component is not null && entry.Weight > 0).Select(entry => entry.Component)
			: provider.GetComponents();

		foreach (var component in wrapped)
		{
			if (ReferenceEquals(component, provider))
				continue;

			CollectTerrain(component, terrain, depth + 1);
		}
	}
}
