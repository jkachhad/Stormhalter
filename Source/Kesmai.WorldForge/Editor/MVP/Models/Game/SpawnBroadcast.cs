#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Kesmai.WorldForge.Scripting;
using Microsoft.CodeAnalysis.CSharp;

namespace Kesmai.WorldForge;

/// <summary>Who hears a spawn broadcast.</summary>
public enum BroadcastScope
{
	/// <summary>Every player in the segment.</summary>
	Segment,
	/// <summary>Every player in the region the creature spawned in.</summary>
	Region,
	/// <summary>Every player inside the named subregion.</summary>
	Subregion,
}

/// <summary>
/// A message sent to nearby players each time the spawner spawns. Like <see cref="SpawnTimeWindow"/> it is written as a
/// marked block in the spawner's OnAfterSpawn script, after the spawn time block, so a spawn that was removed for being
/// outside its window returns before it and isn't announced.
/// </summary>
public class SpawnBroadcast
{
	internal const string Begin = "// worldforge-spawn-broadcast:";
	internal const string End = "// end worldforge-spawn-broadcast";

	public bool Enabled { get; set; }
	public string Message { get; set; } = "{name} has appeared!";
	public BroadcastScope Scope { get; set; } = BroadcastScope.Region;
	public string Subregion { get; set; } = String.Empty;

	public SpawnBroadcast Clone() => new() { Enabled = Enabled, Message = Message, Scope = Scope, Subregion = Subregion };

	private static readonly Regex _setting = new(@"(?<key>\w+)=(?<value>[^;]*)", RegexOptions.Compiled);

	private static string Encode(string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

	private static string Decode(string text)
	{
		try
		{
			return Encoding.UTF8.GetString(Convert.FromBase64String(text));
		}
		catch (FormatException)
		{
			return String.Empty;
		}
	}

	/// <summary>The broadcast written in a script, or a switched-off one.</summary>
	public static SpawnBroadcast Read(Script? script)
	{
		var broadcast = new SpawnBroadcast();
		var line = script?.Body.Replace("\r\n", "\n").Split('\n').Select(l => l.Trim()).FirstOrDefault(l => l.StartsWith(Begin));

		if (line is null || !(script?.IsEnabled ?? false))
			return broadcast;

		broadcast.Enabled = true;

		foreach (Match setting in _setting.Matches(line.Substring(Begin.Length)))
		{
			var value = setting.Groups["value"].Value.Trim();

			switch (setting.Groups["key"].Value)
			{
				case "scope" when Enum.TryParse<BroadcastScope>(value, out var scope):
					broadcast.Scope = scope;
					break;
				case "subregion":
					broadcast.Subregion = Decode(value);
					break;
				case "message":
					broadcast.Message = Decode(value);
					break;
			}
		}

		return broadcast;
	}

	/// <summary>The marked block, or an empty string when the broadcast is off.</summary>
	public string ToBlock()
	{
		if (!Enabled)
			return String.Empty;

		// the settings line keeps the text encoded, so any character in it reads back
		var settings = $"scope={Scope}; subregion={Encode(Subregion)}; message={Encode(Message)}";
		var text = SymbolDisplay.FormatLiteral(Message, quote: true);

		var players = Scope switch
		{
			BroadcastScope.Segment => "spawn.Segment.Groups",
			BroadcastScope.Region => "spawn.Region.Groups",
			_ => "spawn.Region.Groups",
		};

		var lines = new List<string>
		{
			$"{Begin} {settings}",
			"// Written by the Broadcast on spawn settings in Properties; change it there.",
			"{",
			$"\tvar message = {text}.Replace(\"{{name}}\", spawn.Name);",
			$"\tvar players = {players}.OfType<PlayerGroup>().SelectMany(g => g.Members).OfType<PlayerEntity>();",
		};

		if (Scope == BroadcastScope.Subregion)
		{
			lines.Add($"\tvar areas = spawn.Region.Subregions.Where(s => s.Name == {SymbolDisplay.FormatLiteral(Subregion, quote: true)}).ToList();");
			lines.Add(String.Empty);
			lines.Add("\tplayers = players.Where(p => areas.Any(a => a.Contains(p.Location)));");
		}

		lines.Add(String.Empty);
		lines.Add("\tforeach (var player in players)");
		lines.Add("\t\tplayer.SendMessage(Color.Gold, message);");
		lines.Add("}");
		lines.Add(End);

		return String.Join(Environment.NewLine, lines);
	}

	/// <summary>Writes the broadcast into the script, after any spawn time block and above the script's own code.</summary>
	public void Apply(Script script) => SpawnTimeWindow.Compose(script, SpawnTimeWindow.Read(script).ToBlock(), ToBlock(), SpawnSound.Read(script).ToBlock());
}
