using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using RoslynPad.Roslyn;

namespace Kesmai.WorldForge.Roslyn;

public class SegmentWorkspace : IDisposable
{
    public Workspace Workspace { get; }

    private readonly InMemorySource _internal;
    private readonly Dictionary<string, DocumentId> _documents = new(StringComparer.OrdinalIgnoreCase);
    private readonly FileSystemWatcher _watcher;

    public SegmentWorkspace(string rootPath, RoslynHost host)
    {
        var sourcePath = Path.Combine(rootPath, "Source");

        using var hostWorkspace = host.CreateWorkspace();
        Workspace = new AdhocWorkspace(hostWorkspace.Services.HostServices, "Custom");

        var projectId = ProjectId.CreateNewId();
        var solution = Workspace.CurrentSolution
            .AddProject(projectId, "InMemoryProject", "InMemoryProject", LanguageNames.CSharp);

        var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        var systemCore = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);

        solution = solution.AddMetadataReference(projectId, mscorlib);
        solution = solution.AddMetadataReference(projectId, systemCore);
        Workspace.TryApplyChanges(solution);

        foreach (var path in Directory.EnumerateFiles(sourcePath, "*.cs", SearchOption.AllDirectories))
            AddDocument(path);

        _watcher = new FileSystemWatcher(sourcePath, "*.cs")
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
        };

        _watcher.Changed += (_, e) => OnFileChanged(e.FullPath);
        _watcher.Created += (_, e) => OnFileCreated(e.FullPath);
        _watcher.Deleted += (_, e) => OnFileDeleted(e.FullPath);
        _watcher.Renamed += (_, e) => OnFileRenamed(e.OldFullPath, e.FullPath);
        _watcher.EnableRaisingEvents = true;

        _internal = new InMemorySource("Internal", string.Empty);
        AddDocument(_internal);
    }

    public void UpdateInternal(string text) => _internal.Text = text;

    private Document AddDocument(string filePath)
    {
        var project = Workspace.CurrentSolution.Projects.First();
        var text = File.ReadAllText(filePath);

        var documentId = DocumentId.CreateNewId(project.Id);
        var sourceText = SourceText.From(text);
        var solution = Workspace.CurrentSolution.AddDocument(documentId, Path.GetFileName(filePath), sourceText, filePath: filePath);
        Workspace.TryApplyChanges(solution);
        _documents[filePath] = documentId;

        return Workspace.CurrentSolution.GetDocument(documentId)!;
    }

    private Document AddDocument(InMemorySource source)
    {
        var project = Workspace.CurrentSolution.Projects.First();
        var documentId = DocumentId.CreateNewId(project.Id);
        var sourceText = SourceText.From(source.Text);
        var solution = Workspace.CurrentSolution.AddDocument(documentId, source.Name, sourceText);
        Workspace.TryApplyChanges(solution);

        source.TextChanged += (_, _) =>
        {
            var text = SourceText.From(source.Text);
            Workspace.TryApplyChanges(Workspace.CurrentSolution.WithDocumentText(documentId, text));
        };

        return Workspace.CurrentSolution.GetDocument(documentId)!;
    }

    private void OnFileChanged(string path)
    {
        if (!_documents.TryGetValue(path, out var id))
            return;

        try
        {
            var text = SourceText.From(File.ReadAllText(path));
            Workspace.TryApplyChanges(Workspace.CurrentSolution.WithDocumentText(id, text));
        }
        catch (IOException)
        {
        }
    }

    private void OnFileDeleted(string path)
    {
        if (_documents.TryGetValue(path, out var id))
        {
            Workspace.TryApplyChanges(Workspace.CurrentSolution.RemoveDocument(id));
            _documents.Remove(path);
        }
    }

    private void OnFileRenamed(string oldPath, string newPath)
    {
        if (_documents.TryGetValue(oldPath, out var id))
        {
            Workspace.TryApplyChanges(Workspace.CurrentSolution.RemoveDocument(id));
            _documents.Remove(oldPath);
        }

        if (Path.GetExtension(newPath).Equals(".cs", StringComparison.OrdinalIgnoreCase))
        {
            AddDocument(newPath);
        }
    }

    private void OnFileCreated(string path)
    {
        if (_documents.ContainsKey(path))
            return;

        if (Path.GetExtension(path).Equals(".cs", StringComparison.OrdinalIgnoreCase))
        {
            AddDocument(path);
        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        Workspace?.Dispose();
    }
}

