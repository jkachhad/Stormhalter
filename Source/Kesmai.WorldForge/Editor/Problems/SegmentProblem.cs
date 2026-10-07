namespace Kesmai.WorldForge.Editor;

public enum ProblemSeverity
{
	/// <summary>
	/// Likely unintended, but the segment still works.
	/// </summary>
	Warning,

	/// <summary>
	/// Something that won't work in game.
	/// </summary>
	Error,
}

/// <summary>
/// An issue found in a segment, with where to find it.
/// </summary>
public sealed class SegmentProblem
{
	public ProblemSeverity Severity { get; }

	public string Message { get; }

	/// <summary>
	/// Gets the region holding the tile to jump to, or null if the problem has no tile to show.
	/// </summary>
	public SegmentRegion Region { get; }

	public int X { get; }
	public int Y { get; }

	/// <summary>
	/// Gets the location, spawner or other segment object the problem belongs to, if any.
	/// </summary>
	public ISegmentObject Source { get; }

	/// <summary>
	/// Gets a short description of where the problem is, for display.
	/// </summary>
	public string Where
	{
		get
		{
			if (Region != null)
				return $"{Region.Name} ({X}, {Y})";

			return Source?.Name ?? string.Empty;
		}
	}

	public SegmentProblem(ProblemSeverity severity, string message, SegmentRegion region, int x, int y,
		ISegmentObject source = null)
	{
		Severity = severity;
		Message = message;
		Region = region;
		X = x;
		Y = y;
		Source = source;
	}

	public SegmentProblem(ProblemSeverity severity, string message, ISegmentObject source)
		: this(severity, message, null, 0, 0, source)
	{
	}
}
