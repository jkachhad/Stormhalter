using System;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Microsoft.CodeAnalysis.Completion;

namespace Kesmai.WorldForge.UI.Controls;

internal class RoslynCompletionData : ICompletionData
{
    private readonly CompletionItem _item;
    private readonly string _description;

    public RoslynCompletionData(CompletionItem item, string description)
    {
        _item = item;
        _description = description;
        Text = item.DisplayText;
    }

    public System.Windows.Media.ImageSource Image => null;

    public string Text { get; }

    public object Content => Text;

    public object Description => _description;

    public double Priority => 0;

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        textArea.Document.Replace(completionSegment, Text);
    }
}

