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
using Microsoft.CodeAnalysis.Text;
using RoslynPad.Roslyn;

namespace Kesmai.WorldForge.Roslyn;

#nullable enable
public class CustomRoslynHost : RoslynHost, IDisposable
{
    private CustomResolver _resolver;
    private Solution _solution;
    private bool _disposed = false;

    public CustomRoslynHost(Segment segment) : base(
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
        _resolver = new CustomResolver(segment);
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
                .WithScriptClassName(name)
                .WithSourceReferenceResolver(_resolver);

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
            .WithScriptClassName(name)
            .WithSourceReferenceResolver(_resolver);

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
                _resolver = null; // Nullify to release reference
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

public class CustomResolver : SourceReferenceResolver, IDisposable
{
    private SourceText _cache;

    public CustomResolver(Segment segment)
    {
        var builder = new StringBuilder();

        if (segment.Internal != null)
            builder.AppendLine(segment.Internal.ToString());

        foreach (var lootTemplate in segment.Treasures.Select(t => t.Name))
            builder.AppendLine($"Func<MobileEntity, Container, ItemEntity> {lootTemplate};");

        foreach (var entities in segment.Entities.Select(t => t.Name))
            builder.AppendLine($"Func<CreatureEntity> {entities};");

        _cache = SourceText.From(builder.ToString());
    }

    public override SourceText ReadText(string resolvedPath) => _cache;

    public override string NormalizePath(string path, string baseFilePath) => path;
    public override string ResolveReference(string path, string baseFilePath) => path;
    public override Stream OpenRead(string resolvedPath) => null;

    public void Dispose()
    {
        _cache = null; // Clear the cache to release memory
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != this.GetType())
            return false;

        if (obj is CustomResolver other && other._cache != _cache)
            return false;

        return true;
    }

    public override int GetHashCode()
    {
        return _cache.GetHashCode();
    }
}
#nullable disable