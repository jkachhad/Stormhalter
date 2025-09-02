using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host;
using RoslynPad.Roslyn;
using RoslynPad.Roslyn.Diagnostics;

namespace Kesmai.WorldForge.Roslyn;

public class CustomRoslynWorkspace : RoslynWorkspace
{
    public CustomRoslynWorkspace(HostServices hostServices, string workspaceKind, RoslynHost roslynHost) : base(hostServices, workspaceKind, roslynHost)
    {
    }

    public override bool CanApplyChange(ApplyChangesKind feature)
    {
        return feature switch
        {
            ApplyChangesKind.RemoveProject or ApplyChangesKind.AddProject => true,
            ApplyChangesKind.AddSolutionAnalyzerReference or ApplyChangesKind.RemoveSolutionAnalyzerReference => true,
            
            _ => base.CanApplyChange(feature),
        };
    }
}

public class CustomRoslynHost : RoslynHost
{
    private CustomRoslynWorkspace _workspace;
    
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
        
        solution = solution.AddProject(ProjectInfo.Create(
                projectId, VersionStamp.Create(), projectName, projectName,
                LanguageNames.CSharp,
                filePath: args.WorkingDirectory,
                isSubmission: false,
                parseOptions: parseOptions,
                compilationOptions: compilationOptions,
                metadataReferences: previousProject != null ? [] : DefaultReferences,
                projectReferences: previousProject != null ? [new ProjectReference(previousProject.Id)] : projectReferences));
        
        var project = solution.GetProject(projectId)!;
        
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
}