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
        if (e.NewValue is SolutionFile file && File.Exists(file.FullPath))
        {
            _codeEditor.Document.Text = File.ReadAllText(file.FullPath);
        }
    }
}

public class SolutionFile : ObservableObject
{
    public string Name { get; set; }
    public string FullPath { get; set; }
    public string ProjectPath { get; set; }
    public ObservableCollection<SolutionFile> Children { get; } = new();
}

public class SolutionViewModel : ObservableObject, IDisposable
{
    public string Name => "(Solution)";
    public ObservableCollection<SolutionFile> Files { get; } = new();

    private readonly string _solutionPath;
    private readonly List<string> _projects = new();

    public RelayCommand AddFileCommand { get; }
    public RelayCommand<SolutionFile> DeleteFileCommand { get; }
    public RelayCommand<SolutionFile> RenameFileCommand { get; }

    public SolutionViewModel(string solutionPath)
    {
        _solutionPath = solutionPath;
        LoadSolution();

        AddFileCommand = new RelayCommand(AddFile);
        DeleteFileCommand = new RelayCommand<SolutionFile>(DeleteFile, f => f != null);
        RenameFileCommand = new RelayCommand<SolutionFile>(RenameFile, f => f != null);
    }

    private void LoadSolution()
    {
        if (!File.Exists(_solutionPath))
            return;

        var baseDir = Path.GetDirectoryName(_solutionPath);
        _projects.Clear();
        foreach (var line in File.ReadLines(_solutionPath))
        {
            var trimmed = line.TrimStart();
            if (!trimmed.StartsWith("Project("))
                continue;
            var parts = trimmed.Split(',');
            if (parts.Length < 2)
                continue;
            var projectPath = parts[1].Trim().Trim('"');
            projectPath = Path.Combine(baseDir, projectPath);
            if (!File.Exists(projectPath))
                continue;
            _projects.Add(projectPath);
            ParseProject(projectPath);
        }
    }

    private void ParseProject(string projectPath)
    {
        var projectDir = Path.GetDirectoryName(projectPath);
        var doc = XDocument.Load(projectPath);
        foreach (var compile in doc.Descendants().Where(e => e.Name.LocalName == "Compile"))
        {
            var include = compile.Attribute("Include")?.Value;
            if (string.IsNullOrEmpty(include))
                continue;
            var filePath = Path.Combine(projectDir, include.Replace('\\', Path.DirectorySeparatorChar));
            Files.Add(new SolutionFile { Name = Path.GetFileName(filePath), FullPath = filePath, ProjectPath = projectPath });
        }
    }

    private void AddFile()
    {
        var project = _projects.FirstOrDefault();
        if (string.IsNullOrEmpty(project))
            return;

        var dialog = new SaveFileDialog
        {
            DefaultExt = ".cs",
            Filter = "C# File (*.cs)|*.cs",
            InitialDirectory = Path.GetDirectoryName(project)
        };

        if (dialog.ShowDialog() == true)
        {
            if (!File.Exists(dialog.FileName))
                File.WriteAllText(dialog.FileName, string.Empty);

            UpdateProjectAdd(project, dialog.FileName);

            Files.Add(new SolutionFile
            {
                Name = Path.GetFileName(dialog.FileName),
                FullPath = dialog.FileName,
                ProjectPath = project
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
    }
}
