using System;
using System.Collections.Generic;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Kesmai.WorldForge.Roslyn;

namespace Kesmai.WorldForge.UI.Controls;

/// <summary>
/// Text editor that displays a read-only method signature and braces while allowing
/// the user to edit only the script body.
/// </summary>
public class ScriptEditor : TextEditor
{
    private bool _isUpdating;
    private int _prefixLength;
    private int _suffixLength;
    private readonly ReadOnlyBackgroundRenderer _backgroundRenderer;
    private readonly TextMarkerService _markerService;
    private Document? _document;
    private CompletionWindow? _completionWindow;
    private static readonly ScriptWorkspace _workspace = new();

    /// <summary>
    /// Identifies the <see cref="Body"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BodyProperty =
        DependencyProperty.Register(
            nameof(Body),
            typeof(string),
            typeof(ScriptEditor),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBodyChanged));

    /// <summary>
    /// Identifies the <see cref="Header"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof(ScriptEditor),
            new FrameworkPropertyMetadata(string.Empty, OnHeaderChanged));

    /// <summary>
    /// Gets or sets the editable body of the script.
    /// </summary>
    public string Body
    {
        get => (string)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>
    /// Gets or sets the read-only method signature displayed above the body.
    /// </summary>
    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public ScriptEditor()
    {
        SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");

        _backgroundRenderer = new ReadOnlyBackgroundRenderer(this);
        TextArea.TextView.BackgroundRenderers.Add(_backgroundRenderer);

        _markerService = new TextMarkerService(Document);
        TextArea.TextView.BackgroundRenderers.Add(_markerService);

        TextArea.TextEntering += TextArea_TextEntering;
        TextArea.TextEntered += TextArea_TextEntered;

        TextChanged += async (_, __) =>
        {
            if (_isUpdating)
                return;

            try
            {
                _isUpdating = true;
                var text = base.Text ?? string.Empty;
                if (text.Length >= _prefixLength + _suffixLength)
                {
                    var body = text.Substring(_prefixLength, text.Length - _prefixLength - _suffixLength);
                    SetCurrentValue(BodyProperty, body);
                    if (DataContext is EntityScript script)
                    {
                        script.Body = body;
                        await UpdateRoslynAsync(script);
                    }
                }
            }
            finally
            {
                _isUpdating = false;
            }
        };
    }

    private static void OnBodyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var editor = (ScriptEditor)d;
        editor.UpdateDocument();
    }

    private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var editor = (ScriptEditor)d;
        editor.UpdateDocument();
    }

    private void UpdateDocument()
    {
        if (_isUpdating)
            return;

        try
        {
            _isUpdating = true;

            var nl = Environment.NewLine;
            var header = Header ?? string.Empty;
            var body = Body ?? string.Empty;
            var prefix = header + nl + "{" + nl;
            var suffix = nl + "}";

            _prefixLength = prefix.Length;
            _suffixLength = suffix.Length;

            base.Text = prefix + body + suffix;
            TextArea.Caret.Offset = _prefixLength;
            TextArea.ReadOnlySectionProvider = new HeaderFooterReadOnlySectionProvider(this);
            TextArea.TextView.InvalidateLayer(KnownLayer.Background);

            if (DataContext is EntityScript script)
            {
                script.Body = body;
                _ = UpdateRoslynAsync(script);
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private async Task UpdateRoslynAsync(EntityScript script)
    {
        if (_document == null)
            _document = _workspace.CreateDocument(script.Name, script);
        else
            _document = _workspace.UpdateDocument(_document, script);

        await UpdateDiagnosticsAsync();
    }

    private async Task UpdateDiagnosticsAsync()
    {
        if (_document == null)
            return;

        var diagnostics = await _workspace.GetDiagnosticsAsync(_document).ConfigureAwait(true);
        _markerService.Clear();
        foreach (var d in diagnostics)
        {
            if (d.Location.IsInSource)
            {
                var span = d.Location.SourceSpan;
                _markerService.Mark(span.Start, span.Length);
            }
        }
        TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
    }

    private async void TextArea_TextEntered(object? sender, TextCompositionEventArgs e)
    {
        if (_document != null && e.Text == ".")
        {
            await ShowCompletionAsync();
        }
    }

    private void TextArea_TextEntering(object? sender, TextCompositionEventArgs e)
    {
        if (e.Text.Length > 0 && _completionWindow != null)
        {
            if (!char.IsLetterOrDigit(e.Text[0]))
            {
                _completionWindow.CompletionList.RequestInsertion(e);
            }
        }
    }

    private async Task ShowCompletionAsync()
    {
        if (_document == null)
            return;

        var service = CompletionService.GetService(_document);
        if (service == null)
            return;

        var results = await service.GetCompletionsAsync(_document, TextArea.Caret.Offset);
        if (results == null)
            return;

        _completionWindow = new CompletionWindow(TextArea);
        var data = _completionWindow.CompletionList.CompletionData;
        foreach (var item in results.Items)
        {
            var desc = await service.GetDescriptionAsync(_document, item);
            data.Add(new RoslynCompletionData(item, desc.ToString()));
        }

        _completionWindow.Closed += (_, __) => _completionWindow = null;
        _completionWindow.Show();
    }

    private class ReadOnlyBackgroundRenderer : IBackgroundRenderer
    {
        private readonly ScriptEditor _editor;
        private readonly Brush _backgroundBrush;

        public ReadOnlyBackgroundRenderer(ScriptEditor editor)
        {
            _editor = editor;
            _backgroundBrush = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
            _backgroundBrush.Freeze();
        }

        public KnownLayer Layer => KnownLayer.Background;

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (textView.Document == null)
                return;

            int docLength = textView.Document.TextLength;

            DrawSegment(drawingContext, textView, 0, _editor._prefixLength);
            DrawSegment(drawingContext, textView, docLength - _editor._suffixLength, _editor._suffixLength);
        }

        private void DrawSegment(DrawingContext drawingContext, TextView textView, int start, int length)
        {
            if (length <= 0)
                return;

            var segment = new TextSegment { StartOffset = start, Length = length };
            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment))
            {
                drawingContext.DrawRectangle(_backgroundBrush, null, rect);
            }
        }
    }

    private class HeaderFooterReadOnlySectionProvider : IReadOnlySectionProvider
    {
        private readonly ScriptEditor _editor;

        public HeaderFooterReadOnlySectionProvider(ScriptEditor editor)
        {
            _editor = editor;
        }

        public bool CanInsert(int offset)
        {
            return offset >= _editor._prefixLength &&
                   offset <= _editor.Document.TextLength - _editor._suffixLength;
        }

        public IEnumerable<ISegment> GetDeletableSegments(ISegment segment)
        {
            int start = Math.Max(segment.Offset, _editor._prefixLength);
            int end = Math.Min(segment.EndOffset, _editor.Document.TextLength - _editor._suffixLength);
            if (start < end)
                yield return new TextSegment { StartOffset = start, Length = end - start };
        }
    }
}
