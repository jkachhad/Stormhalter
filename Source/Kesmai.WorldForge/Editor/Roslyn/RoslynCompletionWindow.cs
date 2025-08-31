using System;
using System.Threading.Tasks;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Microsoft.CodeAnalysis.Completion;

namespace Kesmai.WorldForge.Roslyn;

public class RoslynCompletionWindow : CompletionWindow
{
	private readonly RoslynCodeEditor _editor;

	public RoslynCompletionWindow(RoslynCodeEditor editor) : base(editor.TextArea)
	{
		_editor = editor;
	}

	public async Task InitializeAsync()
	{
		var document = _editor.Workspace.CurrentSolution.GetDocument(_editor.DocumentId);
		if (document == null)
			return;

		var service = CompletionService.GetService(document);
		if (service == null)
			return;

		var results = await service.GetCompletionsAsync(document, TextArea.Caret.Offset);
		if (results == null)
			return;

		var data = CompletionList.CompletionData;
		foreach (var item in results.Items)
		{
			data.Add(new RoslynCompletionData(item.DisplayText));
		}
	}

	private class RoslynCompletionData : ICompletionData
	{
		public RoslynCompletionData(string text)
		{
			Text = text;
		}

		public ImageSource? Image => null;
		public string Text { get; }
		public object Content => Text;
		public object Description => Text;
		public double Priority => 0;

		public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
		{
			textArea.Document.Replace(completionSegment, Text);
		}
	}
}