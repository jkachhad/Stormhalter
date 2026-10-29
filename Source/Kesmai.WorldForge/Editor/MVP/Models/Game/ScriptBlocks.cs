#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Kesmai.WorldForge.Scripting;

namespace Kesmai.WorldForge;

/// <summary>
/// The marked blocks WorldForge writes at the top of a script (spawn time, broadcast, sound, kill counter). They are
/// always written first, each followed by a blank line, in the script's own line endings, so taking them out again
/// leaves the script's own code exactly as it was, character for character.
/// </summary>
internal static class ScriptBlocks
{
	internal const string KeptOpen = "#if false // WorldForge: this code was in a disabled script; WorldForge enabled the script for its settings";
	internal const string KeptClose = "#endif // WorldForge";

	// older blocks kept a script's switched-off code under these lines; they are still recognised
	private static readonly string[] _keptOpenings =
	[
		KeptOpen,
		"#if false // WorldForge: this code was in a disabled script; Spawn time enabled the script",
		"#if false // WorldForge: this code was in a disabled script; Spawn after kills enabled the script",
	];

	private static readonly (string Begin, string End)[] _markers =
	[
		("// worldforge-spawn-time:", "// end worldforge-spawn-time"),
		("// worldforge-spawn-broadcast:", "// end worldforge-spawn-broadcast"),
		("// worldforge-spawn-sound:", "// end worldforge-spawn-sound"),
		("// worldforge-kill-count:", "// end worldforge-kill-count"),
		("// worldforge-spawn-hold:", "// end worldforge-spawn-hold"),
	];

	/// <summary>The script's line ending: CRLF when it has one, otherwise LF (what scripts read from a segment have).</summary>
	internal static string NewLine(string body) => body.Contains("\r\n") ? "\r\n" : "\n";

	private static int SkipLineBreak(string body, int at)
	{
		if (String.CompareOrdinal(body, at, "\r\n", 0, 2) == 0)
			return at + 2;

		return at < body.Length && body[at] == '\n' ? at + 1 : at;
	}

	/// <summary>The script's own code: everything after WorldForge's blocks at the top, exactly as it is.</summary>
	internal static string OwnCode(string body)
	{
		var at = 0;

		while (true)
		{
			var marker = _markers.FirstOrDefault(m => String.CompareOrdinal(body, at, m.Begin, 0, m.Begin.Length) == 0);

			if (marker.Begin is null)
				return body.Substring(at);

			var end = body.IndexOf(marker.End, at, StringComparison.Ordinal);

			if (end < 0)
				return body.Substring(at);

			// the block's last line, then the blank line WorldForge put after it
			at = SkipLineBreak(body, SkipLineBreak(body, end + marker.End.Length));
		}
	}

	/// <summary>
	/// Puts the blocks at the top of the script, above its own code. With no blocks the script goes back to just its
	/// own code; code that was only kept switched off goes back to a switched-off script.
	/// </summary>
	internal static void Write(Script script, IEnumerable<string> blocks)
	{
		var own = OwnCode(script.Body);
		var nl = NewLine(script.Body.Length > 0 ? script.Body : own);
		var written = blocks.Where(b => b.Length > 0).Select(b => b.Replace("\r\n", "\n").Replace("\n", nl)).ToList();
		var opening = _keptOpenings.FirstOrDefault(o => own.StartsWith(o + nl, StringComparison.Ordinal) && own.EndsWith(nl + KeptClose, StringComparison.Ordinal));

		if (written.Count == 0)
		{
			if (opening != null)
			{
				script.Body = own.Substring(opening.Length + nl.Length, own.Length - opening.Length - nl.Length * 2 - KeptClose.Length);
				script.IsEnabled = false;
				return;
			}

			script.Body = own;

			if (String.IsNullOrWhiteSpace(own))
				script.IsEnabled = false;

			return;
		}

		// the blocks only run in an enabled script; code the script held while switched off stays switched off
		if (!script.IsEnabled && !String.IsNullOrWhiteSpace(own) && opening is null)
			own = KeptOpen + nl + own + nl + KeptClose;

		var top = String.Join(nl + nl, written);

		script.Body = own.Length == 0 ? top : top + nl + nl + own;
		script.IsEnabled = true;
	}
}
