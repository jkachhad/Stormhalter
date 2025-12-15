using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge;

public sealed class ComponentImageCache
{
    private readonly Dictionary<IComponentProvider, WriteableBitmap> _renders = new();

    public WriteableBitmap Get(IComponentProvider component)
    {
        if (!_renders.TryGetValue(component, out var bmp))
            _renders[component] = bmp = Update(component);

        return bmp;
    }

    public WriteableBitmap Update(IComponentProvider component, bool forceUpdate = false)
    {
        // Build render list (layer + tint + order) from your component model.
        var renderList = new List<TerrainRender>();
        
        foreach (var render in component.GetRenders())
            renderList.AddRange(render.Terrain.Select(layer => new TerrainRender(layer, render.Color)));

        // Prepare (normalize to Pbgra32, freeze) and compute bounds once.
        var layers = PrepareLayers(renderList);

        if (layers.Count == 0)
        {
            var empty = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Pbgra32, null);
            
            empty.Lock(); 
            empty.AddDirtyRect(new Int32Rect(0, 0, 1, 1)); 
            empty.Unlock(); 
            empty.Freeze();
            
            _renders[component] = empty;
            return empty;
        }

        int maxWidth = layers.Max(l => l.OffsetX + l.Width);
        int maxHeight = layers.Max(l => l.OffsetY + l.Height);

        // Reuse existing WB if size matches; otherwise create new.
        if (!_renders.TryGetValue(component, out var wb) || wb.PixelWidth != maxWidth || wb.PixelHeight != maxHeight || wb.Format != PixelFormats.Pbgra32 || forceUpdate)
            _renders[component] = (wb = new WriteableBitmap(maxWidth, maxHeight, 96, 96, PixelFormats.Pbgra32, null));

        CompositeInto(wb, layers);

