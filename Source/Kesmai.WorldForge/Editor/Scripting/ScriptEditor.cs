using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CommonServiceLocator;
using DigitalRune.ServiceLocation;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
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

	private async void OnLoaded(object sender, RoutedEventArgs e)
	{
		var services = (ServiceContainer)ServiceLocator.Current;
		var applicationPresenter = services.GetInstance<ApplicationPresenter>();

		await InitializeAsync(applicationPresenter.RoslynHost, new ClassificationHighlightColors(),
			Directory.GetCurrentDirectory(), String.Empty, SourceCodeKind.Script);
		
		_initialized = true;
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
}