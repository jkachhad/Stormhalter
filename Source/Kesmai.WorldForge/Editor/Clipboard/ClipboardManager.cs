using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using CommonServiceLocator;
using XNA = Microsoft.Xna.Framework;

namespace Kesmai.WorldForge.Editor;

public class ClipboardManager
{
	#region Static

	/// <summary>
	/// The clipboard format for copied tiles. The data is a byte array: for each tile in the copied area, its x and y
	/// within the area, whether it has a tile, and the tile's XML. A byte array is copied out of the clipboard on every
	/// read, so each paste gets its own data and pasting the same copy again works.
	/// </summary>
	public const string TilesFormat = "Kesmai.WorldForge.Tiles";

	#endregion

	#region Fields

	#endregion

	#region Properties and Events

	#endregion

	#region Constructors and Cleanup

	/// <summary>
	/// Initializes a new instance of the <see cref="ClipboardManager"/> class.
	/// </summary>
	public ClipboardManager()
	{
	}

	#endregion

	#region Methods

	/// <summary>
	/// Copies the first currently selected area.
	/// </summary>
	public void Copy(Selection selection)
	{
		var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
		var document = presenter.ActiveDocument;

		if (document is SegmentRegion region)
		{
			var area = selection.FirstOrDefault<XNA.Rectangle>();

			using var memoryStream = new MemoryStream();
			using var binaryWriter = new BinaryWriter(memoryStream);

			if (area != default(XNA.Rectangle))
			{
				for (int x = area.Left; x < area.Right; x++)
				for (int y = area.Top; y < area.Bottom; y++)
				{
					binaryWriter.Write((int)x - area.Left);
					binaryWriter.Write((int)y - area.Top);

					var tile = region.GetTile(x, y);

					if (tile != null)
					{
						binaryWriter.Write((bool)true);
						binaryWriter.Write((string)tile.GetSerializingElement().ToString());
					}
					else
					{
						binaryWriter.Write((bool)false);
					}
				}
			}

			binaryWriter.Flush();

			Clipboard.SetDataObject(new DataObject(TilesFormat, memoryStream.ToArray()), copy: true);
		}
	}

	/// <summary>
	/// The copied tiles on the clipboard, each with its offset in the copied area, or an empty list when the clipboard
	/// holds no tiles (text, nothing, or data that can't be read).
	/// </summary>
	public static List<(int X, int Y, XElement Tile)> ReadTiles()
	{
		var tiles = new List<(int X, int Y, XElement Tile)>();

		if (Clipboard.GetDataObject() is not { } data || !data.GetDataPresent(TilesFormat) || data.GetData(TilesFormat) is not byte[] bytes)
			return tiles;

		try
		{
			using var reader = new BinaryReader(new MemoryStream(bytes));

			while (reader.BaseStream.Position < reader.BaseStream.Length)
			{
				var x = reader.ReadInt32();
				var y = reader.ReadInt32();

				if (reader.ReadBoolean())
					tiles.Add((x, y, XElement.Parse(reader.ReadString())));
			}
		}
		catch (Exception exception) when (exception is EndOfStreamException or IOException or XmlException)
		{
			// clipboard data that doesn't read as tiles pastes nothing rather than half an area
			Debug.WriteLine($"Clipboard paste: the copied tiles couldn't be read: {exception.Message}");
			tiles.Clear();
		}

		return tiles;
	}

	/// <summary>
	/// Pastes this buffer at the first selected area.
	/// </summary>
	public void Paste(Selection selection)
	{
		var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
		var document = presenter.ActiveDocument;

		if (document is SegmentRegion region && selection.Region.ID==region.ID)
		{
			var area = selection.FirstOrDefault<XNA.Rectangle>();
			var tiles = ReadTiles();

			if (area != default(XNA.Rectangle) && tiles.Count > 0)
			{
				var edit = presenter.History.Begin("Paste");

				if (area is {Width:1,Height:1})
				{
					PasteToOne(area, region, edit, tiles);
				}
				else
				{
					PasteToMany(area, region, edit, tiles);
				}

				edit.Commit();
			}
		}
	}

	private static void SetTile(SegmentRegion region, MapEdit edit, int mx, int my, XElement tile)
	{
		// each paste gets its own copy of the tile, with its new position
		var tileElement = new XElement(tile);

		tileElement.SetAttributeValue("x", mx);
		tileElement.SetAttributeValue("y", my);

		edit.Capture(region, mx, my);
		region.SetTile(mx, my, new SegmentTile(tileElement));
	}

	/// <summary>
	/// Pastes The whole buffer starting at the top left of the current 1x1 selection.
	/// </summary>
	private void PasteToOne(XNA.Rectangle area, SegmentRegion region, MapEdit edit, List<(int X, int Y, XElement Tile)> tiles)
	{
		foreach (var (x, y, tile) in tiles)
			SetTile(region, edit, area.Left + x, area.Top + y, tile);

		region.UpdateTiles();
	}

	/// <summary>
	/// Tile the buffer until it has filled the current selection
	/// </summary>
	private void PasteToMany(XNA.Rectangle area, SegmentRegion region, MapEdit edit, List<(int X, int Y, XElement Tile)> tiles)
	{
		var ox = area.Left; // region to paste into bounded by selection "area"
		var oy = area.Top;
		var maxx = area.Right-1;
		var maxy = area.Bottom-1;

		var cx = ox; //current top-left of paste itteration
		var cy = oy;

		var mx = ox; //current tile in paste operation
		var my = oy;

		var stopwatch = Stopwatch.StartNew();

		while (cx <= maxx && cy <= maxy)
		{
			//paste the buffer, but respect the right and bottom edges
			foreach (var (x, y, tile) in tiles)
			{
				if (stopwatch.Elapsed.TotalSeconds > 5)
					break;

				mx = cx + x;
				my = cy + y;

				if (mx <= maxx && my <= maxy)
					SetTile(region, edit, mx, my, tile);
			}

			if (stopwatch.Elapsed.TotalSeconds > 5)
				break;

			if (mx < maxx) { cx = mx + 1; } // if there's space to the right, move our target and run again
			else { cx = ox; cy = my + 1; } // if we've reached the end horizontally, move down and return to origin x. If this moves us below our selection, the loop will stop.
		}

		region.UpdateTiles();
	}
	#endregion
}
