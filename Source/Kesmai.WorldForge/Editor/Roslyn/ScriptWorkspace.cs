using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;

namespace Kesmai.WorldForge.Roslyn;

public class ScriptWorkspace
{
    private readonly AdhocWorkspace _workspace;
    private readonly Project _project;

    public ScriptWorkspace()
    {
        var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
        _workspace = new AdhocWorkspace(host);

        var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "ScriptProject", "ScriptProject", LanguageNames.CSharp)
            .WithMetadataReferences(new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
            });

        _project = _workspace.AddProject(projectInfo);

        const string hostCode = @"public static class ScriptHost
{
    public static void Greet(string name) { }
}";

        _workspace.AddDocument(_project.Id, "ScriptHost.cs", SourceText.From(hostCode));
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

