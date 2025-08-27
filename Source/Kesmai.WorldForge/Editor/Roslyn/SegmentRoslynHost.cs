using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Kesmai.WorldForge.Editor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoslynPad.Roslyn;

namespace Kesmai.WorldForge.Roslyn;

public class SegmentRoslynHost : RoslynHost, IDisposable
{
    private readonly Segment _segment;
    private Solution _solution;
    private bool _disposed;

    public SegmentRoslynHost(Segment segment) : base(
        additionalAssemblies: new[]
        {
            Assembly.Load("RoslynPad.Roslyn.Windows"),
            Assembly.Load("RoslynPad.Editor.Windows"),
        },
        RoslynHostReferences.NamespaceDefault.With(imports: CreateImports(segment)))
    {
        _segment = segment;
    }

    private static string[] CreateImports(Segment segment)
    {
        var imports = new List<string>
        {
            "WorldForge"
        };

        if (!string.IsNullOrEmpty(segment?.Name))
        {
            imports.Add($"Kesmai.Server.Segment.{segment.Name}");
            imports.Add($"static Kesmai.Server.Segment.{segment.Name}.Internal");
        }

        return imports.ToArray();
    }

    protected override Project CreateProject(Solution solution, DocumentCreationArgs args, CompilationOptions compilationOptions, Project? previousProject = null)
    {
        var name = "WorldForge";
        var id = ProjectId.CreateNewId(name);

        var baseSolution = _segment.Workspace?.Workspace?.CurrentSolution ?? solution;

        var existingProject = baseSolution.Projects.FirstOrDefault(p => p.Name == name);
        var sourceProject = _segment.Workspace?.Workspace?.CurrentSolution.Projects.FirstOrDefault();

        if (existingProject != null)
        {
            var updatedCompilationOptions = compilationOptions
                .WithScriptClassName(name);

            var project = existingProject
                .WithCompilationOptions(updatedCompilationOptions)
                .WithParseOptions(new CSharpParseOptions(LanguageVersion.CSharp10, DocumentationMode.None, SourceCodeKind.Script));

            if (sourceProject != null && existingProject.Id != sourceProject.Id && !existingProject.ProjectReferences.Any(r => r.ProjectId == sourceProject.Id))
            {
                project = project.AddProjectReference(new ProjectReference(sourceProject.Id));
            }

            return project;
        }

        var parseOptions = new CSharpParseOptions(
            kind: SourceCodeKind.Script,
            languageVersion: LanguageVersion.CSharp10,
            documentationMode: DocumentationMode.None
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
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();

        var dotNextDllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DotNext.dll");
        references.Add(MetadataReference.CreateFromFile(dotNextDllPath));

        var scriptingData = Core.ScriptingData;

        if (scriptingData != null)
            references.Add(MetadataReference.CreateFromImage(scriptingData));

        var projectReferences = new List<ProjectReference>();

        if (previousProject != null)
            projectReferences.Add(new ProjectReference(previousProject.Id));

        if (sourceProject != null)
            projectReferences.Add(new ProjectReference(sourceProject.Id));

        var projectInfo = ProjectInfo.Create(
            id, VersionStamp.Create(),
            name, name,
            LanguageNames.CSharp,
            isSubmission: true,
            parseOptions: parseOptions,
            compilationOptions: compilationOptions,
            metadataReferences: previousProject != null ? ImmutableArray<MetadataReference>.Empty : references,
            projectReferences: projectReferences.Count > 0 ? projectReferences : null
        );

        _solution = baseSolution.AddProject(projectInfo);

        return _solution.GetProject(id);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        if (_solution != null)
        {
            foreach (var projectId in _solution.ProjectIds)
            {
                _solution = _solution.RemoveProject(projectId);
            }

            _solution = null;
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
