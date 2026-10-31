using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge;

/// <summary>
/// A small picture of a region for the Regions grid menu: one pixel per tile, the color of what is drawn on it (each
/// sprite's average color over the tile, its layers laid over one another), on black where there is no tile.
/// </summary>
public static class RegionPreview
{
	// the tile's own square inside a sprite: the map draws a sprite 45 pixels up and left of its 55 pixel tile
	private const int SpriteInset = 45;
	private const int TileSpan = 55;

	// average color of each sprite over its tile square, premultiplied; many tiles share sprites
	private static readonly Dictionary<GameSprite, (double B, double G, double R, double A)> _averages = new();

	public static BitmapSource Render(SegmentRegion region)
	{
		var tiles = region.GetTiles().ToList();

		if (tiles.Count == 0)
			return null;

		var left = tiles.Min(t => t.X);
		var top = tiles.Min(t => t.Y);
		var width = tiles.Max(t => t.X) - left + 1;
		var height = tiles.Max(t => t.Y) - top + 1;

		// a region far bigger than a picture needs is drawn at one pixel per few tiles
		var step = Math.Max(1, (int)Math.Ceiling(Math.Max(width, height) / 256.0));
		var pixelsWide = (width + step - 1) / step;
		var pixelsHigh = (height + step - 1) / step;
		var pixels = new byte[pixelsWide * pixelsHigh * 4];

		for (var i = 3; i < pixels.Length; i += 4)
			pixels[i] = 255;

		foreach (var tile in tiles)
		{
			var px = (tile.X - left) / step;
			var py = (tile.Y - top) / step;
			var (b, g, r) = TileColor(tile);
			var at = (py * pixelsWide + px) * 4;

			pixels[at] = b;
			pixels[at + 1] = g;
			pixels[at + 2] = r;
		}

		var bitmap = BitmapSource.Create(pixelsWide, pixelsHigh, 96, 96, PixelFormats.Bgra32, null, pixels, pixelsWide * 4);
		bitmap.Freeze();
		return bitmap;
	}

	private static (byte B, byte G, byte R) TileColor(SegmentTile tile)
	{
		double b = 0, g = 0, r = 0;

		// a tile only has renders once its terrain has been worked out
		if (tile.Renders is not { } renders)
			return (0, 0, 0);

		foreach (var render in renders.OrderBy(render => render.Layer.Order))
		{
			var sprite = render.Layer.Sprite;

			if (sprite?.Bitmap is null)
				continue;

			var average = Average(sprite);
			var tint = render.Color;

			// the sprite's color, tinted, laid over what is beneath it
			var cover = 1 - average.A;
			b = average.B * tint.B / 255.0 + b * cover;
			g = average.G * tint.G / 255.0 + g * cover;
			r = average.R * tint.R / 255.0 + r * cover;
		}

		return ((byte)Math.Clamp(b, 0, 255), (byte)Math.Clamp(g, 0, 255), (byte)Math.Clamp(r, 0, 255));
	}

	private static (double B, double G, double R, double A) Average(GameSprite sprite)
	{
		if (_averages.TryGetValue(sprite, out var average))
			return average;

		BitmapSource source = sprite.Bitmap;

		if (source.Format != PixelFormats.Pbgra32)
			source = new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);

		var resolution = Math.Max(1, sprite.Resolution);
		var x0 = (int)Math.Round((SpriteInset - sprite.Offset.X) * resolution);
		var y0 = (int)Math.Round((SpriteInset - sprite.Offset.Y) * resolution);
		var rect = new Int32Rect(Math.Max(0, x0), Math.Max(0, y0), 0, 0);

		rect.Width = Math.Min(source.PixelWidth, x0 + TileSpan * resolution) - rect.X;
		rect.Height = Math.Min(source.PixelHeight, y0 + TileSpan * resolution) - rect.Y;

		// a sprite that doesn't reach the tile square (or an odd one) is averaged whole
		if (rect.Width <= 0 || rect.Height <= 0)
			rect = new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight);

		var data = new byte[rect.Width * rect.Height * 4];
		source.CopyPixels(rect, data, rect.Width * 4, 0);

		double b = 0, g = 0, r = 0, a = 0;

		for (var i = 0; i < data.Length; i += 4)
		{
			b += data[i];
			g += data[i + 1];
			r += data[i + 2];
			a += data[i + 3];
		}

		var count = data.Length / 4.0;
		average = (b / count, g / count, r / count, a / count / 255.0);
		_averages[sprite] = average;
		return average;
	}
}
