using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Kesmai.WorldForge.Editor.Scripting;

public class ScriptWorkspace
{
    private readonly AdhocWorkspace _workspace;
    private readonly ProjectId _projectId;
    private readonly Dictionary<string, ScriptDocument> _documents = new();

    public static ScriptWorkspace Instance { get; } = new ScriptWorkspace();

    private ScriptWorkspace()
    {
        _workspace = new AdhocWorkspace();
        var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(),
            "Scripts", "Scripts", LanguageNames.CSharp);
        _workspace.AddProject(projectInfo);
        _projectId = projectInfo.Id;
    }

    public ScriptDocument AddOrUpdate(string name, string text, string? filePath = null)
    {
        if (_documents.TryGetValue(name, out var existing))
        {
            _workspace.TryApplyChanges(_workspace.CurrentSolution.WithDocumentText(existing.DocumentId, SourceText.From(text)));
            existing.UpdateText(text);
            return existing;
        }

        var documentId = DocumentId.CreateNewId(_projectId);
        var loader = TextLoader.From(TextAndVersion.Create(SourceText.From(text), VersionStamp.Create(), filePath));
        var documentInfo = DocumentInfo.Create(documentId, name, filePath: filePath, loader: loader);
        _workspace.AddDocument(documentInfo);

        var doc = new ScriptDocument(documentId, name, text, filePath);
        _documents[name] = doc;
        return doc;
    }

    public Document? GetDocument(string name)
    {
        return _documents.TryGetValue(name, out var doc) ? _workspace.CurrentSolution.GetDocument(doc.DocumentId) : null;
    }
}