        wb.Freeze(); // freeze snapshot result (we’ll recreate on next Update)
        return wb;
    }

    // --- Preparation ---

    private static List<PreparedLayer> PrepareLayers(IEnumerable<TerrainRender> renders)
    {
        var list = new List<PreparedLayer>();

        foreach (var r in renders.OrderBy(r => r.Layer.Order))
        {
            var sprite = r.Layer.Sprite;
            if (sprite?.Bitmap == null) continue;

            // Normalize to Pbgra32 once and Freeze so CopyPixels is cheap.
            BitmapSource src = sprite.Bitmap;
            if (src is BitmapImage bi && bi.IsDownloading) continue;

            if (src.Format != PixelFormats.Pbgra32)
                src = new FormatConvertedBitmap(src, PixelFormats.Pbgra32, null, 0);

            if (src.CanFreeze) src.Freeze();

            var offset = sprite.Offset; // Vector2F
            var tint = r.Color;         // System.Windows.Media.Color (ARGB)

            list.Add(new PreparedLayer
            {
                Source = src,
                Width = src.PixelWidth,
                Height = src.PixelHeight,
                Stride = ((src.PixelWidth * 32 + 31) / 32) * 4,
                OffsetX = (int)offset.X,
                OffsetY = (int)offset.Y,
                // Keep tint factors as bytes; apply in-premultiplied space during blend.
                TintR = tint.R,
                TintG = tint.G,
                TintB = tint.B,
                TintA = tint.A, // kept in case you later want to modulate alpha too
            });
        }

        return list;
    }

    // --- Compositing ---

    private static void CompositeInto(WriteableBitmap wb, List<PreparedLayer> layers)
    {
        wb.Lock();
        try
        {
            // Clear output to transparent
            unsafe
            {
                new Span<byte>((void*)wb.BackBuffer, wb.BackBufferStride * wb.PixelHeight).Clear();
            }

            foreach (var layer in layers) BlendLayer(wb, layer);
            wb.AddDirtyRect(new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight));
        }
        finally
        {
            wb.Unlock();
        }
    }

    private static void BlendLayer(WriteableBitmap dst, PreparedLayer layer)
    {
        // Intersect layer rect with dst
        int x0 = Math.Max(0, layer.OffsetX);
        int y0 = Math.Max(0, layer.OffsetY);
        int x1 = Math.Min(dst.PixelWidth, layer.OffsetX + layer.Width);
        int y1 = Math.Min(dst.PixelHeight, layer.OffsetY + layer.Height);

        if (x0 >= x1 || y0 >= y1) return;

        // Reusable per-thread row buffer; stackalloc avoids heap traffic
        int cols = x1 - x0;
        int rows = y1 - y0;

        // For larger images, you can parallelize rows. Keep it simple (single-thread) unless needed.
        // Parallel.For(0, rows, r => BlendRow(...));
        for (int r = 0; r < rows; r++)
            BlendRow(dst, layer, y0 + r, x0, cols, srcRowY: (y0 + r) - layer.OffsetY);
    }

    private static void BlendRow(WriteableBitmap dst, PreparedLayer layer, int dstY, int dstX, int cols, int srcRowY)
    {
        int byteCount = cols * 4;

        unsafe
        {
            // Prefer stackalloc for small rows to avoid heap traffic.
            if (byteCount <= 32_768) // 32 KB threshold; tweak if you like
            {
                byte* pScratch = stackalloc byte[byteCount];
                var rect = new Int32Rect(dstX - layer.OffsetX, srcRowY, cols, 1);
                layer.Source.CopyPixels(rect, (IntPtr)pScratch, byteCount, cols * 4);

                byte* pDstRow = (byte*)dst.BackBuffer + dstY * dst.BackBufferStride + dstX * 4;
                TintAndBlendPremulRow(pScratch, pDstRow, cols, layer.TintR, layer.TintG, layer.TintB /*, layer.TintA*/);
            }
            else
            {
                // Large row: rent pooled array to keep GC pressure low.
                byte[] scratch = ArrayPool<byte>.Shared.Rent(byteCount);
                try
                {
                    fixed (byte* pScratch = scratch)
                    {
                        var rect = new Int32Rect(dstX - layer.OffsetX, srcRowY, cols, 1);
                        layer.Source.CopyPixels(rect, (IntPtr)pScratch, byteCount, cols * 4);

                        byte* pDstRow = (byte*)dst.BackBuffer + dstY * dst.BackBufferStride + dstX * 4;
                        TintAndBlendPremulRow(pScratch, pDstRow, cols, layer.TintR, layer.TintG, layer.TintB /*, layer.TintA*/);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(scratch);
                }
            }
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void TintAndBlendPremulRow(byte* src, byte* dst, int cols, byte tr, byte tg, byte tb /*, byte ta*/)
    {
        // Pbgra32 (premultiplied): [B,G,R,A]
        // Apply RGB tint in premult space: c' = (c * tint) / 255
        // Then standard src-over: dst = src + dst*(1 - a_s)

        for (int x = 0; x < cols; x++)
        {
            int sB = src[0], sG = src[1], sR = src[2], sA = src[3];

            // Premult tint (alpha unaffected, mimic your original RGB-only tint)
            sB = (sB * tb + 127) / 255;
            sG = (sG * tg + 127) / 255;
            sR = (sR * tr + 127) / 255;
            // If you want tint to affect alpha too, uncomment next line:
            // sA = (sA * ta + 127) / 255;

            int dB = dst[0], dG = dst[1], dR = dst[2], dA = dst[3];
            int invA = 255 - sA;

            dst[0] = (byte)(sB + ((dB * invA + 127) / 255));
            dst[1] = (byte)(sG + ((dG * invA + 127) / 255));
            dst[2] = (byte)(sR + ((dR * invA + 127) / 255));
            dst[3] = (byte)(sA + ((dA * invA + 127) / 255));

            src += 4; dst += 4;
        }
    }

    private sealed class PreparedLayer
    {
        public BitmapSource Source;
        public int Width, Height, Stride;
        public int OffsetX, OffsetY;
        public byte TintR, TintG, TintB, TintA;
    }
}
