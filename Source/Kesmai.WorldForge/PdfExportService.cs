using iTextSharp.text;
using iTextSharp.text.pdf;
using Kesmai.WorldForge.Editor;
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Threading.Tasks;
using Kesmai.WorldForge.UI.Windows;

namespace Kesmai.WorldForge
{
    public class PdfExportService
    {
        private const int TileSize = 45; // Confirm this matches your application's tile size

        private readonly TerrainManager _terrainManager;

        public PdfExportService(TerrainManager terrainManager)
        {
            _terrainManager = terrainManager;
        }

        public async Task ExportCurrentViewAsync(SegmentRegion region, string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                Document document = new Document(PageSize.A0.Rotate(), 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, fs);

                document.Open();

                AddRegionInfo(document, region);

                // Create a new ProgressBarWindow and show it
                var progressBarWindow = new ProgressBarWindow();
                progressBarWindow.Show();

                // Create a Progress<int> object and provide a callback to update the progress bar
                var progress = new Progress<int>(value =>
                {
                    // Update the progress bar
                    progressBarWindow.UpdateProgress(value);
                });

                await RenderFullRegionAsync(document, region, progress);

                // Close the ProgressBarWindow when the operation is complete
                progressBarWindow.Close();
                MessageBox.Show("PDF exported successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                AddLegend(document);

                document.Close();
            }
            
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }

        private async Task RenderFullRegionAsync(Document document, SegmentRegion region, IProgress<int> progress)
        {
            var bounds = GetRegionBounds(region);
            int width = (bounds.right - bounds.left + 1) * TileSize;
            int height = (bounds.bottom - bounds.top + 1) * TileSize;

            Debug.WriteLine($"Bitmap Size: Width={width}, Height={height}");

            using (Bitmap bitmap = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.White);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                    var tiles = region.GetTiles().ToList();
                    for (int i = 0; i < tiles.Count; i++)
                    {
                        var tile = tiles[i];
                        if (tile != null)
                        {
                            var x = (tile.X - bounds.left) * (TileSize * .5);
                            var y = (tile.Y - bounds.top) * (TileSize * .5);
                            Debug.WriteLine($"Drawing Tile: X={tile.X}, Y={tile.Y}, DrawX={x}, DrawY={y}");
                            RenderTile(g, tile, (int)x, (int)y);
                        }

                        // Report progress
                        int percentComplete = (i + 1) * 100 / tiles.Count;
                        progress.Report(percentComplete);

                        // Use Task.Delay to simulate asynchronous work
                        await Task.Delay(1);
                    }
                }

                // For debugging: Save the bitmap to a file to inspect it
                bitmap.Save("debug_output.png", ImageFormat.Png);

                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    byte[] imageBytes = ms.ToArray();

                    iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageBytes);

                    // Scale the image to fit the page
                    float pageWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
                    float pageHeight = document.PageSize.Height - document.TopMargin - document.BottomMargin;
                    image.ScaleToFit(pageWidth, pageHeight);

                    document.Add(image);
                }
            }
        }

        private void AddRegionInfo(Document document, SegmentRegion region)
        {
            var bounds = GetRegionBounds(region);
            Paragraph info = new Paragraph();
            info.Add(new Chunk($"Region: {region.Name}\n", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14)));
            info.Add($"ID: {region.ID}\n");
            info.Add($"Size: {bounds.right - bounds.left + 1}x{bounds.bottom - bounds.top + 1}\n");
            info.Add($"Elevation: {region.Elevation}\n");
            document.Add(info);
            document.Add(new Paragraph("\n"));
        }

        private (int left, int top, int right, int bottom) GetRegionBounds(SegmentRegion region)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;

            foreach (var tile in region.GetTiles())
            {
                minX = Math.Min(minX, tile.X);
                minY = Math.Min(minY, tile.Y);
                maxX = Math.Max(maxX, tile.X);
                maxY = Math.Max(maxY, tile.Y);
            }

            Debug.WriteLine($"Region Bounds: Left={minX}, Top={minY}, Right={maxX}, Bottom={maxY}");
            return (minX, minY, maxX, maxY);
        }

        private void RenderTile(Graphics g, SegmentTile tile, int x, int y)
        {
            foreach (var render in tile.Renders)
            {
                var sprite = render.Layer.Sprite;
                if (sprite != null && sprite.Texture != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        sprite.Texture.SaveAsPng(ms, sprite.Texture.Width, sprite.Texture.Height);
                        ms.Seek(0, SeekOrigin.Begin);
                        using (var spriteImage = System.Drawing.Image.FromStream(ms))
                        {
                            var destRect = new System.Drawing.Rectangle(x-22, y, TileSize, TileSize);

                            // Apply color tint
                            var colorMatrix = new ColorMatrix();
                            colorMatrix.Matrix33 = render.Color.A / 255f; // Alpha
                            colorMatrix.Matrix00 = render.Color.R / 255f; // Red
                            colorMatrix.Matrix11 = render.Color.G / 255f; // Green
                            colorMatrix.Matrix22 = render.Color.B / 255f; // Blue

                            var imageAttributes = new ImageAttributes();
                            imageAttributes.SetColorMatrix(colorMatrix);

                            // Draw the image without any spacing
                            g.DrawImage(spriteImage, destRect, 0, 0, sprite.Texture.Width, sprite.Texture.Height, GraphicsUnit.Pixel, imageAttributes);
                        }
                    }
                }
            }
        }

        private void AddLegend(Document document)
        {
            // ... (unchanged)
        }
    }
}