using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kesmai.WorldForge.Editor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using RoslynPad.Roslyn;
using RoslynPad.Roslyn.Diagnostics;

namespace Kesmai.WorldForge.Roslyn;

public class CustomRoslynHost : RoslynHost
{
    private CustomRoslynWorkspace _workspace;

    private Solution _segmentSolution;
    
    public CustomRoslynWorkspace Workspace => _workspace;
    
    public CustomRoslynHost(IEnumerable<Assembly> additionalAssemblies, RoslynHostReferences references) : base(additionalAssemblies, references)
    {
        _workspace = new CustomRoslynWorkspace(HostServices, WorkspaceKind.Host, this);
        _workspace.Services.GetRequiredService<IDiagnosticsUpdater>()
            .DisabledDiagnostics = DisabledDiagnostics;
    }

    public override RoslynWorkspace CreateWorkspace()
    {
        return _workspace;
    }
    
    protected override Project CreateProject(Solution solution, DocumentCreationArgs args, CompilationOptions compilationOptions, Project? previousProject = null)
    {
        var projectName = "Editor";
        var projectId = ProjectId.CreateNewId(projectName);

        var parseOptions = ParseOptions.WithKind(args.SourceCodeKind);
        
        var projectReferences = solution.ProjectIds.Select(p => new ProjectReference(p));
        
        _segmentSolution = solution.AddProject(ProjectInfo.Create(
                projectId, VersionStamp.Create(), projectName, projectName,
                LanguageNames.CSharp,
                filePath: args.WorkingDirectory,
                isSubmission: false,
                parseOptions: parseOptions,
                compilationOptions: compilationOptions,
                metadataReferences: previousProject != null ? [] : DefaultReferences,
                projectReferences: previousProject != null ? [new ProjectReference(previousProject.Id)] : projectReferences));
        
        var project = _segmentSolution.GetProject(projectId)!;
        
        if (GetUsings(project) is { Length: > 0 } usings)
            project = project.AddDocument("Usings.g.cs", usings).Project;

        return project;

        static string GetUsings(Project project)
        {
            if (project.CompilationOptions is CSharpCompilationOptions options)
                return String.Join(" ", options.Usings.Select(i => $"global using {i};"));

            return String.Empty;
        }
    }
    
    public void CreateSegmentProject(Segment segment)
    {
        var workspace = _workspace;
        var solution = workspace.CurrentSolution;
		
        var project = solution.AddProject($"Segment", $"Kesmai.Server.Segments.{segment.Name}", LanguageNames.CSharp)
            /* C# minimum to support global usings. */
            .WithParseOptions(new CSharpParseOptions(LanguageVersion.CSharp10))
            /* Minimum references to prevent overloading */
            .AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddMetadataReference(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));

        solution = project.Solution;

        var directory = new DirectoryInfo(@"C:\Example\Source");
		
        foreach(var file in directory.GetFiles("*.cs", SearchOption.AllDirectories))
        {
            solution = solution.AddDocument(DocumentId.CreateNewId(project.Id), file.Name, 
                SourceText.From(File.ReadAllText(file.FullName)));
        }
		
        workspace.TryApplyChanges(solution);
    }
}