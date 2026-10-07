using System;
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
    // weakly keyed, so images are released with their provider (deleted templates and brushes,
    // providers removed from a template, a closed segment) without explicit removal.
    private readonly ConditionalWeakTable<IComponentProvider, WriteableBitmap> _renders = new();

    // premultiplied pixels per sprite; many components share sprites, so each is converted once.
    private readonly Dictionary<GameSprite, SpritePixels> _spritePixels = new();

    public WriteableBitmap Get(IComponentProvider component)
    {
        if (!_renders.TryGetValue(component, out var bmp))
            bmp = Update(component);

        return bmp;
    }

    /// <summary>
    /// Composites a new image for the component and caches it, replacing any previous image.
    /// </summary>
    public WriteableBitmap Update(IComponentProvider component)
    {
        // Build render list (layer + tint + order) from your component model.
        var renderList = new List<TerrainRender>();
        
        foreach (var render in component.GetRenders())
            renderList.AddRange(render.Terrain.Select(layer => new TerrainRender(layer, render.Color)));

        // Resolve cached pixels and compute bounds once.
        var layers = PrepareLayers(renderList);

        if (layers.Count == 0)
        {
            var empty = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Pbgra32, null);
            
            empty.Lock(); 
            empty.AddDirtyRect(new Int32Rect(0, 0, 1, 1)); 
            empty.Unlock(); 
            empty.Freeze();
            
            _renders.AddOrUpdate(component, empty);
            return empty;
        }

        int maxWidth = layers.Max(l => l.OffsetX + l.Pixels.Width);
        int maxHeight = layers.Max(l => l.OffsetY + l.Pixels.Height);

        // Images are frozen once composited, so each update creates a new bitmap.
        var wb = new WriteableBitmap(maxWidth, maxHeight, 96, 96, PixelFormats.Pbgra32, null);

        CompositeInto(wb, layers);

        wb.Freeze();

        _renders.AddOrUpdate(component, wb);
        return wb;
    }

    // --- Preparation ---

    private List<PreparedLayer> PrepareLayers(IEnumerable<TerrainRender> renders)
    {
        var list = new List<PreparedLayer>();

        foreach (var r in renders.OrderBy(r => r.Layer.Order))
        {
            var sprite = r.Layer.Sprite;
            if (sprite?.Bitmap == null) continue;

            var offset = sprite.Offset; // Vector2F
            var tint = r.Color;         // System.Windows.Media.Color (ARGB)

            list.Add(new PreparedLayer
            {
                Pixels = GetSpritePixels(sprite),
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

    private SpritePixels GetSpritePixels(GameSprite sprite)
    {
        if (_spritePixels.TryGetValue(sprite, out var pixels))
            return pixels;

        // Normalize to Pbgra32 and copy the whole image out in one call.
        BitmapSource src = sprite.Bitmap;

        if (src.Format != PixelFormats.Pbgra32)
            src = new FormatConvertedBitmap(src, PixelFormats.Pbgra32, null, 0);

        var width = src.PixelWidth;
        var height = src.PixelHeight;
        var stride = width * 4;
        var data = new byte[stride * height];

        src.CopyPixels(data, stride, 0);

        _spritePixels[sprite] = pixels = new SpritePixels(data, width, height, stride);
        return pixels;
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

    private static unsafe void BlendLayer(WriteableBitmap dst, PreparedLayer layer)
    {
        var pixels = layer.Pixels;

        // Intersect layer rect with dst
        int x0 = Math.Max(0, layer.OffsetX);
        int y0 = Math.Max(0, layer.OffsetY);
        int x1 = Math.Min(dst.PixelWidth, layer.OffsetX + pixels.Width);
        int y1 = Math.Min(dst.PixelHeight, layer.OffsetY + pixels.Height);

        if (x0 >= x1 || y0 >= y1) return;

        int cols = x1 - x0;

        fixed (byte* pSrc = pixels.Data)
        {
            for (int y = y0; y < y1; y++)
            {
                byte* pSrcRow = pSrc + (y - layer.OffsetY) * pixels.Stride + (x0 - layer.OffsetX) * 4;
                byte* pDstRow = (byte*)dst.BackBuffer + y * dst.BackBufferStride + x0 * 4;

                TintAndBlendPremulRow(pSrcRow, pDstRow, cols, layer.TintR, layer.TintG, layer.TintB /*, layer.TintA*/);
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

    private sealed record SpritePixels(byte[] Data, int Width, int Height, int Stride);

    private sealed class PreparedLayer
    {
        public SpritePixels Pixels;
        public int OffsetX, OffsetY;
        public byte TintR, TintG, TintB, TintA;
    }
}
