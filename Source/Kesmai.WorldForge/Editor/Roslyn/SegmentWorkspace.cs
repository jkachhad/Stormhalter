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
    public AdhocWorkspace Workspace { get; }

    private readonly InMemorySource _internal;
    private readonly Dictionary<string, DocumentId> _documents = new(StringComparer.OrdinalIgnoreCase);
    private readonly FileSystemWatcher _watcher;

    public SegmentWorkspace(string rootPath, RoslynHost host)
    {
        var sourcePath = Path.Combine(rootPath, "Source");

        Workspace = (AdhocWorkspace)host.CreateWorkspace();
        var project = Workspace.AddProject("InMemoryProject", LanguageNames.CSharp);

        var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        var systemCore = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);

        project = project.AddMetadataReference(mscorlib);
        project = project.AddMetadataReference(systemCore);
        Workspace.TryApplyChanges(project.Solution);

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

        var loader = TextLoader.From(TextAndVersion.Create(SourceText.From(text), VersionStamp.Create()));
        var documentInfo = DocumentInfo.Create(
            DocumentId.CreateNewId(project.Id),
            Path.GetFileName(filePath),
            filePath: filePath,
            loader: loader);

        var document = Workspace.AddDocument(documentInfo);
        _documents[filePath] = document.Id;
        return document;
    }

    private Document AddDocument(InMemorySource source)
    {
        var project = Workspace.CurrentSolution.Projects.First();
        var document = Workspace.AddDocument(project.Id, source.Name, SourceText.From(source.Text));
        var documentId = document.Id;

        source.TextChanged += (_, _) =>
        {
            var text = SourceText.From(source.Text);
            Workspace.TryApplyChanges(Workspace.CurrentSolution.WithDocumentText(documentId, text));
        };

        return document;
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

