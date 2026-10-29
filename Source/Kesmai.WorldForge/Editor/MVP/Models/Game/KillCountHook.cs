#nullable enable

using System;
using Kesmai.WorldForge.Scripting;
using Microsoft.CodeAnalysis.CSharp;

namespace Kesmai.WorldForge;

/// <summary>Where an enemy's deaths count for "Spawn after kills".</summary>
public enum KillCountArea
{
	Segment,
	Region,
}

/// <summary>
/// The marked block WorldForge puts at the top of an enemy's OnDeath script while a spawner uses "Spawn after kills" on
/// it. It counts the enemy's deaths in Storage, both across the segment and per region, so it survives a restart. It is
/// taken out again once no spawner uses that enemy, leaving the script exactly as it was.
/// </summary>
public static class KillCountHook
{
	public const string StorageName = "WorldForge:Kills";

	private const string Begin = "// worldforge-kill-count:";
	private const string End = "// end worldforge-kill-count";

	internal static string Literal(string text) => SymbolDisplay.FormatLiteral(text, quote: true);

	/// <summary>The C# expression for a counter's key: the enemy's name, or its name and a region index.</summary>
	internal static string CounterKey(string enemy, string? regionIndex)
		=> regionIndex is null ? Literal(enemy.Trim()) : $"{Literal(enemy.Trim() + "@")} + {regionIndex}";

	public static bool IsInstalled(Script? onDeath) => onDeath is { IsEnabled: true } && onDeath.Body.Contains(Begin);

	/// <summary>
	/// Whether the counter in this OnDeath counts deaths under this enemy's name. An entity copied from another keeps
	/// that one's counter, which counts its deaths under the other name.
	/// </summary>
	public static bool IsInstalledFor(Script? onDeath, string enemy)
		=> IsInstalled(onDeath) && onDeath!.Body.Replace("\r\n", "\n").Contains($"{Marker(enemy)}\n");

	/// <summary>Whether the OnDeath has a counter at all, for this or another name.</summary>
	public static bool HasCounter(Script? onDeath) => onDeath is not null && onDeath.Body.Contains(Begin);

	private static string Marker(string enemy) => $"{Begin} enemy={Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(enemy.Trim()))}";

	private static string Block(string enemy)
	{
		var segment = CounterKey(enemy, null);
		var region = CounterKey(enemy, "source.RegionIndex");

		return String.Join("\n",
			Marker(enemy),
			"// Counts this creature's deaths for spawners set to Spawn after kills. WorldForge adds and removes it; change it on those spawners.",
			"{",
			$"\tvar kills = Storage.Get({Literal(StorageName)});",
			$"\tvar inRegion = {region};",
			String.Empty,
			$"\tkills.Set<int>({segment}, kills.Get<int>({segment}, 0) + 1);",
			"\tkills.Set<int>(inRegion, kills.Get<int>(inRegion, 0) + 1);",
			"}",
			End);
	}

	/// <summary>Adds the counter at the top of the OnDeath script; the rest of the script stays exactly as it was.</summary>
	public static void Install(Script onDeath, string enemy) => ScriptBlocks.Write(onDeath, [Block(enemy)]);

	/// <summary>Takes the counter out, putting back the script exactly as it was before it was added.</summary>
	public static void Remove(Script onDeath)
	{
		if (onDeath.Body.Contains(Begin))
			ScriptBlocks.Write(onDeath, []);
	}
}
