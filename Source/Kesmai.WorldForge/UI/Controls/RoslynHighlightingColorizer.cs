using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using RoslynDocument = Microsoft.CodeAnalysis.Document;

namespace Kesmai.WorldForge.UI.Controls;

/// <summary>
/// Applies syntax highlighting based on Roslyn classification results.
/// </summary>
public class RoslynHighlightingColorizer : DocumentColorizingTransformer
{
    private IList<ClassifiedSpan> _classifications = new List<ClassifiedSpan>();

    private static readonly Dictionary<string, Brush> _brushes = new()
    {
        { ClassificationTypeNames.Keyword, CreateBrush(0x56, 0x9C, 0xD6) },
        { ClassificationTypeNames.StringLiteral, CreateBrush(0xD6, 0x9D, 0x85) },
        { ClassificationTypeNames.Comment, CreateBrush(0x00, 0x80, 0x00) },
        { ClassificationTypeNames.ClassName, CreateBrush(0x2B, 0x91, 0xAF) },
        { ClassificationTypeNames.NumericLiteral, CreateBrush(0xB5, 0xCE, 0xA8) },
    };

    private static Brush CreateBrush(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }

    /// <summary>
    /// Updates the classification spans for the specified document.
    /// </summary>
    public async Task UpdateAsync(RoslynDocument document)
    {
        var text = await document.GetTextAsync().ConfigureAwait(true);
        _classifications = (await Classifier.GetClassifiedSpansAsync(document, new TextSpan(0, text.Length)).ConfigureAwait(true)).ToList();
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        int lineStart = line.Offset;
        int lineEnd = line.EndOffset;

        foreach (var span in _classifications)
        {
            if (span.TextSpan.End <= lineStart)
                continue;
            if (span.TextSpan.Start >= lineEnd)
                break;

            if (_brushes.TryGetValue(span.ClassificationType, out var brush))
            {
                int start = System.Math.Max(span.TextSpan.Start, lineStart);
                int end = System.Math.Min(span.TextSpan.End, lineEnd);

                ChangeLinePart(start, end, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(brush);
                });
            }
        }
    }
}
