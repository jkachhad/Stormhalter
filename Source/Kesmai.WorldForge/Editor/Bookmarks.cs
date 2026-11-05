using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge;

/// <summary>
/// Named spots on the map (a region and a tile) to jump back to, kept between sessions for each segment in
/// .storage\Bookmarks.cfg.
/// </summary>
public static class Bookmarks
{
	public sealed record Bookmark(string Segment, string Name, int Region, int X, int Y);

	private static readonly string _path = Path.Combine(".storage", "Bookmarks.cfg");

	/// <summary>What bookmarks are kept under for a segment: its folder, or its name before it has one.</summary>
	public static string KeyFor(Segment segment) => string.IsNullOrEmpty(segment.Directory) ? segment.Name : segment.Directory;

	public static List<Bookmark> For(Segment segment) => Load().Where(b => b.Segment == KeyFor(segment)).ToList();

	public static void Add(Bookmark bookmark)
	{
		var all = Load();

		// one bookmark per name in a segment; saving a name again moves it
		all.RemoveAll(b => b.Segment == bookmark.Segment && string.Equals(b.Name, bookmark.Name, StringComparison.OrdinalIgnoreCase));
		all.Add(bookmark);

		Save(all);
	}

	public static void Remove(Bookmark bookmark)
	{
		var all = Load();

		all.Remove(bookmark);

		Save(all);
	}

	private static List<Bookmark> Load()
	{
		try
		{
			if (!File.Exists(_path))
				return new();

			return XElement.Load(_path).Elements("bookmark").Select(e => new Bookmark((string)e.Attribute("segment") ?? "",
				(string)e.Attribute("name") ?? "", (int)e.Attribute("region"), (int)e.Attribute("x"), (int)e.Attribute("y"))).ToList();
		}
		catch (Exception exception)
		{
			Debug.WriteLine($"Bookmarks couldn't be read: {exception.Message}");
			return new();
		}
	}

	private static void Save(List<Bookmark> all)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

		new XElement("bookmarks", all.Select(b => new XElement("bookmark", new XAttribute("segment", b.Segment),
			new XAttribute("name", b.Name), new XAttribute("region", b.Region), new XAttribute("x", b.X), new XAttribute("y", b.Y)))).Save(_path);
	}
}
