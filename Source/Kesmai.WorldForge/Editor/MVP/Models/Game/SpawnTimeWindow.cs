#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.WorldForge.Scripting;

namespace Kesmai.WorldForge;

public enum SpawnTimeMode
{
	/// <summary>The spawner works at any time.</summary>
	Always,
	/// <summary>Only during the ticked in-game light phases.</summary>
	InGamePhases,
	/// <summary>Only between two times on the server's clock.</summary>
	ServerClock,
}

/// <summary>
/// When a spawner may spawn. The server's spawners have no setting for this, so it is written as a marked block at
/// the top of the spawner's OnAfterSpawn script: outside the window the new spawn (or every spawn) is removed and the
/// spawner tries again after its usual delay. The settings are kept in the block's first line, so they read back.
/// </summary>
public class SpawnTimeWindow
{
	public static readonly string[] Phases = ["Dawn", "Sunrise", "Daytime", "Sunset", "Midnight", "Nighttime"];

	private const string Begin = "// worldforge-spawn-time:";
	private const string End = "// end worldforge-spawn-time";
	private const string KeptOpen = "#if false // WorldForge: this code was in a disabled script; Spawn time enabled the script";
	private const string KeptClose = "#endif // WorldForge";

	public SpawnTimeMode Mode { get; set; }
	public HashSet<string> AllowedPhases { get; } = new(StringComparer.OrdinalIgnoreCase);
	public TimeSpan From { get; set; } = new(20, 0, 0);
	public TimeSpan Until { get; set; } = new(2, 0, 0);

	/// <summary>Outside the window, remove everything the spawner made rather than only the new spawn.</summary>
	public bool RemoveAll { get; set; }

	public SpawnTimeWindow Clone()
	{
		var copy = new SpawnTimeWindow { Mode = Mode, From = From, Until = Until, RemoveAll = RemoveAll };
		copy.AllowedPhases.UnionWith(AllowedPhases);
		return copy;
	}

	private static readonly Regex _setting = new(@"(?<key>\w+)=(?<value>[^;]*)", RegexOptions.Compiled);

	/// <summary>The window written in a script, or one that is always open.</summary>
	public static SpawnTimeWindow Read(Script? script)
	{
		var window = new SpawnTimeWindow();
		var line = script?.Body.Replace("\r\n", "\n").Split('\n').Select(l => l.Trim()).FirstOrDefault(l => l.StartsWith(Begin));

		if (line is null || !(script?.IsEnabled ?? false))
			return window;

		foreach (Match setting in _setting.Matches(line.Substring(Begin.Length)))
		{
			var value = setting.Groups["value"].Value.Trim();

			switch (setting.Groups["key"].Value)
			{
				case "mode" when Enum.TryParse<SpawnTimeMode>(value, out var mode):
					window.Mode = mode;
					break;
				case "phases":
					window.AllowedPhases.UnionWith(value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Where(p => Phases.Contains(p)));
					break;
				case "from" when TimeSpan.TryParseExact(value, @"hh\:mm", CultureInfo.InvariantCulture, out var from):
					window.From = from;
					break;
				case "until" when TimeSpan.TryParseExact(value, @"hh\:mm", CultureInfo.InvariantCulture, out var until):
					window.Until = until;
					break;
				case "removeAll":
					window.RemoveAll = value == "true";
					break;
			}
		}

		return window;
	}

	/// <summary>The marked block, or an empty string when the window is always open.</summary>
	public string ToBlock()
	{
		if (Mode == SpawnTimeMode.Always)
			return String.Empty;

		var nl = Environment.NewLine;
		var phases = Phases.Where(AllowedPhases.Contains).ToList();
		var settings = $"mode={Mode}; phases={String.Join(",", phases)}; from={From:hh\\:mm}; until={Until:hh\\:mm}; removeAll={(RemoveAll ? "true" : "false")}";
		var remove = RemoveAll ? "spawner.Despawn(); /* remove everything this spawner made */" : "spawn.Delete(); /* remove the new spawn */";

		var lines = new List<string>
		{
			$"{Begin} {settings}",
			"// Written by the Spawn time settings in Properties; change it there.",
			"{",
		};

		if (Mode == SpawnTimeMode.InGamePhases)
		{
			var test = phases.Count == 0 ? "false" : String.Join(" || ", phases.Select(p => $"phase == Lightphase.{p}"));

			lines.Add("\tvar phase = spawn.Facet.Lightphase;");
			lines.Add(String.Empty);
			lines.Add($"\tif (!({test}))");
		}
		else
		{
			var from = $"new TimeSpan({From.Hours}, {From.Minutes}, 0)";
			var until = $"new TimeSpan({Until.Hours}, {Until.Minutes}, 0)";

			// a window like 20:00 to 02:00 runs past midnight; the same time twice means all day
			var test = From == Until ? "true"
				: From < Until ? $"now >= {from} && now < {until}"
				: $"now >= {from} || now < {until}";

			lines.Add("\tvar now = DateTime.Now.TimeOfDay;");
			lines.Add(String.Empty);
			lines.Add($"\tif (!({test}))");
		}

		lines.Add("\t{");
		lines.Add($"\t\t{remove}");
		lines.Add("\t\treturn;");
		lines.Add("\t}");
		lines.Add("}");
		lines.Add(End);

		return String.Join(nl, lines);
	}

	/// <summary>Writes the window into the script, replacing any earlier block and leaving the rest of the script as it was.</summary>
	public void Apply(Script script)
	{
		var nl = Environment.NewLine;
		var rest = Remove(script.Body).Trim('\r', '\n');
		var block = ToBlock();

		if (block.Length == 0)
		{
			// code that was only kept switched off goes back to a switched-off script
			if (rest.StartsWith(KeptOpen) && rest.EndsWith(KeptClose))
			{
				script.Body = rest.Substring(KeptOpen.Length, rest.Length - KeptOpen.Length - KeptClose.Length).Trim('\r', '\n');
				script.IsEnabled = false;
				return;
			}

			script.Body = rest;

			if (rest.Trim().Length == 0)
				script.IsEnabled = false;

			return;
		}

		// the block only runs in an enabled script; code the script held while switched off stays switched off
		if (!script.IsEnabled && rest.Trim().Length > 0 && !rest.StartsWith(KeptOpen))
			rest = KeptOpen + nl + rest + nl + KeptClose;

		script.Body = rest.Length == 0 ? block : block + nl + nl + rest;
		script.IsEnabled = true;
	}

	/// <summary>The script without its spawn time block.</summary>
	private static string Remove(string body)
	{
		var lines = body.Replace("\r\n", "\n").Split('\n').ToList();
		var start = lines.FindIndex(l => l.Trim().StartsWith(Begin));

		if (start < 0)
			return body;

		var end = lines.FindIndex(start, l => l.Trim() == End);

		if (end < 0)
			return body;

		lines.RemoveRange(start, end - start + 1);

		return String.Join(Environment.NewLine, lines);
	}
}
