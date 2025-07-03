using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.IO;
using System.Linq;

namespace Kesmai.WorldForge.Roslyn;

public class MapProjectWorkspace : IDisposable
{
    public AdhocWorkspace Workspace { get; }
    public Project Project { get; private set; }
    public string FolderPath { get; }

    public MapProjectWorkspace(string folderPath, string projectName)
    {
        FolderPath = folderPath;
        Workspace = new AdhocWorkspace();

        var projectInfo = ProjectInfo.Create(
            ProjectId.CreateNewId(projectName),
            VersionStamp.Create(),
            projectName,
            projectName,
            LanguageNames.CSharp,
            filePath: Path.Combine(folderPath, projectName + ".csproj"));

        Workspace.AddProject(projectInfo);

        var csFiles = Directory.EnumerateFiles(FolderPath, "*.cs", SearchOption.TopDirectoryOnly);
        foreach (var file in csFiles)
        {
            var source = SourceText.From(File.ReadAllText(file));
            var docInfo = DocumentInfo.Create(
                DocumentId.CreateNewId(projectInfo.Id),
                Path.GetFileName(file),
                loader: TextLoader.From(TextAndVersion.Create(source, VersionStamp.Create())),
                filePath: file);
            Workspace.AddDocument(docInfo);
        }

        Project = Workspace.CurrentSolution.GetProject(projectInfo.Id);
    }

    public void AddFile(string fileName)
    {
        var filePath = Path.Combine(FolderPath, fileName);
        if (!File.Exists(filePath))
            File.WriteAllText(filePath, string.Empty);
        var source = SourceText.From(File.ReadAllText(filePath));
        var docInfo = DocumentInfo.Create(
            DocumentId.CreateNewId(Project.Id),
            fileName,
            loader: TextLoader.From(TextAndVersion.Create(source, VersionStamp.Create())),
            filePath: filePath);
        Workspace.AddDocument(docInfo);
        Project = Workspace.CurrentSolution.GetProject(Project.Id);
    }

    public void RemoveFile(string fileName)
    {
        var document = Project.Documents.FirstOrDefault(d => Path.GetFileName(d.FilePath) == fileName);
        if (document != null)
        {
            Workspace.RemoveDocument(document.Id);
            if (File.Exists(document.FilePath))
                File.Delete(document.FilePath);
            Project = Workspace.CurrentSolution.GetProject(Project.Id);
        }
    }

    public void RenameFile(string oldName, string newName)
    {
        var document = Project.Documents.FirstOrDefault(d => Path.GetFileName(d.FilePath) == oldName);
        if (document != null)
        {
            var folder = Path.GetDirectoryName(document.FilePath);
            var newPath = Path.Combine(folder, newName);
            if (File.Exists(newPath))
                File.Delete(newPath);
            File.Move(document.FilePath, newPath);

            Workspace.RemoveDocument(document.Id);
            var source = SourceText.From(File.ReadAllText(newPath));
            var docInfo = DocumentInfo.Create(
                DocumentId.CreateNewId(Project.Id),
                newName,
                loader: TextLoader.From(TextAndVersion.Create(source, VersionStamp.Create())),
                filePath: newPath);
            Workspace.AddDocument(docInfo);
            Project = Workspace.CurrentSolution.GetProject(Project.Id);
        }
    }

    public void Dispose()
    {
        Workspace.Dispose();
    }
}
