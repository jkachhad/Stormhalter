using Microsoft.CodeAnalysis;

namespace Kesmai.WorldForge.Editor.Scripting;

public class ScriptDocument
{
    public string Name { get; }
    public string? FilePath { get; }
    public DocumentId DocumentId { get; }
    public string Text { get; private set; }

    internal ScriptDocument(DocumentId id, string name, string text, string? filePath)
    {
        DocumentId = id;
        Name = name;
        Text = text;
        FilePath = filePath;
    }

    internal void UpdateText(string text)
    {
        Text = text;
    }
}
