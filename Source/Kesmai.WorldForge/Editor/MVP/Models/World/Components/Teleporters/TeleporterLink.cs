using System.Linq;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.Models;

/// <summary>
/// Links a teleporter to where it lands in two clicks: pick the teleporter, then the landing tile in any region. When
/// the landing tile has a teleporter of its own (the other end of stairs or a rope), it can be pointed back at the first.
/// </summary>
public static class TeleporterLink
{
	/// <summary>The teleporter waiting for its landing tile, if one was picked.</summary>
	public static (SegmentRegion Region, int X, int Y)? Pending { get; set; }

	/// <summary>The teleporter placed directly on a tile, if it has one (one inside a template or component is shared).</summary>
	public static TeleportComponent TeleporterOn(SegmentTile tile) => tile?.Providers.OfType<TeleportComponent>().FirstOrDefault();

	/// <summary>The picked teleporter, while its tile still has it.</summary>
	public static TeleportComponent PendingTeleporter
		=> Pending is { } pending ? TeleporterOn(pending.Region.GetTile(pending.X, pending.Y)) : null;

	/// <summary>
	/// Points the picked teleporter at (x, y) in <paramref name="region"/>, and when <paramref name="back"/> is set,
	/// the teleporter there back at the picked one. Each changed tile is captured in <paramref name="edit"/> first.
	/// </summary>
	/// <returns>How many teleporters changed.</returns>
	public static int Land(SegmentRegion region, int x, int y, bool back, MapEdit edit)
	{
		if (Pending is not { } pending || PendingTeleporter is not { } source)
			return 0;

		var (sourceRegion, sx, sy) = pending;

		edit?.Capture(sourceRegion, sx, sy);

		source.DestinationX = x;
		source.DestinationY = y;
		source.DestinationRegion = region.ID;

		var changed = 1;

		if (back && TeleporterOn(region.GetTile(x, y)) is { } other && other != source)
		{
			edit?.Capture(region, x, y);

			other.DestinationX = sx;
			other.DestinationY = sy;
			other.DestinationRegion = sourceRegion.ID;
			changed++;
		}

		Pending = null;

		return changed;
	}
}
