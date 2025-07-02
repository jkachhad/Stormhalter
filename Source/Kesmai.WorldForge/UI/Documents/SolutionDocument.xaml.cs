using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Win32;

namespace Kesmai.WorldForge.UI.Documents;

public partial class SolutionDocument : UserControl
{
    public SolutionDocument()
    {
        InitializeComponent();
    }

    private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is SolutionFile file)
        {
            var vm = DataContext as SolutionViewModel;
            var document = vm?.CurrentSolution?.GetDocument(file.DocumentId);
            if (document != null)
            {
                var text = document.GetTextAsync().Result;
                _codeEditor.Document.Text = text.ToString();
            }
        }
    }
}

public class SolutionFile : ObservableObject
{
    public string Name { get; set; }
    public string FullPath { get; set; }
    public string ProjectPath { get; set; }
    public DocumentId DocumentId { get; set; }
    public ObservableCollection<SolutionFile> Children { get; } = new();
}

public class SolutionViewModel : ObservableObject, IDisposable
{
    private readonly string _segmentName;
    public string Name => $"{_segmentName} (Solution)";
    public ObservableCollection<SolutionFile> Files { get; } = new();

    private readonly string _solutionPath;
    private readonly List<ProjectId> _projects = new();
    private AdhocWorkspace _workspace;
    private Solution _solution;

    internal Solution CurrentSolution => _solution;

    public RelayCommand AddFileCommand { get; }
    public RelayCommand<SolutionFile> DeleteFileCommand { get; }
    public RelayCommand<SolutionFile> RenameFileCommand { get; }

    public SolutionViewModel(string solutionPath)
    {
        _solutionPath = solutionPath;
        _segmentName = Path.GetFileNameWithoutExtension(solutionPath);
        LoadSolution();

        AddFileCommand = new RelayCommand(AddFile);
        DeleteFileCommand = new RelayCommand<SolutionFile>(DeleteFile, f => f != null);
        RenameFileCommand = new RelayCommand<SolutionFile>(RenameFile, f => f != null);
    }

    private void LoadSolution()
    {
        if (!File.Exists(_solutionPath))
            return;

        MSBuildLocator.RegisterDefaults();
        using var msWorkspace = MSBuildWorkspace.Create();
        var loadedSolution = msWorkspace.OpenSolutionAsync(_solutionPath).Result;

        _workspace = new AdhocWorkspace();
        _workspace.TryApplyChanges(loadedSolution);
        _solution = _workspace.CurrentSolution;

        _projects.Clear();
        foreach (var project in _solution.Projects)
        {
            _projects.Add(project.Id);
            foreach (var document in project.Documents)
            {
                if (document.FilePath == null)
                    continue;
                Files.Add(new SolutionFile
                {
                    Name = Path.GetFileName(document.FilePath),
                    FullPath = document.FilePath,
                    ProjectPath = project.FilePath ?? string.Empty,
                    DocumentId = document.Id
                });
            }
        }
    }

    private void AddFile()
    {
        var projectId = _projects.FirstOrDefault();
        if (projectId == default)
            return;

        var project = _solution.GetProject(projectId);
        if (project == null)
            return;

        var dialog = new SaveFileDialog
        {
            DefaultExt = ".cs",
            Filter = "C# File (*.cs)|*.cs",
            InitialDirectory = Path.GetDirectoryName(project.FilePath)
        };

        if (dialog.ShowDialog() == true)
        {
            if (!File.Exists(dialog.FileName))
                File.WriteAllText(dialog.FileName, string.Empty);

            var documentId = DocumentId.CreateNewId(projectId);
            var newSolution = project.AddDocument(documentId, Path.GetFileName(dialog.FileName), SourceText.From(string.Empty), filePath: dialog.FileName).Project.Solution;
            _workspace.TryApplyChanges(newSolution);
            _solution = _workspace.CurrentSolution;

            UpdateProjectAdd(project.FilePath!, dialog.FileName);

            Files.Add(new SolutionFile
            {
                Name = Path.GetFileName(dialog.FileName),
                FullPath = dialog.FileName,
                ProjectPath = project.FilePath ?? string.Empty,
                DocumentId = documentId
            });
        }
    }

