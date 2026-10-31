using System.Collections.Generic;
using System.Linq;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.Models;

/// <summary>
/// Fills the destinations of the sky tiles joined to one: each gets the first tile's destination moved by how far it is
/// from that tile, in the same region. A sky tile one to the east of one going to 27, 5 in region 13 goes to 28, 5 in 13.
/// </summary>
public static class SkyDestinationFill
{
	/// <summary>The sky component placed directly on a tile, if it has one.</summary>
	public static SkyComponent SkyOn(SegmentTile tile) => tile?.Providers.OfType<SkyComponent>().FirstOrDefault();

	/// <summary>
	/// Fills every sky tile joined to (x, y) through its four sides, from the destination of the sky at (x, y). Each
	/// changed tile is captured in <paramref name="edit"/> first, so the fill can be undone.
	/// </summary>
	/// <returns>How many other sky tiles were changed.</returns>
	public static int Fill(SegmentRegion region, int x, int y, MapEdit edit)
	{
		var start = SkyOn(region.GetTile(x, y));

		if (start is null)
			return 0;

		var changed = 0;
		var seen = new HashSet<(int X, int Y)> { (x, y) };
		var queue = new Queue<(int X, int Y)>();
		queue.Enqueue((x, y));

		while (queue.Count > 0)
		{
			var (cx, cy) = queue.Dequeue();

			foreach (var (nx, ny) in new[] { (cx + 1, cy), (cx - 1, cy), (cx, cy + 1), (cx, cy - 1) })
			{
				if (!seen.Add((nx, ny)) || SkyOn(region.GetTile(nx, ny)) is not { } sky)
					continue;

				queue.Enqueue((nx, ny));

				var destinationX = start.DestinationX + (nx - x);
				var destinationY = start.DestinationY + (ny - y);

				if (sky.DestinationX == destinationX && sky.DestinationY == destinationY && sky.DestinationRegion == start.DestinationRegion)
					continue;

				edit?.Capture(region, nx, ny);

				sky.DestinationX = destinationX;
				sky.DestinationY = destinationY;
				sky.DestinationRegion = start.DestinationRegion;
				changed++;
			}
		}

		return changed;
	}
}
