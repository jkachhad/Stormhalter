using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using DotNext;
using System.Runtime;
using System.Text;
using CommonServiceLocator;
using ICSharpCode.AvalonEdit.Document;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Text;
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
        var workspace = solution.Workspace;
        
        var name = args.Name ?? "New";
        var path = Path.Combine(args.WorkingDirectory, name);
        var id = ProjectId.CreateNewId(name);

        var parseOptions = ParseOptions.WithKind(args.SourceCodeKind);
        var isScript = args.SourceCodeKind == SourceCodeKind.Script;

        if (isScript)
        {
            compilationOptions = compilationOptions.WithScriptClassName(name);
        }

        var analyzerConfigDocuments = AnalyzerConfigFiles.Where(File.Exists).Select(file => DocumentInfo.Create(
            DocumentId.CreateNewId(id, debugName: file),
            name: file,
            loader: new FileTextLoader(file, defaultEncoding: null),
            filePath: file));

        var projectReferences = solution.ProjectIds.Select(p => new ProjectReference(p));
        
        solution = solution.AddProject(ProjectInfo.Create(
                id,
                VersionStamp.Create(),
                name,
                name,
                LanguageNames.CSharp,
                filePath: path,
                isSubmission: isScript,
                parseOptions: parseOptions,
                compilationOptions: compilationOptions,
                metadataReferences: previousProject != null ? [] : DefaultReferences,
                projectReferences: previousProject != null ? [new ProjectReference(previousProject.Id)] : projectReferences)
            .WithAnalyzerConfigDocuments(analyzerConfigDocuments));
        
        var project = solution.GetProject(id)!;
        
        if (!isScript && GetUsings(project) is { Length: > 0 } usings)
        {
            project = project.AddDocument("RoslynPadGeneratedUsings", usings).Project;
        }

        return project;

        static string GetUsings(Project project)
        {
            if (project.CompilationOptions is CSharpCompilationOptions options)
            {
                return string.Join(" ", options.Usings.Select(i => $"global using {i};"));
            }

            return string.Empty;
        }
    }
}

#nullable enable
public class CustomRoslynHostDeprecated : RoslynHost, IDisposable
{
    private Solution _solution;
    private bool _disposed = false;

    public CustomRoslynHostDeprecated(Segment segment) : base(
        additionalAssemblies: new[]
        {
            Assembly.Load("RoslynPad.Roslyn.Windows"),
            Assembly.Load("RoslynPad.Editor.Windows"),
        },
        RoslynHostReferences.NamespaceDefault.With(imports: new[]
        {
            "WorldForge",
        }))
    {
    }

    protected override Project CreateProject(Solution solution, DocumentCreationArgs args, CompilationOptions compilationOptions, Project? previousProject = null)
    {
        var name = "WorldForge";
        var id = ProjectId.CreateNewId(name);

        // Check if the project already exists in the solution
        var existingProject = solution.Projects.FirstOrDefault(p => p.Name == name);

        if (existingProject != null)
        {
            // Optionally update existing project's options, references, etc., if necessary
            var updatedCompilationOptions = compilationOptions
                .WithScriptClassName(name);

            var project = existingProject
                .WithCompilationOptions(updatedCompilationOptions)
                .WithParseOptions(new CSharpParseOptions(LanguageVersion.CSharp10, DocumentationMode.None, SourceCodeKind.Script));

            // Return the existing, possibly updated, project
            return project;
        }

        // If no existing project is found, create a new one
        var parseOptions = new CSharpParseOptions(
            kind: SourceCodeKind.Script,
            languageVersion: LanguageVersion.CSharp10,
            documentationMode: DocumentationMode.None // Skip documentation comments
        );

        compilationOptions = compilationOptions
            .WithScriptClassName(name);

        if (compilationOptions is CSharpCompilationOptions csharpCompilationOptions)
        {
            compilationOptions = csharpCompilationOptions
                .WithNullableContextOptions(NullableContextOptions.Disable);
        }

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a =>
            {
                var location = a.Location;
                return !string.IsNullOrEmpty(location) ? MetadataReference.CreateFromFile(location) : null;
            })
            .Where(reference => reference != null)
            .ToList();

        // Add DotNext.dll to the references
        var dotNextDllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DotNext.dll"); // Path to DotNext.dll in the assemblies folder
        references.Add(MetadataReference.CreateFromFile(dotNextDllPath));

        var scriptingData = Core.ScriptingData;

        if (scriptingData != null)
            references.Add(MetadataReference.CreateFromImage(scriptingData));

        var projectInfo = ProjectInfo.Create(
            id, VersionStamp.Create(),
            name, name,
            LanguageNames.CSharp,
            isSubmission: true,
            parseOptions: parseOptions,
            compilationOptions: compilationOptions,
            metadataReferences: previousProject != null ? ImmutableArray<MetadataReference>.Empty : references,
            projectReferences: previousProject != null ? new[] { new ProjectReference(previousProject.Id) } : null
        );

        _solution = solution.AddProject(projectInfo);

        return _solution.GetProject(id);
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Clean up managed resources
                CleanupSolution();
            }
            // Clean up unmanaged resources
            _disposed = true;
        }
    }

    private void CleanupSolution()
    {
        if (_solution != null)
        {
            foreach (var projectId in _solution.ProjectIds)
            {
                _solution = _solution.RemoveProject(projectId);
            }
            _solution = null;
        }

        // Optionally force garbage collection, if necessary
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
#nullable disable