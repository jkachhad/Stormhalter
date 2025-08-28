using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
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
    private readonly ClassificationHighlightColors _colors = new();
    private IList<ClassifiedSpan> _classifications = new List<ClassifiedSpan>();

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

            var color = _colors.GetColor(span.ClassificationType);
            if (color != null)
            {
                int start = Math.Max(span.TextSpan.Start, lineStart);
                int end = Math.Min(span.TextSpan.End, lineEnd);

                ChangeLinePart(start, end, element =>
                {
                    var foreground = color.Foreground?.GetBrush(CurrentContext);
                    if (foreground != null)
                        element.TextRunProperties.SetForegroundBrush(foreground);

                    var background = color.Background?.GetBrush(CurrentContext);
                    if (background != null)
                        element.TextRunProperties.SetBackgroundBrush(background);

                    if (color.FontWeight != null)
                        element.TextRunProperties.SetFontWeight(color.FontWeight.Value);
                });
            }
        }
    }
}

internal sealed class ClassificationHighlightColors
{
    public HighlightingColor DefaultBrush { get; } = CreateColor(Colors.Black);

    public HighlightingColor TypeBrush { get; } = CreateColor(Colors.Teal);
    public HighlightingColor MethodBrush { get; } = CreateColor(Colors.Olive);
    public HighlightingColor ParameterBrush { get; } = CreateColor(Colors.DarkBlue);
    public HighlightingColor CommentBrush { get; } = CreateColor(Colors.Green);
    public HighlightingColor XmlCommentBrush { get; } = CreateColor(Colors.Gray);
    public HighlightingColor KeywordBrush { get; } = CreateColor(Colors.Blue);
    public HighlightingColor PreprocessorKeywordBrush { get; } = CreateColor(Colors.Gray);
    public HighlightingColor StringBrush { get; } = CreateColor(Colors.Maroon);
    public HighlightingColor BraceMatchingBrush { get; } = CreateColor(Colors.Black, Color.FromArgb(150, 219, 224, 204));
    public HighlightingColor StaticSymbolBrush { get; } = CreateColor(null, null, FontWeights.Bold);

    private readonly Lazy<ImmutableDictionary<string, HighlightingColor>> _map;

    public ClassificationHighlightColors()
    {
        _map = new Lazy<ImmutableDictionary<string, HighlightingColor>>(() => new Dictionary<string, HighlightingColor>
        {
            [ClassificationTypeNames.ClassName] = TypeBrush,
            [ClassificationTypeNames.RecordClassName] = TypeBrush,
            [ClassificationTypeNames.RecordStructName] = TypeBrush,
            [ClassificationTypeNames.StructName] = TypeBrush,
            [ClassificationTypeNames.InterfaceName] = TypeBrush,
            [ClassificationTypeNames.DelegateName] = TypeBrush,
            [ClassificationTypeNames.EnumName] = TypeBrush,
            [ClassificationTypeNames.ModuleName] = TypeBrush,
            [ClassificationTypeNames.TypeParameterName] = TypeBrush,
            [ClassificationTypeNames.MethodName] = MethodBrush,
            [ClassificationTypeNames.ExtensionMethodName] = MethodBrush,
            [ClassificationTypeNames.ParameterName] = ParameterBrush,
            [ClassificationTypeNames.Comment] = CommentBrush,
            [ClassificationTypeNames.StaticSymbol] = StaticSymbolBrush,
            [ClassificationTypeNames.XmlDocCommentAttributeName] = XmlCommentBrush,
            [ClassificationTypeNames.XmlDocCommentAttributeQuotes] = XmlCommentBrush,
            [ClassificationTypeNames.XmlDocCommentAttributeValue] = XmlCommentBrush,
            [ClassificationTypeNames.XmlDocCommentCDataSection] = XmlCommentBrush,
            [ClassificationTypeNames.XmlDocCommentComment] = XmlCommentBrush,
            [ClassificationTypeNames.XmlDocCommentDelimiter] = XmlCommentBrush,
            [ClassificationTypeNames.XmlDocCommentEntityReference] = XmlCommentBrush,
            [ClassificationTypeNames.XmlDocCommentName] = XmlCommentBrush,
            [ClassificationTypeNames.XmlDocCommentProcessingInstruction] = XmlCommentBrush,
            [ClassificationTypeNames.XmlDocCommentText] = CommentBrush,
            [ClassificationTypeNames.Keyword] = KeywordBrush,
            [ClassificationTypeNames.ControlKeyword] = KeywordBrush,
            [ClassificationTypeNames.PreprocessorKeyword] = PreprocessorKeywordBrush,
            [ClassificationTypeNames.StringLiteral] = StringBrush,
            [ClassificationTypeNames.VerbatimStringLiteral] = StringBrush,
            [AdditionalClassificationTypeNames.BraceMatching] = BraceMatchingBrush,
        }.ToImmutableDictionary());
    }

    public HighlightingColor GetColor(string classificationType)
    {
        return _map.Value.TryGetValue(classificationType, out var color) ? color : DefaultBrush;
    }

    private static HighlightingColor CreateColor(Color? foreground, Color? background = null, FontWeight? fontWeight = null)
    {
        var color = new HighlightingColor();

        if (foreground.HasValue)
            color.Foreground = new SimpleHighlightingBrush(foreground.Value);
        if (background.HasValue)
            color.Background = new SimpleHighlightingBrush(background.Value);
        if (fontWeight.HasValue)
            color.FontWeight = fontWeight.Value;

        color.Freeze();
        return color;
    }
}

internal static class AdditionalClassificationTypeNames
{
    public const string BraceMatching = nameof(BraceMatching);
}
