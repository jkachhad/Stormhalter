using iTextSharp.text;
using iTextSharp.text.pdf;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge;

/// <summary>Renders an XML-backed region to a single-page PDF map.</summary>
public static class PdfExportService
{
	private const int TileSize = 55;
	private const int TerrainOffset = 45;

	public static async Task ExportAsync(SegmentRegion region, string filePath,
		IProgress<int> progress = null)
	{
		ArgumentNullException.ThrowIfNull(region);
		ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

		region.EnsureTilesUpdated();

		var tiles = region.GetTiles().ToList();
		if (tiles.Count == 0)
			throw new InvalidOperationException("The selected region has no tiles to export.");

		var bounds = GetBounds(tiles);
		var width = checked((bounds.Right - bounds.Left + 1) * TileSize + TerrainOffset * 2);
		var height = checked((bounds.Bottom - bounds.Top + 1) * TileSize + TerrainOffset * 2);

		// GDI+ bitmaps have practical size limits; give a useful error instead of an
		// opaque OutOfMemoryException for unusually large/sparse maps.
		if (width > 32767 || height > 32767 || (long)width * height > 400_000_000)
			throw new InvalidOperationException(
				$"The region bounds ({width} x {height} pixels) are too large to export as one page.");

		var textureCache = new Dictionary<Texture2D, Bitmap>();
		try
		{
			// Reading a texture back from the GPU and encoding it as PNG is expensive.
			// A map reuses a small set of textures many times, so perform that work once.
			var textures = tiles
				.SelectMany(tile => tile.Renders)
				.Select(render => render.Layer.Sprite?.Texture)
				.Where(texture => texture is not null)
				.Distinct()
				.ToList();

			for (var index = 0; index < textures.Count; index++)
			{
				var texture = textures[index];
				using var textureStream = new MemoryStream();
				texture.SaveAsPng(textureStream, texture.Width, texture.Height);
				textureStream.Position = 0;
				using var source = System.Drawing.Image.FromStream(textureStream);
				textureCache.Add(texture, new Bitmap(source));

				progress?.Report(textures.Count == 0 ? 10 : (index + 1) * 10 / textures.Count);
				if (index % 10 == 0)
					await Task.Yield();
			}

			using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			using (var graphics = Graphics.FromImage(bitmap))
			{
				// PDFs are intended for documents/upload, so transparent terrain
				// pixels should reveal a white page rather than a black render-target
				// background. Layers still composite normally above this base.
				graphics.Clear(Color.Black);
				graphics.CompositingMode = CompositingMode.SourceOver;
				graphics.CompositingQuality = CompositingQuality.GammaCorrected;
				graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

				for (var index = 0; index < tiles.Count; index++)
				{
					var tile = tiles[index];
					var x = (tile.X - bounds.Left) * TileSize + TerrainOffset;
					var y = (tile.Y - bounds.Top) * TileSize + TerrainOffset;
					RenderTile(graphics, tile, x, y, textureCache);

					progress?.Report(10 + (index + 1) * 70 / tiles.Count);
					if (index % 25 == 0)
						await Task.Yield();
				}
			}

			progress?.Report(85);
			await Task.Yield();

			var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
			if (!String.IsNullOrEmpty(directory))
				Directory.CreateDirectory(directory);

			using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
			using var document = new Document(PageSize.A0.Rotate(), 36, 36, 36, 36);
			PdfWriter.GetInstance(document, stream);
			document.Open();

			var title = new Paragraph($"{region.Name} (Region {region.ID})",
				FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14));
			title.SpacingAfter = 8;
			document.Add(title);

			using var imageStream = new MemoryStream();
			bitmap.Save(imageStream, ImageFormat.Png);
			progress?.Report(95);
			var image = iTextSharp.text.Image.GetInstance(imageStream.ToArray());
			image.ScaleToFit(
				document.PageSize.Width - document.LeftMargin - document.RightMargin,
				document.PageSize.Height - document.TopMargin - document.BottomMargin - title.TotalLeading);
			image.Alignment = Element.ALIGN_CENTER;
			document.Add(image);
			progress?.Report(100);
		}
		finally
		{
			foreach (var image in textureCache.Values)
				image.Dispose();
		}
	}

	private static void RenderTile(Graphics graphics, SegmentTile tile, int x, int y,
		IReadOnlyDictionary<Texture2D, Bitmap> textureCache)
	{
		// Floor tint alpha is editor metadata and must not make the map base
		// transparent in an exported document. Keep RGB tinting, but reserve
		// alpha modulation for overlays (hidden markers, traps, effects, etc.).
		var floorLayers = tile.Providers
			.SelectMany(provider => provider.GetComponents())
			.OfType<FloorComponent>()
			.SelectMany(floor => floor.GetRenders())
			.SelectMany(render => render.Terrain)
			.ToHashSet();

		foreach (var render in tile.Renders)
		{
			var sprite = render.Layer.Sprite;
			if (sprite?.Texture is null)
				continue;

			if (!textureCache.TryGetValue(sprite.Texture, out var spriteImage))
				continue;

			using var attributes = new ImageAttributes();
			var tint = new ColorMatrix
			{
				Matrix00 = render.Color.R / 255f,
				Matrix11 = render.Color.G / 255f,
				Matrix22 = render.Color.B / 255f,
				Matrix33 = floorLayers.Contains(render.Layer)
					? 1f
					: render.Color.A / 255f
			};
			attributes.SetColorMatrix(tint);

			var offsetX = (int)Math.Floor(sprite.Offset.X);
			var offsetY = (int)Math.Floor(sprite.Offset.Y);
			var resolution = Math.Max(1, sprite.Resolution);
			var renderWidth = Math.Max(1, sprite.Texture.Width / resolution);
			var renderHeight = Math.Max(1, sprite.Texture.Height / resolution);
			var destination = new System.Drawing.Rectangle(
				x - TerrainOffset + offsetX, y - TerrainOffset + offsetY,
				renderWidth, renderHeight);
			graphics.DrawImage(spriteImage, destination, 0, 0, sprite.Texture.Width,
				sprite.Texture.Height, GraphicsUnit.Pixel, attributes);
		}
	}

	private static (int Left, int Top, int Right, int Bottom) GetBounds(
		IEnumerable<SegmentTile> tiles)
	{
		var left = int.MaxValue;
		var top = int.MaxValue;
		var right = int.MinValue;
		var bottom = int.MinValue;

		foreach (var tile in tiles)
		{
			left = Math.Min(left, tile.X);
			top = Math.Min(top, tile.Y);
			right = Math.Max(right, tile.X);
			bottom = Math.Max(bottom, tile.Y);
		}

		return (left, top, right, bottom);
	}
}
