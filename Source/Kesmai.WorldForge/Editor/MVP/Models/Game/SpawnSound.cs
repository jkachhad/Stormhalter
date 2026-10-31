#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Kesmai.WorldForge.Scripting;
using Microsoft.CodeAnalysis.CSharp;

namespace Kesmai.WorldForge;

/// <summary>Who hears a spawn sound.</summary>
public enum SoundScope
{
	/// <summary>Anyone within range of the creature, the way creatures make their own sounds.</summary>
	Nearby,
	/// <summary>Every player in the segment.</summary>
	Segment,
	/// <summary>Every player in the region the creature spawned in.</summary>
	Region,
	/// <summary>Every player inside the named subregion.</summary>
	Subregion,
}

/// <summary>
/// A sound played each time the spawner spawns. Like <see cref="SpawnBroadcast"/> it is a marked block in the
/// spawner's OnAfterSpawn script, after the spawn time block, so a spawn removed for being outside its window is silent.
/// </summary>
public class SpawnSound
{
	internal const string Begin = "// worldforge-spawn-sound:";
	internal const string End = "// end worldforge-spawn-sound";

	public bool Enabled { get; set; }
	public int SoundId { get; set; }
	public SoundScope Scope { get; set; } = SoundScope.Nearby;

	/// <summary>With Nearby: how many tiles away it can be heard.</summary>
	public int Range { get; set; } = 3;

	/// <summary>How loud, on the scale scripts use for EmitSound (they mostly use 9).</summary>
	public int Volume { get; set; } = 9;

	public string Subregion { get; set; } = String.Empty;

	public SpawnSound Clone() => new() { Enabled = Enabled, SoundId = SoundId, Scope = Scope, Range = Range, Volume = Volume, Subregion = Subregion };

	private static readonly Regex _setting = new(@"(?<key>\w+)=(?<value>[^;]*)", RegexOptions.Compiled);

	/// <summary>The sound written in a script, or a switched-off one.</summary>
	public static SpawnSound Read(Script? script)
	{
		var sound = new SpawnSound();
		var line = script?.Body.Replace("\r\n", "\n").Split('\n').Select(l => l.Trim()).FirstOrDefault(l => l.StartsWith(Begin));

		if (line is null || !(script?.IsEnabled ?? false))
			return sound;

		sound.Enabled = true;

		foreach (Match setting in _setting.Matches(line.Substring(Begin.Length)))
		{
			var value = setting.Groups["value"].Value.Trim();

			switch (setting.Groups["key"].Value)
			{
				case "sound" when Int32.TryParse(value, out var id):
					sound.SoundId = id;
					break;
				case "scope" when Enum.TryParse<SoundScope>(value, out var scope):
					sound.Scope = scope;
					break;
				case "range" when Int32.TryParse(value, out var range):
					sound.Range = range;
					break;
				case "volume" when Int32.TryParse(value, out var volume):
					sound.Volume = volume;
					break;
				case "subregion":
					try
					{
						sound.Subregion = Encoding.UTF8.GetString(Convert.FromBase64String(value));
					}
					catch (FormatException)
					{
					}
					break;
			}
		}

		return sound;
	}

	/// <summary>The marked block, or an empty string when the sound is off.</summary>
	public string ToBlock()
	{
		if (!Enabled)
			return String.Empty;

		var volume = Math.Clamp(Volume, 0, 255);
		var settings = $"sound={SoundId}; scope={Scope}; range={Range}; volume={volume}; subregion={Convert.ToBase64String(Encoding.UTF8.GetBytes(Subregion))}";

		var lines = new List<string>
		{
			$"{Begin} {settings}",
			"// Written by the Sound on spawn settings in Properties; change it there.",
			"{",
		};

		if (Scope == SoundScope.Nearby)
		{
			lines.Add($"\tspawn.EmitSound({SoundId}, {Math.Max(0, Range)}, {volume});");
		}
		else
		{
			var groups = Scope == SoundScope.Segment ? "spawn.Segment.Groups" : "spawn.Region.Groups";

			lines.Add($"\tvar players = {groups}.OfType<PlayerGroup>().SelectMany(g => g.Members).OfType<PlayerEntity>();");

			if (Scope == SoundScope.Subregion)
			{
				lines.Add($"\tvar areas = spawn.Region.Subregions.Where(s => s.Name == {SymbolDisplay.FormatLiteral(Subregion, quote: true)}).ToList();");
				lines.Add(String.Empty);
				lines.Add("\tplayers = players.Where(p => areas.Any(a => a.Contains(p.Location)));");
			}

			lines.Add(String.Empty);
			lines.Add("\tforeach (var player in players)");
			lines.Add($"\t\tplayer.SendPacket(new Kesmai.Server.Network.PlaySoundPacket({SoundId}, (byte){volume}, null, 0));");
		}

		lines.Add("}");
		lines.Add(End);

		return String.Join(Environment.NewLine, lines);
	}

	/// <summary>Writes the sound into the script, after the spawn time and broadcast blocks and above the script's own code.</summary>
	public void Apply(Script script)
		=> SpawnTimeWindow.Compose(script, SpawnTimeWindow.Read(script).ToBlock(), SpawnBroadcast.Read(script).ToBlock(), ToBlock());
}