    private void DeleteFile(SolutionFile file)
    {
        if (file == null)
            return;

        if (MessageBox.Show($"Delete '{file.Name}'?", "WorldForge", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        if (File.Exists(file.FullPath))
            File.Delete(file.FullPath);

        var project = _solution.GetProject(file.DocumentId.ProjectId);
        if (project != null)
        {
            var newSolution = project.RemoveDocument(file.DocumentId).Solution;
            _workspace.TryApplyChanges(newSolution);
            _solution = _workspace.CurrentSolution;
        }

        UpdateProjectRemove(file.ProjectPath, file.FullPath);

        Files.Remove(file);
    }

    private void RenameFile(SolutionFile file)
    {
        if (file == null)
            return;

        var dialog = new InputDialog("Rename file", file.Name);
        if (dialog.ShowDialog() != true)
            return;

        var newName = dialog.Input;
        if (string.IsNullOrWhiteSpace(newName))
            return;

        var newPath = Path.Combine(Path.GetDirectoryName(file.FullPath)!, newName);

        File.Move(file.FullPath, newPath);

        var newSolution = _solution.WithDocumentFilePath(file.DocumentId, newPath);
        _workspace.TryApplyChanges(newSolution);
        _solution = _workspace.CurrentSolution;

        UpdateProjectRename(file.ProjectPath, file.FullPath, newPath);

        file.Name = Path.GetFileName(newPath);
        file.FullPath = newPath;
    }

    private void UpdateProjectAdd(string projectPath, string filePath)
    {
        var doc = XDocument.Load(projectPath);
        var ns = doc.Root!.Name.Namespace;
        var itemGroup = doc.Root.Elements(ns + "ItemGroup").FirstOrDefault();
        if (itemGroup == null)
        {
            itemGroup = new XElement(ns + "ItemGroup");
            doc.Root.Add(itemGroup);
        }
        var relative = Path.GetRelativePath(Path.GetDirectoryName(projectPath)!, filePath).Replace(Path.DirectorySeparatorChar, '\\');
        itemGroup.Add(new XElement(ns + "Compile", new XAttribute("Include", relative)));
        doc.Save(projectPath);
    }

    private void UpdateProjectRemove(string projectPath, string filePath)
    {
        var doc = XDocument.Load(projectPath);
        var ns = doc.Root!.Name.Namespace;
        var relative = Path.GetRelativePath(Path.GetDirectoryName(projectPath)!, filePath).Replace(Path.DirectorySeparatorChar, '\\');
        var compile = doc.Descendants(ns + "Compile").FirstOrDefault(e => string.Equals(e.Attribute("Include")?.Value, relative, StringComparison.OrdinalIgnoreCase));
        compile?.Remove();
        doc.Save(projectPath);
    }

    private void UpdateProjectRename(string projectPath, string oldPath, string newPath)
    {
        var doc = XDocument.Load(projectPath);
        var ns = doc.Root!.Name.Namespace;
        var oldRel = Path.GetRelativePath(Path.GetDirectoryName(projectPath)!, oldPath).Replace(Path.DirectorySeparatorChar, '\\');
        var newRel = Path.GetRelativePath(Path.GetDirectoryName(projectPath)!, newPath).Replace(Path.DirectorySeparatorChar, '\\');
        var compile = doc.Descendants(ns + "Compile").FirstOrDefault(e => string.Equals(e.Attribute("Include")?.Value, oldRel, StringComparison.OrdinalIgnoreCase));
        if (compile != null)
        {
            compile.SetAttributeValue("Include", newRel);
            doc.Save(projectPath);
        }
    }

    public void Dispose()
    {
        Files.Clear();
        _workspace?.Dispose();
    }
}
