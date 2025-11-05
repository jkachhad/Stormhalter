using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CommonServiceLocator;
using DigitalRune.ServiceLocation;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using Kesmai.WorldForge.Editor;
using Microsoft.CodeAnalysis;
using RoslynPad.Editor;

namespace Kesmai.WorldForge.Scripting;

public class ScriptEditor : RoslynCodeEditor
{
	private static readonly string NewLine = Environment.NewLine;
    
    /// <summary>Text shown at the start of the document before the header. Read-only.</summary>
    public string Signature
    {
        get => (string)GetValue(SignatureProperty);
        set => SetValue(SignatureProperty, value);
    }

    public static readonly DependencyProperty SignatureProperty =
        DependencyProperty.Register(nameof(Signature), typeof(string), typeof(ScriptEditor),
            new PropertyMetadata(string.Empty, OnSignatureChanged));

    private static void OnSignatureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScriptEditor editor)
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
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(ScriptEditor),
            new PropertyMetadata(string.Empty, OnHeaderChanged));

    private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScriptEditor editor)
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
        DependencyProperty.Register(nameof(Body), typeof(string), typeof(ScriptEditor),
            new PropertyMetadata(string.Empty, OnBodyChanged));

    private static void OnBodyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScriptEditor editor)
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
        DependencyProperty.Register(nameof(Footer), typeof(string), typeof(ScriptEditor),
            new PropertyMetadata(string.Empty, OnFooterChanged));

    private static void OnFooterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScriptEditor editor)
            return;
        
        editor._footer = e.NewValue as string ?? string.Empty;
        
        if (!editor._updatingDocument)
            editor.UpdateDocument();
    }
    
    private string _signature = string.Empty;
    private string _header = string.Empty;
    private string _body = string.Empty;
    private string _footer = string.Empty;
    
    private bool _initialized;
    private bool _updatingDocument;
    
    public TextSegment SignatureSegment { get; } = new();
    public TextSegment HeaderSegment { get; } = new();
    public TextSegment BodySegment { get; } = new();
    public TextSegment FooterSegment { get; } = new();

	public ScriptEditor()
	{
        Background = Brushes.White;

        TextArea.ReadOnlySectionProvider = new ReadOnlySectionsProvider(this);
        
        Loaded += OnLoaded;
	}
	
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
            return;
        
        Loaded -= OnLoaded;
        
        var workspace = ServiceLocator.Current.GetInstance<SegmentWorkspace>();

        await InitializeAsync(workspace.Host, new ClassificationHighlightColors(),
            Directory.GetCurrentDirectory(), string.Empty, SourceCodeKind.Regular);
        
        UpdateDocument();
        
        Document.Changed += DocumentOnChanged;
        
        _initialized = true;
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
    }

    private void DocumentOnChanged(object? sender, DocumentChangeEventArgs e)
	{
		if (_updatingDocument)
			return;

		_updatingDocument = true; try
		{
			var bodySegmentDelta = e.InsertionLength - e.RemovalLength;
			BodySegment.Length = Math.Max(0, BodySegment.Length + bodySegmentDelta);

		try
		{
			var bodySegmentDelta = e.InsertionLength - e.RemovalLength;
			BodySegment.Length = Math.Max(0, BodySegment.Length + bodySegmentDelta);

			_body = Document.GetText(BodySegment);
			SetCurrentValue(BodyProperty, _body);

			FooterSegment.StartOffset = BodySegment.EndOffset + NewLine.Length;
		}
		finally
		{
			_updatingDocument = false;
		}
	}
    
    private class ReadOnlySectionsProvider : IReadOnlySectionProvider
    {
        private readonly ScriptEditor _editor;

        public ReadOnlySectionsProvider(ScriptEditor editor)
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