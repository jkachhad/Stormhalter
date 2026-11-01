using System.Xml.Linq;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Editor;

/// <summary>
/// The contents of one tile at a point in time, used to undo and redo map edits.
/// </summary>
/// <remarks>
/// Terrain components belong to a single tile and are edited in place, so they are stored as detached copies.
/// Segment components, templates and brushes are shared between tiles, so they are stored by reference.
/// </remarks>
public sealed class TileSnapshot
{
	/// <summary>
	/// A snapshot of a location that holds no tile.
	/// </summary>
	public static readonly TileSnapshot Missing = new TileSnapshot(null, null);

	private readonly IComponentProvider[] _providers;
	private readonly XElement _element;

	/// <summary>
	/// Gets a value indicating whether a tile existed when the snapshot was taken.
	/// </summary>
	public bool Exists => _providers != null;

	private TileSnapshot(IComponentProvider[] providers, XElement element)
	{
		_providers = providers;
		_element = element;
	}

	/// <summary>
	/// Captures the current contents of the tile, or <see cref="Missing"/> when there is no tile.
	/// </summary>
	public static TileSnapshot Capture(SegmentTile tile)
	{
		if (tile is null)
			return Missing;

		var providers = new IComponentProvider[tile.Providers.Count];

		for (var index = 0; index < providers.Length; index++)
			providers[index] = Detach(tile.Providers[index]);

		return new TileSnapshot(providers, tile.GetSerializingElement());
	}

	/// <summary>
	/// Determines whether both snapshots describe the same tile contents, as they would be saved.
	/// </summary>
	public bool IsEquivalent(TileSnapshot other)
	{
		if (other is null || Exists != other.Exists)
			return false;

		if (!Exists)
			return true;

		return XNode.DeepEquals(_element, other._element);
	}

	/// <summary>
	/// Restores the tile at the location to the contents of this snapshot.
	/// </summary>
	public void Restore(SegmentRegion region, int x, int y)
	{
		if (!Exists)
		{
			if (region.GetTile(x, y) != null)
				region.DeleteTile(x, y);

			return;
		}

		var tile = region.GetTile(x, y) ?? region.CreateTile(x, y);

		tile.Providers.Clear();

		// copy again, so the snapshot stays unchanged when the restored tile is edited.
		foreach (var provider in _providers)
			tile.Providers.Add(Detach(provider));

		tile.UpdateTerrain();
	}

	private static IComponentProvider Detach(IComponentProvider provider)
	{
		if (provider is TerrainComponent terrainComponent)
			return terrainComponent.Clone();

		return provider;
	}
}
