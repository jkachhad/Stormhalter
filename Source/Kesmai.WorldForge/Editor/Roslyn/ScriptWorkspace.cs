using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Kesmai.WorldForge.Roslyn;

public class ScriptWorkspace
{
    private readonly AdhocWorkspace _workspace;
    private readonly Project _project;

    public ScriptWorkspace()
    {
        _workspace = new AdhocWorkspace();

        var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "ScriptProject", "ScriptProject", LanguageNames.CSharp)
            .WithMetadataReferences(new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
            });

        _project = _workspace.AddProject(projectInfo);
    }

    public Document CreateDocument(string name, EntityScript script)
    {
        var text = SourceText.From(script.ToDocumentText());
        return _workspace.AddDocument(_project.Id, name, text);
    }

    public Document UpdateDocument(Document document, EntityScript script)
    {
        var text = SourceText.From(script.ToDocumentText());
        _workspace.TryApplyChanges(document.WithText(text).Project.Solution);
        return _workspace.CurrentSolution.GetDocument(document.Id)!;
    }

    public async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(Document document)
    {
        var model = await document.GetSemanticModelAsync().ConfigureAwait(false);
        return model.GetDiagnostics();
    }
}

