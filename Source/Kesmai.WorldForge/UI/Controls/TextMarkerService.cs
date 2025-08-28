using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace Kesmai.WorldForge.UI.Controls;

internal sealed class TextMarkerService : IBackgroundRenderer
{
    private readonly List<TextMarker> _markers = new();

    public TextMarkerService(TextDocument document)
    {
        // document parameter reserved for future use
    }

    public KnownLayer Layer => KnownLayer.Selection;

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (_markers.Count == 0 || !textView.VisualLinesValid)
            return;

        foreach (var marker in _markers)
        {
            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
            {
                var geometry = CreateSquiggle(rect.BottomLeft, rect.BottomRight);
                drawingContext.DrawGeometry(null, new Pen(Brushes.Red, 1), geometry);
            }
        }
    }

    private static StreamGeometry CreateSquiggle(Point start, Point end)
    {
        const double step = 2;
        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();
        var x = start.X;
        var y = start.Y;
        var up = true;
        ctx.BeginFigure(new Point(x, y), false, false);
        while (x < end.X)
        {
            x += step;
            y += up ? -step : step;
            ctx.LineTo(new Point(x, y), true, false);
            up = !up;
        }
        geometry.Freeze();
        return geometry;
    }

    public void Mark(int startOffset, int length)
    {
        _markers.Add(new TextMarker(startOffset, length));
    }

    public void Clear()
    {
        _markers.Clear();
    }

    public TextMarker? GetMarkerAt(int offset)
    {
        return _markers.FirstOrDefault(m => offset >= m.StartOffset && offset <= m.EndOffset);
    }

    internal sealed class TextMarker : TextSegment
    {
        public TextMarker(int start, int length)
        {
            StartOffset = start;
            Length = length;
        }
    }
}

