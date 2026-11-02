#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;

namespace Kesmai.WorldForge.Editor.Items;

/// <summary>
/// Item names live in the repository's Content\Localization\Enu.xml as numbered entries (6000001 "amulet", 6000086
/// "spear", ...). An item's class points at one with LabelNumber. This finds the entry for a name, or adds one.
/// </summary>
public class ItemNames
{
	public const int First = 6000001;
	public const int Last = 6004999;

	private static readonly Regex _entry = new(@"<entry\s+index=""(?<index>\d+)""\s*>(?<text>[^<]*)</entry>", RegexOptions.Compiled);

	public string Path { get; }

	public ItemNames(string contentPath)
	{
		Path = System.IO.Path.Combine(contentPath, "Localization", "Enu.xml");
	}

	public bool Exists => File.Exists(Path);

	/// <summary>Item name entries by number.</summary>
	public Dictionary<int, string> Read()
	{
		var names = new Dictionary<int, string>();

		if (!Exists)
			return names;

		foreach (Match match in _entry.Matches(File.ReadAllText(Path)))
		{
			var index = Int32.Parse(match.Groups["index"].Value);

			if (index is >= First and <= Last)
				names[index] = System.Net.WebUtility.HtmlDecode(match.Groups["text"].Value);
		}

		return names;
	}

	/// <summary>The entry already holding this name, if any. Names match without regard to case.</summary>
	public int? Find(string name)
	{
		foreach (var (index, text) in Read())
		{
			if (String.Equals(text.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase))
				return index;
		}

		return null;
	}

	/// <summary>The number a new name would get: one after the highest item name.</summary>
	public int Next()
	{
		var names = Read();
		return names.Count == 0 ? First : names.Keys.Max() + 1;
	}

	/// <summary>Adds the name as a new entry right after the last item name, keeping the file's layout, and returns its number.</summary>
	public int Add(string name)
	{
		var text = File.ReadAllText(Path);
		var newline = text.Contains("\r\n") ? "\r\n" : "\n";
		var lines = text.Split(newline).ToList();
		var index = Next();

		var last = lines.FindLastIndex(line => _entry.Match(line) is { Success: true } m
		                                       && Int32.Parse(m.Groups["index"].Value) is >= First and <= Last);

		var indent = last >= 0 ? lines[last].Substring(0, lines[last].Length - lines[last].TrimStart().Length) : "  ";
		var entry = $"{indent}<entry index=\"{index}\">{SecurityElement.Escape(name.Trim())}</entry>";

		if (last >= 0)
			lines.Insert(last + 1, entry);
		else
			lines.Insert(Math.Max(0, lines.FindLastIndex(l => l.Contains("</data>"))), entry);

		File.WriteAllText(Path, String.Join(newline, lines));

		return index;
	}
}
