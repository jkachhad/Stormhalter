using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

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
    public ObservableCollection<SolutionFile> Children { get; } = new();
}

public class SolutionViewModel : ObservableObject, IDisposable
{
    public string Name => "(Solution)";
    public ObservableCollection<SolutionFile> Files { get; } = new();

    private readonly string _solutionPath;

    public SolutionViewModel(string solutionPath)
    {
        _solutionPath = solutionPath;
        LoadSolution();
    }

    private void LoadSolution()
    {
        if (!File.Exists(_solutionPath))
            return;

        var baseDir = Path.GetDirectoryName(_solutionPath);
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
            Files.Add(new SolutionFile { Name = Path.GetFileName(filePath), FullPath = filePath });
        }
    }

    public void Dispose()
    {
        Files.Clear();
    }
}
