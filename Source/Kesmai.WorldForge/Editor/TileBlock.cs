using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Kesmai.WorldForge.Editor;
using XNA = Microsoft.Xna.Framework;

namespace Kesmai.WorldForge;

/// <summary>
/// A block of tiles lifted out of a region: each tile's XML at its offset in the block. Copied tiles and stamps are
/// blocks, and a block can be pasted into any region.
/// </summary>
public sealed class TileBlock
{
	public int Width { get; }
	public int Height { get; }

	/// <summary>The tiles that are there; a cell with no tile is left out.</summary>
	public IReadOnlyList<(int X, int Y, XElement Tile)> Tiles { get; }

	public TileBlock(int width, int height, IEnumerable<(int X, int Y, XElement Tile)> tiles)
	{
		Width = width;
		Height = height;
		Tiles = tiles.ToList();
	}

	/// <summary>The tiles in a rectangle of a region (its right and bottom are outside it).</summary>
	public static TileBlock FromRegion(SegmentRegion region, XNA.Rectangle area)
	{
		var tiles = new List<(int, int, XElement)>();

		for (var x = area.Left; x < area.Right; x++)
		for (var y = area.Top; y < area.Bottom; y++)
		{
			if (region.GetTile(x, y) is { } tile)
				tiles.Add((x - area.Left, y - area.Top, tile.GetSerializingElement()));
		}

		return new TileBlock(area.Width, area.Height, tiles);
	}

	/// <summary>The copied tiles on the clipboard, or null when it holds none.</summary>
	public static TileBlock FromClipboard()
	{
		var tiles = ClipboardManager.ReadTiles();

		if (tiles.Count == 0)
			return null;

		return new TileBlock(tiles.Max(t => t.X) + 1, tiles.Max(t => t.Y) + 1, tiles);
	}

	/// <summary>Puts the block on the clipboard as copied tiles, so Ctrl+V pastes it.</summary>
	public void ToClipboard()
	{
		using var memoryStream = new MemoryStream();
		using var writer = new BinaryWriter(memoryStream);

		var cells = Tiles.ToDictionary(t => (t.X, t.Y), t => t.Tile);

		// every cell, as a copy writes them, so pasting into a larger selection repeats the block at its full size
		for (var x = 0; x < Width; x++)
		for (var y = 0; y < Height; y++)
		{
			writer.Write(x);
			writer.Write(y);

			if (cells.TryGetValue((x, y), out var tile))
			{
				writer.Write(true);
				writer.Write(tile.ToString());
			}
			else
			{
				writer.Write(false);
			}
		}

		writer.Flush();

		Clipboard.SetDataObject(new DataObject(ClipboardManager.TilesFormat, memoryStream.ToArray()), copy: true);
	}

	/// <summary>
	/// Sets the block's tiles into a region with its top left at <paramref name="left"/>, <paramref name="top"/>;
	/// cells of the block with no tile leave the region's tile as it is.
	/// </summary>
	public void Paste(SegmentRegion region, int left, int top, MapEdit edit)
	{
		foreach (var (x, y, tile) in Tiles)
		{
			var element = new XElement(tile);
			var (mx, my) = (left + x, top + y);

			element.SetAttributeValue("x", mx);
			element.SetAttributeValue("y", my);

			edit?.Capture(region, mx, my);
			// a tile works out its own look as it is made
			region.SetTile(mx, my, new SegmentTile(element));
		}
	}

	/// <summary>A region holding just the block's tiles at their offsets, for a preview picture.</summary>
	public SegmentRegion ToRegion()
	{
		var region = new SegmentRegion(0);

		foreach (var (x, y, tile) in Tiles)
		{
			var element = new XElement(tile);

			element.SetAttributeValue("x", x);
			element.SetAttributeValue("y", y);

			region.SetTile(x, y, new SegmentTile(element));
		}

		return region;
	}

	public XElement ToXml(string name)
		=> new("stamp", new XAttribute("name", name), new XAttribute("width", Width), new XAttribute("height", Height),
			Tiles.Select(t => new XElement("cell", new XAttribute("x", t.X), new XAttribute("y", t.Y), new XElement(t.Tile))));

	public static TileBlock FromXml(XElement element)
		=> new((int)element.Attribute("width"), (int)element.Attribute("height"),
			element.Elements("cell").Select(c => ((int)c.Attribute("x"), (int)c.Attribute("y"), c.Elements().First())));
}

/// <summary>Saved stamps: blocks of tiles kept between sessions, one file each in .storage\Stamps.</summary>
public static class Stamps
{
	public static readonly string Folder = Path.Combine(".storage", "Stamps");

	public static List<(string Name, string Path)> List()
	{
		if (!Directory.Exists(Folder))
			return new();

		return Directory.GetFiles(Folder, "*.stamp").Select(path => (Name: ReadName(path), Path: path))
			.OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase).ToList();
	}

	public static TileBlock Load(string path) => TileBlock.FromXml(XElement.Load(path));

	public static string Save(string name, TileBlock block)
	{
		Directory.CreateDirectory(Folder);

		var file = string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
		var path = Path.Combine(Folder, file + ".stamp");

		block.ToXml(name).Save(path);

		return path;
	}

	private static string ReadName(string path)
	{
		try
		{
			// the name is the root's first attribute; reading the whole file just for it would be slow for big stamps
			using var reader = System.Xml.XmlReader.Create(path);

			if (reader.MoveToContent() == System.Xml.XmlNodeType.Element && reader.GetAttribute("name") is { } name)
				return name;
		}
		catch (Exception)
		{
			// a file that doesn't read is listed by its file name
		}

		return Path.GetFileNameWithoutExtension(path);
	}
}
