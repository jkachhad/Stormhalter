using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.Messaging;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using Kesmai.WorldForge.Editor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using SourceText = Microsoft.CodeAnalysis.Text.SourceText;
using TextDocument = ICSharpCode.AvalonEdit.Document.TextDocument;

namespace Kesmai.WorldForge.Roslyn;

/// <summary>
/// A <see cref="TextEditor"/> with distinct signature, header, body and footer sections.
/// The signature, header and footer are read-only; only the body may be edited by the user.
/// </summary>
public class RoslynCodeEditor : TextEditor
{
    private static readonly string NewLine = Environment.NewLine;
    
    /// <summary>Text shown at the start of the document before the header. Read-only.</summary>
    public string Signature
    {
        get => (string)GetValue(SignatureProperty);
        set => SetValue(SignatureProperty, value);
    }

    public static readonly DependencyProperty SignatureProperty =
        DependencyProperty.Register(nameof(Signature), typeof(string), typeof(RoslynCodeEditor),
            new PropertyMetadata(string.Empty, OnSignatureChanged));

    private static void OnSignatureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RoslynCodeEditor editor)
            return;
        
        editor._signature = e.NewValue as string ?? string.Empty;
        
        if (!editor._updatingDocument)
            editor.UpdateDocument();
    }

    /// <summary>Text shown after the signature. Read-only.</summary>
    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(RoslynCodeEditor),
            new PropertyMetadata(string.Empty, OnHeaderChanged));

    private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RoslynCodeEditor editor)
            return;
        
        editor._header = e.NewValue as string ?? string.Empty;
        
        if (!editor._updatingDocument)
            editor.UpdateDocument();
    }

    /// <summary>Editable body text.</summary>
    public string Body
    {
        get => (string)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    public static readonly DependencyProperty BodyProperty =
        DependencyProperty.Register(nameof(Body), typeof(string), typeof(RoslynCodeEditor),
            new PropertyMetadata(string.Empty, OnBodyChanged));

    private static void OnBodyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RoslynCodeEditor editor)
            return;
        
        if (editor._updatingDocument)
            return;

        editor._body = e.NewValue as string ?? string.Empty;
        editor.UpdateDocument();
    }

    /// <summary>Text shown at the end of the document. Read-only.</summary>
    public string Footer
    {
        get => (string)GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(nameof(Footer), typeof(string), typeof(RoslynCodeEditor),
            new PropertyMetadata(string.Empty, OnFooterChanged));

    private static void OnFooterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RoslynCodeEditor editor)
            return;
        
        editor._footer = e.NewValue as string ?? string.Empty;
        
        if (!editor._updatingDocument)
            editor.UpdateDocument();
    }
    
    private string _signature = string.Empty;
    private string _header = string.Empty;
    private string _body = string.Empty;
    private string _footer = string.Empty;

    private bool _updatingDocument;

    private readonly AdhocWorkspace _workspace;
    private readonly DocumentId _documentId;

    private RoslynCompletionWindow? _completionWindow;
    
    internal AdhocWorkspace Workspace => _workspace;
    internal DocumentId DocumentId => _documentId;

    public TextSegment SignatureSegment { get; } = new();
    public TextSegment HeaderSegment { get; } = new();
    public TextSegment BodySegment { get; } = new();
    public TextSegment FooterSegment { get; } = new();

    public RoslynCodeEditor()
    {
        Document = new TextDocument();

        var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
        var segment = presenter.Segment; 
            
        if (segment is null)
            throw new InvalidOperationException("No active segment.");
        
        _workspace = segment.Workspace;
        
        /* Create an editor specific project. */
        var project = _workspace.AddProject($"IntellisenseProject{Guid.NewGuid()}", LanguageNames.CSharp)
            /* C# minimum to support global usings. */
            .WithParseOptions(new CSharpParseOptions(LanguageVersion.CSharp10))
            /* Minimum references to prevent overloading */
            .AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddMetadataReference(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location))
            /* Add the in-memory segment project. */
            .AddProjectReference(new ProjectReference(segment.ProjectId));
        
        /* Add the scripting reference. */
        var scriptingData = Core.ScriptingData;
        
        if (scriptingData != null)
            project = project.AddMetadataReference(MetadataReference.CreateFromImage(scriptingData));
        
        _workspace.TryApplyChanges(project.Solution);
        
        /* virtual document to glue external namespaces and types. */
        var globalDocument = _workspace.AddDocument(project.Id, "GlobalUsings.cs", 
            SourceText.From($"global using static Kesmai.Server.Segments.{segment.Name};"));
        /* This editor's document. */
        var roslynDocument = _workspace.AddDocument(project.Id, "Script.cs", 
            SourceText.From(string.Empty));
       
        _documentId = roslynDocument.Id;

        UpdateDocument();

        Background = Brushes.White;
        
        TextArea.TextView.LineTransformers.Add(new BodyColorizingTransformer(this));
        TextArea.TextView.LineTransformers.Add(new RoslynColorizingTransformer(this));
        
        TextArea.ReadOnlySectionProvider = new ReadOnlySectionsProvider(this);
        
        Document.Changed += DocumentOnChanged;
        
        TextArea.TextEntering += TextAreaOnTextEntering;
        TextArea.TextEntered += TextAreaOnTextEntered;
        TextArea.PreviewKeyDown += TextAreaOnPreviewKeyDown;
    }

    private void UpdateDocument()
    {
        _updatingDocument = true;

        Document.Text = string.Join(NewLine, _signature, _header, _body, _footer);

        SignatureSegment.StartOffset = 0;
        SignatureSegment.Length = _signature.Length;

        HeaderSegment.StartOffset = SignatureSegment.EndOffset + NewLine.Length;
        HeaderSegment.Length = _header.Length;

        BodySegment.StartOffset = HeaderSegment.EndOffset + NewLine.Length;
        BodySegment.Length = _body.Length;

        FooterSegment.StartOffset = BodySegment.EndOffset + NewLine.Length;
        FooterSegment.Length = _footer.Length;

        _updatingDocument = false;

        UpdateWorkspaceDocument();
    }

    private void DocumentOnChanged(object? sender, DocumentChangeEventArgs e)
    {
        if (_updatingDocument)
            return;

        _updatingDocument = true;

        BodySegment.Length += e.InsertionLength - e.RemovalLength;
        _body = Document.GetText(BodySegment);
        SetCurrentValue(BodyProperty, _body);
        FooterSegment.StartOffset = BodySegment.EndOffset + NewLine.Length;

        _updatingDocument = false;

        UpdateWorkspaceDocument();
    }

    private void UpdateWorkspaceDocument()
    {
        var document = _workspace.CurrentSolution.GetDocument(_documentId);
        if (document != null)
        {
            var newDoc = document.WithText(SourceText.From(Document.Text));
            _workspace.TryApplyChanges(newDoc.Project.Solution);
        }
    }
    
    private async Task ShowCompletionAsync()
    {
        if (_completionWindow != null)
            _completionWindow.Close();

        var window = _completionWindow = new RoslynCompletionWindow(this);
        
        await window.InitializeAsync();

        if (window.CompletionList.CompletionData.Count > 0)
        {
            window.Closed += (_, _) => _completionWindow = null;
            window.Show();
        }
    }

    private async void TextAreaOnTextEntered(object? sender, TextCompositionEventArgs e)
    {
        if (e.Text == ".")
        {
            await ShowCompletionAsync();
        }
    }

    private void TextAreaOnTextEntering(object? sender, TextCompositionEventArgs e)
    {
        if (e.Text.Length > 0 && _completionWindow != null)
        {
            if (!char.IsLetterOrDigit(e.Text[0]))
            {
                _completionWindow.CompletionList.RequestInsertion(e);
            }
        }
    }

    private async void TextAreaOnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            await ShowCompletionAsync();
            e.Handled = true;
        }
    }
    
    private class RoslynColorizingTransformer : DocumentColorizingTransformer
    {
        private readonly RoslynCodeEditor _editor;
        private IList<ClassifiedSpan> _spans = new List<ClassifiedSpan>();

        public RoslynColorizingTransformer(RoslynCodeEditor editor)
        {
            _editor = editor;
            _editor.Document.Changed += (_, __) => UpdateHighlighting();
            UpdateHighlighting();
        }

        private async void UpdateHighlighting()
        {
            _editor.UpdateWorkspaceDocument();
            
            var document = _editor.Workspace.CurrentSolution.GetDocument(_editor.DocumentId);
            
            if (document == null)
            {
                _spans = new List<ClassifiedSpan>();
                return;
            }

            var text = await document.GetTextAsync();
            var span = new TextSpan(0, text.Length);
            _spans = (await Classifier.GetClassifiedSpansAsync(document, span)).ToList();
            _editor.TextArea.TextView.Redraw();
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (_spans.Count == 0)
                return;

            int lineStart = line.Offset;
            int lineEnd = line.EndOffset;
            foreach (var classified in _spans)
            {
                var span = classified.TextSpan;
                if (span.End <= lineStart || span.Start >= lineEnd)
                    continue;

                var brush = GetBrush(classified);
                if (brush == null)
                    continue;

                int start = Math.Max(span.Start, lineStart);
                int end = Math.Min(span.End, lineEnd);
                ChangeLinePart(start, end, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(brush);
                });
            }
        }

        private static Brush? GetBrush(ClassifiedSpan span) => span.ClassificationType switch
        {
            ClassificationTypeNames.ClassName or
            ClassificationTypeNames.StructName or
            ClassificationTypeNames.InterfaceName or
            ClassificationTypeNames.EnumName or
            ClassificationTypeNames.DelegateName or
            ClassificationTypeNames.ModuleName or
            ClassificationTypeNames.RecordClassName or
            ClassificationTypeNames.RecordStructName or
            ClassificationTypeNames.TypeParameterName => Brushes.Teal,

            ClassificationTypeNames.MethodName or
            ClassificationTypeNames.ExtensionMethodName => Brushes.MediumPurple,

            ClassificationTypeNames.PropertyName or
            ClassificationTypeNames.EventName or
            ClassificationTypeNames.FieldName or
            ClassificationTypeNames.EnumMemberName or
            ClassificationTypeNames.LocalName or
            ClassificationTypeNames.ParameterName => Brushes.DarkCyan,

            ClassificationTypeNames.Keyword => Brushes.Blue,
            ClassificationTypeNames.StringLiteral => Brushes.Brown,
            ClassificationTypeNames.NumericLiteral => Brushes.Magenta,
            ClassificationTypeNames.Comment => Brushes.Green,
            _ => null
        };
    }
    
    private class BodyColorizingTransformer : DocumentColorizingTransformer
    {
        private readonly RoslynCodeEditor _editor;

        public BodyColorizingTransformer(RoslynCodeEditor editor)
        {
            _editor = editor;
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            var body = _editor.BodySegment;
            if (body.Length == 0)
                return;

            int lineStart = line.Offset;
            int lineEnd = line.EndOffset;
            
            if (lineEnd <= body.StartOffset || lineStart >= body.EndOffset)
                return;

            int start = Math.Max(lineStart, body.StartOffset);
            int end = Math.Min(lineEnd, body.EndOffset);

            ChangeLinePart(start, end, element =>
            {
                element.TextRunProperties.SetBackgroundBrush(Brushes.White);
            });
        }
    }

    private class ReadOnlySectionsProvider : IReadOnlySectionProvider
    {
        private readonly RoslynCodeEditor _editor;

        public ReadOnlySectionsProvider(RoslynCodeEditor editor)
        {
            _editor = editor;
        }

        public bool CanInsert(int offset)
        {
            var body = _editor.BodySegment;
            return offset >= body.StartOffset && offset <= body.EndOffset;
        }

        public IEnumerable<ISegment> GetDeletableSegments(ISegment segment)
        {
            var body = _editor.BodySegment;
            
            int start = Math.Max(segment.Offset, body.StartOffset);
            int end = Math.Min(segment.EndOffset, body.EndOffset);
            
            if (start < end)
                yield return new TextSegment { StartOffset = start, Length = end - start };
        }
    }
}
