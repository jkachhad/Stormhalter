using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kesmai.Prototype.RoslynWorkspace;

public static class RoslynWorkspaceFactory
{
    private class WorkspaceState
    {
        public Dictionary<string, DocumentId> Documents { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> WatchedDirectories { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<FileSystemWatcher> Watchers { get; } = new();
    }

    private static readonly Dictionary<AdhocWorkspace, WorkspaceState> _states = new();

    private static WorkspaceState GetState(AdhocWorkspace workspace)
    {
        if (!_states.TryGetValue(workspace, out var state))
        {
            state = new WorkspaceState();
            _states[workspace] = state;
        }

        return state;
    }

    public static AdhocWorkspace CreateFromFolder(string folderPath)
    {
        var filePaths = Directory.EnumerateFiles(folderPath, "*.cs", SearchOption.AllDirectories);
        var workspace = Create(filePaths, Enumerable.Empty<InMemorySource>());
        WatchDirectory(workspace, folderPath);
        return workspace;
    }

    public static AdhocWorkspace Create(IEnumerable<string> filePaths, IEnumerable<InMemorySource> inMemorySources)
    {
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("InMemoryProject", LanguageNames.CSharp);

        var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        var systemCore = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);

        project = project.AddMetadataReference(mscorlib);
        project = project.AddMetadataReference(systemCore);
        workspace.TryApplyChanges(project.Solution);

        foreach (var path in filePaths)
        {
            AddDocument(workspace, path);
        }

        foreach (var source in inMemorySources)
        {
            AddDocument(workspace, source);
        }

        return workspace;
    }

    public static Document AddDocument(AdhocWorkspace workspace, string filePath)
    {
        var project = workspace.CurrentSolution.Projects.First();
        var text = File.ReadAllText(filePath);

        var loader = TextLoader.From(TextAndVersion.Create(SourceText.From(text), VersionStamp.Create()));
        var documentInfo = DocumentInfo.Create(
            DocumentId.CreateNewId(project.Id),
            Path.GetFileName(filePath),
            filePath: filePath,
            loader: loader);

        var document = workspace.AddDocument(documentInfo);

        var state = GetState(workspace);
        state.Documents[filePath] = document.Id;
        WatchDirectory(workspace, Path.GetDirectoryName(filePath)!);

        return document;
    }

    public static Document AddDocument(AdhocWorkspace workspace, InMemorySource source)
    {
        var project = workspace.CurrentSolution.Projects.First();
        var document = workspace.AddDocument(project.Id, source.Name, SourceText.From(source.Text));
        var documentId = document.Id;

        source.TextChanged += (_, _) =>
        {
            var text = SourceText.From(source.Text);
            workspace.TryApplyChanges(workspace.CurrentSolution.WithDocumentText(documentId, text));
        };

        return document;
    }

    private static void WatchDirectory(AdhocWorkspace workspace, string directory)
    {
        var state = GetState(workspace);
        if (!state.WatchedDirectories.Add(directory))
            return;

        var watcher = new FileSystemWatcher(directory, "*.cs")
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
        };

        watcher.Changed += (_, e) => OnFileChanged(workspace, e.FullPath);
        watcher.Created += (_, e) => OnFileCreated(workspace, e.FullPath);
        watcher.Deleted += (_, e) => OnFileDeleted(workspace, e.FullPath);
        watcher.Renamed += (_, e) => OnFileRenamed(workspace, e.OldFullPath, e.FullPath);

        watcher.EnableRaisingEvents = true;
        state.Watchers.Add(watcher);
    }

    private static void OnFileChanged(AdhocWorkspace workspace, string path)
    {
        var state = GetState(workspace);
        if (!state.Documents.TryGetValue(path, out var id))
            return;

        try
        {
            var text = SourceText.From(File.ReadAllText(path));
            workspace.TryApplyChanges(workspace.CurrentSolution.WithDocumentText(id, text));
        }
        catch (IOException)
        {
        }
    }

    private static void OnFileDeleted(AdhocWorkspace workspace, string path)
    {
        var state = GetState(workspace);
        if (state.Documents.TryGetValue(path, out var id))
        {
            workspace.TryApplyChanges(workspace.CurrentSolution.RemoveDocument(id));
            state.Documents.Remove(path);
        }
    }

    private static void OnFileRenamed(AdhocWorkspace workspace, string oldPath, string newPath)
    {
        var state = GetState(workspace);
        if (state.Documents.TryGetValue(oldPath, out var id))
        {
            workspace.TryApplyChanges(workspace.CurrentSolution.RemoveDocument(id));
            state.Documents.Remove(oldPath);
        }

        if (Path.GetExtension(newPath).Equals(".cs", StringComparison.OrdinalIgnoreCase))
        {
            AddDocument(workspace, newPath);
        }
    }

    private static void OnFileCreated(AdhocWorkspace workspace, string path)
    {
        var state = GetState(workspace);
        if (state.Documents.ContainsKey(path))
            return;

        if (Path.GetExtension(path).Equals(".cs", StringComparison.OrdinalIgnoreCase))
        {
            AddDocument(workspace, path);
        }
    }
}
