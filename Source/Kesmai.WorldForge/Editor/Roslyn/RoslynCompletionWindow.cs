using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;

namespace Kesmai.WorldForge.Roslyn;

public class RoslynCompletionWindow : CompletionWindow
{
	public RoslynCompletionWindow(TextArea textArea) : base(textArea)
	{
	}
}