#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.WorldForge.Scripting;

namespace Kesmai.WorldForge;


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

	/// <summary>Only during the ticked in-game light phases.</summary>
	public bool UsePhases { get; set; }

	/// <summary>Only between From and Until on the server's clock.</summary>
	public bool UseClock { get; set; }

	/// <summary>Only at the listed times on the server's clock, once each a day.</summary>
	public bool UseTimes { get; set; }

	/// <summary>No condition is on, so the spawner works as normal and nothing is written.</summary>
	/// <summary>Only once enough of an enemy have been killed since the last spawn.</summary>
	public bool UseKills { get; set; }

	/// <summary>With UseKills: the entity whose deaths count.</summary>
	public string KillEnemy { get; set; } = String.Empty;

	/// <summary>With UseKills: how many deaths it takes.</summary>
	public int KillsNeeded { get; set; } = 10;

	/// <summary>With UseKills: count only deaths in the spawner's region, rather than anywhere in the segment.</summary>
	public bool KillsInRegion { get; set; }

	/// <summary>No condition is on, so the spawner works as normal and nothing is written.</summary>
	public bool IsAlways => !UsePhases && !UseClock && !UseTimes && !UseKills;
	public HashSet<string> AllowedPhases { get; } = new(StringComparer.OrdinalIgnoreCase);
	public TimeSpan From { get; set; } = new(20, 0, 0);
	public TimeSpan Until { get; set; } = new(2, 0, 0);

	/// <summary>Outside the window, remove everything the spawner made rather than only the new spawn.</summary>
	public bool RemoveAll { get; set; }

	/// <summary>With ServerTimes: the times of day it spawns at.</summary>
	public List<TimeSpan> Times { get; } = new();

	/// <summary>With ServerTimes: how long after each time it may still spawn, for a late spawn attempt or a restart.</summary>
	public int WindowMinutes { get; set; } = 30;

	/// <summary>With ServerTimes: how many creatures it may spawn for each time.</summary>
	public int PerTime { get; set; } = 1;

	public SpawnTimeWindow Clone()
	{
		var copy = new SpawnTimeWindow { UsePhases = UsePhases, UseClock = UseClock, UseTimes = UseTimes, UseKills = UseKills, KillEnemy = KillEnemy, KillsNeeded = KillsNeeded, KillsInRegion = KillsInRegion, From = From, Until = Until, RemoveAll = RemoveAll, WindowMinutes = WindowMinutes, PerTime = PerTime };
		copy.AllowedPhases.UnionWith(AllowedPhases);
		copy.Times.AddRange(Times);
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
				// blocks written before the toggles held one mode
				case "mode":
					window.UsePhases = value == "InGamePhases";
					window.UseClock = value == "ServerClock";
					window.UseTimes = value == "ServerTimes";
					break;
				case "use":
					var used = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
					window.UsePhases = used.Contains("phases");
					window.UseClock = used.Contains("clock");
					window.UseTimes = used.Contains("times");
					window.UseKills = used.Contains("kills");
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
				case "times":
					window.Times.AddRange(ParseTimes(value));
					break;
				case "window" when Int32.TryParse(value, out var minutes):
					window.WindowMinutes = minutes;
					break;
				case "perTime" when Int32.TryParse(value, out var count):
					window.PerTime = count;
					break;
				case "enemy":
					try { window.KillEnemy = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value)); } catch (FormatException) { }
					break;
				case "kills" when Int32.TryParse(value, out var kills):
					window.KillsNeeded = kills;
					break;
				case "killsIn":
					window.KillsInRegion = value == "region";
					break;
			}
		}

		return window;
	}

	/// <summary>A time typed as 20:00, 8:05, 2000 or 20; null when it isn't one.</summary>
	public static TimeSpan? ParseTime(string? text)
	{
		var value = (text ?? String.Empty).Trim();

		if (TimeSpan.TryParseExact(value, [@"h\:mm", @"hh\:mm", @"hhmm", @"hmm"], CultureInfo.InvariantCulture, out var time) && time < TimeSpan.FromDays(1))
			return time;

		return Int32.TryParse(value, out var hours) && hours is >= 0 and < 24 ? TimeSpan.FromHours(hours) : null;
	}

	/// <summary>Times written as "08:00, 20:30", in order and without repeats.</summary>
	public static List<TimeSpan> ParseTimes(string? text)
		=> (text ?? String.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Select(ParseTime).Where(t => t != null).Select(t => t!.Value).Distinct().OrderBy(t => t).ToList();

	public static string FormatTimes(IEnumerable<TimeSpan> times) => String.Join(", ", times.Select(t => t.ToString(@"hh\:mm")));

	/// <summary>
	/// The marked block, or an empty string when no condition is on. Each condition that is on must allow the spawn;
	/// the server times are checked last, so a time is only used up by a spawn the other conditions let through.
	/// </summary>
	public string ToBlock()
	{
		if (IsAlways)
			return String.Empty;

		var nl = Environment.NewLine;
		var phases = Phases.Where(AllowedPhases.Contains).ToList();
		var used = new[] { UsePhases ? "phases" : null, UseClock ? "clock" : null, UseTimes ? "times" : null, UseKills ? "kills" : null }.Where(u => u != null);
		var settings = $"use={String.Join(",", used)}; phases={String.Join(",", phases)}; from={From:hh\\:mm}; until={Until:hh\\:mm}; removeAll={(RemoveAll ? "true" : "false")}"
		               + $"; times={FormatTimes(Times).Replace(" ", String.Empty)}; window={WindowMinutes}; perTime={PerTime}"
		               + $"; enemy={Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(KillEnemy))}; kills={KillsNeeded}; killsIn={(KillsInRegion ? "region" : "segment")}";
		// removed on the next tick: the server is still working with the new spawn when OnAfterSpawn runs, and deleting it
		// there throws inside Spawner.Spawn and crashes the server
		// the new spawn is made invisible at once, so it isn't seen for that tick
		var remove = RemoveAll
			? "Timer.DelayCall(() => spawner.Despawn()); /* remove everything this spawner made, once it has finished spawning */"
			: "Timer.DelayCall(() => spawn.Delete()); /* remove the new spawn, once the spawner has finished with it */";

		var lines = new List<string>
		{
			$"{Begin} {settings}",
			"// Written by the Spawn time settings in Properties; change it there.",
			"{",
			"\tvar allowed = true;",
		};

		if (UsePhases)
		{
			var test = phases.Count == 0 ? "false" : String.Join(" || ", phases.Select(p => $"phase == Lightphase.{p}"));

			lines.Add(String.Empty);
			lines.Add("\t// in-game light phases");
			lines.Add("\tvar phase = spawn.Facet.Lightphase;");
			lines.Add(String.Empty);
			lines.Add($"\tif (!({test}))");
			lines.Add("\t\tallowed = false;");
		}

		if (UseClock)
		{
			var from = $"new TimeSpan({From.Hours}, {From.Minutes}, 0)";
			var until = $"new TimeSpan({Until.Hours}, {Until.Minutes}, 0)";

			// a window like 20:00 to 02:00 runs past midnight; the same time twice means all day
			var test = From == Until ? "true"
				: From < Until ? $"timeOfDay >= {from} && timeOfDay < {until}"
				: $"timeOfDay >= {from} || timeOfDay < {until}";

			lines.Add(String.Empty);
			lines.Add("\t// server clock window");
			lines.Add("\tvar timeOfDay = DateTime.Now.TimeOfDay;");
			lines.Add(String.Empty);
			lines.Add($"\tif (!({test}))");
			lines.Add("\t\tallowed = false;");
		}

		if (UseKills)
		{
			// the enemy's OnDeath counts its deaths in Storage (see KillCountHook); this spawner remembers the count it last used
			lines.Add(String.Empty);
			lines.Add("\t// kills of an enemy since this spawner last spawned");
			lines.Add($"\tvar killKey = {KillCountHook.CounterKey(KillEnemy, KillsInRegion ? "spawn.RegionIndex" : null)};");
			lines.Add($"\tvar killCounts = Storage.Get({KillCountHook.Literal(KillCountHook.StorageName)});");
			lines.Add("\tvar killsUsed = Storage.Get(\"WorldForge:KillsUsed:\" + spawner.Name);");
			lines.Add("\tvar killCount = killCounts.Get<int>(killKey, 0);");
			lines.Add("\tvar killsFrom = killsUsed.Get<int>(killKey, -1);");
			lines.Add(String.Empty);
			// with no count remembered yet, kills from before the spawner started waiting (earlier tests, other setups) don't count
			lines.Add("\t// the first check starts counting from now");
			lines.Add("\tif (killsFrom < 0)");
			lines.Add("\t{");
			lines.Add("\t\tkillsFrom = killCount;");
			lines.Add("\t\tkillsUsed.Set<int>(killKey, killsFrom);");
			lines.Add("\t}");
			lines.Add(String.Empty);
			lines.Add(String.Empty);
			// the kills release a whole wave: it stays open until the spawner has its minimum again
			lines.Add("\t// a wave the kills released stays open until the spawner is filled");
			lines.Add("\tvar waveKey = killKey + \":wave\";");
			lines.Add("\tvar waveOpen = killsUsed.Get<int>(waveKey, 0) == 1;");
			lines.Add(String.Empty);
			lines.Add($"\tif (!waveOpen && killCount - killsFrom < {Math.Max(1, KillsNeeded)})");
			lines.Add("\t\tallowed = false;");
		}

		if (UseTimes)
		{
			// the latest listed time it is still within the window of; each time spawns PerTime creatures once a day
			var times = String.Join(", ", Times.Select(t => $"new TimeSpan({t.Hours}, {t.Minutes}, 0)"));

			lines.Add(String.Empty);
			lines.Add("\t// server times");
			lines.Add("\tvar now = DateTime.Now;");
			lines.Add("\tvar slot = (DateTime?)null;");
			lines.Add(String.Empty);
			lines.Add($"\tforeach (var time in new TimeSpan[] {{ {times} }})");
			lines.Add("\t{");
			lines.Add("\t\tvar start = now.Date + time;");
			lines.Add(String.Empty);
			lines.Add("\t\tif (start > now)");
			lines.Add("\t\t\tstart = start.AddDays(-1);");
			lines.Add(String.Empty);
			lines.Add($"\t\tif (now - start < TimeSpan.FromMinutes({Math.Max(1, WindowMinutes)}) && (slot == null || start > slot))");
			lines.Add("\t\t\tslot = start;");
			lines.Add("\t}");
			lines.Add(String.Empty);
			lines.Add("\tvar record = Storage.Get(\"WorldForge:SpawnTimes:\" + spawner.Name);");
			lines.Add("\tvar key = slot?.ToString(\"yyyy-MM-dd HH:mm\") ?? String.Empty;");
			lines.Add("\tvar spawned = record.Get<string>(\"Slot\", \"\") == key ? record.Get<int>(\"Count\", 0) : 0;");
			lines.Add(String.Empty);
			lines.Add($"\tif (slot == null || spawned >= {Math.Max(1, PerTime)})");
			lines.Add("\t\tallowed = false;");
			lines.Add("\telse if (allowed)");
			lines.Add("\t{");
			lines.Add("\t\trecord.Set<string>(\"Slot\", key);");
			lines.Add("\t\trecord.Set<int>(\"Count\", spawned + 1);");
			lines.Add("\t}");
		}

		lines.Add(String.Empty);
		lines.Add("\tif (!allowed)");
		lines.Add("\t{");
		lines.Add("\t\tspawn.IsInvisible = true; /* not seen while it waits to be removed */");
		lines.Add($"\t\t{remove}");
		lines.Add("\t\treturn;");
		lines.Add("\t}");

		if (UseKills)
		{
			// opening a wave uses the kills up; the next wave needs as many again, counted from now. The spawner's entries
			// hold its live groups; the new spawn's group may not be among them yet
			lines.Add(String.Empty);
			lines.Add("\tif (!waveOpen)");
			lines.Add("\t\tkillsUsed.Set<int>(killKey, killCount);");
			lines.Add(String.Empty);
			lines.Add("\tvar groups = spawner.Sum(entry => entry.Count) + (spawner.Any(entry => entry.Contains(spawn.Group)) ? 0 : 1);");
			lines.Add("\tkillsUsed.Set<int>(waveKey, groups < spawner.Sum(entry => entry.Minimum) ? 1 : 0);");
		}

		lines.Add("}");
		lines.Add(End);

		return String.Join(nl, lines);
	}

	/// <summary>Writes the window into the script, replacing any earlier block and leaving the rest of the script as it was.</summary>
	public void Apply(Script script) => Compose(script, ToBlock(), SpawnBroadcast.Read(script).ToBlock(), SpawnSound.Read(script).ToBlock());

	/// <summary>
	/// Writes the spawn time block, then the broadcast block, at the top of the script, above the script's own code.
	/// With neither, the script goes back to just its own code.
	/// </summary>
	internal static void Compose(Script script, string timeBlock, string broadcastBlock, string soundBlock)
		=> ScriptBlocks.Write(script, [timeBlock, broadcastBlock, soundBlock]);
}
