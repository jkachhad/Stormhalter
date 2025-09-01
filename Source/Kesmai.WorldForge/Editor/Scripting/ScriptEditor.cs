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
	private bool _initialized;
	private DispatcherTimer _updateTimer;
		
	public TextSegmentCollection<TextSegment> ReadOnlySegments { get; set; }
		
	private static readonly DependencyProperty ScriptProperty =
		DependencyProperty.Register("Script", typeof(Script), typeof(ScriptEditor), 
			new PropertyMetadata(OnScriptChange));

	public static void OnScriptChange(object o, DependencyPropertyChangedEventArgs args)
	{
		if (o is not ScriptEditor editor || args.NewValue is not Script newScript) 
			return;

		var template = newScript.Template;

		if (template != null)
			template.Apply(editor, newScript);
	}

	public Script Script
	{
		get => (Script)GetValue(ScriptProperty);
		set => SetValue(ScriptProperty, value);
	}

	public ScriptEditor()
	{
		ReadOnlySegments = new TextSegmentCollection<TextSegment>(Document);
			
		TextArea.ReadOnlySectionProvider = new TextSegmentReadOnlySectionProvider<TextSegment>(ReadOnlySegments);
		TextArea.TextView.BackgroundRenderers.Add(new ScriptBackgroundRenderer(this));

		HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
			
		FontFamily = new FontFamily("Consolas");


        Loaded += OnLoaded;
	}

	protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		if (IsModified)
			UpdateText();
	}

    private async Task OnLoadedAsync(object sender, RoutedEventArgs e)
    {
        if (_initialized)
            return;

        var services = (ServiceContainer)ServiceLocator.Current;
        var presenter = services.GetInstance<ApplicationPresenter>();
        var segment = presenter.Segment;

        await InitializeAsync(segment.Roslyn, new ClassificationHighlightColors(),
            Directory.GetCurrentDirectory(), string.Empty, SourceCodeKind.Regular);

        // Delay folding until TextArea is fully initialized
        await Dispatcher.BeginInvoke(new Action(() =>
        {
            InitializeFolding();
        }), DispatcherPriority.Loaded);
        InitializeFolding();
        _initialized = true;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("VSTHRD", "VSTHRD100:Avoid async void methods")]
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await OnLoadedAsync(sender, e);
    }

    protected override void OnTextChanged(EventArgs e)
	{
		if (!_initialized)
			return;
			
		IsModified = true;
			
		if (_updateTimer != null && _updateTimer.IsEnabled)
			_updateTimer.Stop();

		_updateTimer = new DispatcherTimer()
		{
			Interval = TimeSpan.FromSeconds(1.0),
		};
		_updateTimer.Tick += (o, args) => UpdateText();
		_updateTimer.Start();
	}

	private void UpdateText()
	{
		if (!_initialized)
			return;
			
		if (_updateTimer != null && _updateTimer.IsEnabled)
			_updateTimer.Stop();
			
		var script = Script;
			
		if (script != null)
			script.Parse(this);

        IsModified = false;
	}

	public void Insert(string text, bool readOnly = false)
	{
		var start = Document.TextLength;
		Document.Insert(start, text);
		var end = Document.TextLength;

		if (readOnly)
		{
			ReadOnlySegments.Add(new TextSegment()
			{
				StartOffset = start, EndOffset = end,
			});
		}
	}

	public IEnumerable<TextSegment> GetNonReadOnlySegments()
	{
		var firstSegment = ReadOnlySegments.FirstSegment;
		var lastSegment = ReadOnlySegments.LastSegment;
			
		/* Get the text-segment prior to first segment, even empty text. */
		if (firstSegment != null)
		{
			yield return new TextSegment()
			{
				StartOffset = 0,
				EndOffset = firstSegment.StartOffset
			};
		}

		/* Get segments in between read-only segments. */
		var currentSegment = firstSegment;

		while (currentSegment != lastSegment && currentSegment != null)
		{
			var nextSegment = ReadOnlySegments.GetNextSegment(currentSegment);
				
			var startOffset = currentSegment.EndOffset;
			var endOffset = nextSegment.StartOffset;

			yield return new TextSegment()
			{
				StartOffset = startOffset,
				EndOffset = endOffset,
			};

			currentSegment = nextSegment;
		}
			
		/* Get the text-segment after the last segment, even empty text. */
		if (lastSegment != null)
		{
			yield return new TextSegment()
			{
				StartOffset = lastSegment.EndOffset,
				EndOffset = Document.TextLength
			};
		}
    }

    private bool _foldingInitialized;

    public void InitializeFolding()
    {
        if (_foldingInitialized)
            return;
        HookFoldingMarginClick();
        _foldingInitialized = true;
    }

    public void ExpandAllFolds()
    {
        if (FoldingManager != null)
        {
            foreach (var fold in FoldingManager.AllFoldings)
                fold.IsFolded = false;
        }
        this.TextArea.TextView.InvalidateVisual();
    }

    public void CollapseAllFolds()
    {
		if (FoldingManager != null)
        {
            bool isFirst = true;
            foreach (var fold in FoldingManager.AllFoldings)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue; // Skip the first fold
                }

                // Expand blocks inside braces (small inner folds)
                if (fold.Title != null && fold.Title.StartsWith("{"))
                {
                    fold.IsFolded = false; // Expand inner block
                }
                else
                {
                    fold.IsFolded = true; // Collapse outer block
                }
            }
        }
        this.TextArea.TextView.InvalidateVisual();
    }

    private void HookFoldingMarginClick()
    {
        var foldingMargin = TextArea.LeftMargins
            .OfType<FoldingMargin>()
            .FirstOrDefault();

        if (foldingMargin != null)
        {
            foldingMargin.PreviewMouseLeftButtonDown += FoldingMargin_MouseLeftButtonDown;
        }
    }

    private void FoldingMargin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var posInMargin = e.GetPosition(sender as IInputElement);
        double correctedY = posInMargin.Y + TextArea.TextView.VerticalOffset;

        var visualLine = TextArea.TextView.GetVisualLineFromVisualTop(correctedY);
        if (visualLine == null)
            return;

        int offset = visualLine.FirstDocumentLine.Offset;

        if (offset < 10)
			return;

        var fold = FoldingManager.AllFoldings.FirstOrDefault(f => f.StartOffset >= offset && offset <= f.EndOffset);

        if (fold != null && fold.IsFolded)
        {
            ExpandDirectChildren(fold);
            TextArea.TextView.InvalidateVisual();
            e.Handled = true;
        }
    }

    void ExpandDirectChildren(FoldingSection parent)
    {
        parent.IsFolded = false;

		var fold = FoldingManager.AllFoldings.Where(f => f.StartOffset > parent.StartOffset && f.StartOffset <= parent.EndOffset);

        foreach (var child in fold)
        {
            child.IsFolded = false;
            this.TextArea.TextView.InvalidateVisual();
        }
    }
}