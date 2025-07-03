using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kesmai.WorldForge.Editor.Roslyn;
using Kesmai.WorldForge.Scripting;

namespace Kesmai.WorldForge.UI.Documents;

public partial class SolutionDocument : UserControl
{
    public SolutionDocument()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private SolutionViewModel _viewModel;

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.SelectedFileChanged -= OnSelectedFileChanged;
        }

        _viewModel = e.NewValue as SolutionViewModel;

        if (_viewModel != null)
        {
            _viewModel.SelectedFileChanged += OnSelectedFileChanged;
            LoadSelectedFile();
        }
    }


    private void OnSelectedFileChanged(object sender, EventArgs e)
    {
        LoadSelectedFile();
    }

    private void LoadSelectedFile()
    {
        if (_viewModel?.CurrentFilePath != null)
        {
            _codeEditor.LoadFile(_viewModel.CurrentFilePath);
        }
    }
}

public class SolutionViewModel : ObservableObject
{
    private MapProjectWorkspace _workspace;
    private string _selectedFile;

    public string WorkspaceFolder => _workspace?.FolderPath;

    public string CurrentFilePath =>
        !string.IsNullOrEmpty(SelectedFile) && WorkspaceFolder != null
            ? Path.Combine(WorkspaceFolder, SelectedFile)
            : null;

    public event EventHandler SelectedFileChanged;

    public string Name => "(Solution)";

    public ObservableCollection<string> Files { get; } = new();

    public string SelectedFile
    {
        get => _selectedFile;
        set
        {
            if (SetProperty(ref _selectedFile, value))
            {
                SelectedFileChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public RelayCommand AddFileCommand { get; }
    public RelayCommand<string> RemoveFileCommand { get; }
    public RelayCommand<string> RenameFileCommand { get; }

    public SolutionViewModel(MapProjectWorkspace workspace)
    {
        _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        RefreshFiles();

        AddFileCommand = new RelayCommand(AddFile);
        RemoveFileCommand = new RelayCommand<string>(RemoveFile, f => f != null);
        RenameFileCommand = new RelayCommand<string>(RenameFile, f => f != null);
    }

    public void SetWorkspace(MapProjectWorkspace workspace)
    {
        _workspace = workspace;
        RefreshFiles();
    }

    private void RefreshFiles()
    {
        Files.Clear();
        if (_workspace?.Project != null)
        {
            foreach (var doc in _workspace.Project.Documents)
                Files.Add(Path.GetFileName(doc.FilePath));
        }
    }

    private void AddFile()
    {
        if (_workspace == null)
            return;
        var baseName = "NewFile";
        var index = 1;
        var name = $"{baseName}{index}.cs";
        while (Files.Contains(name))
            name = $"{baseName}{++index}.cs";
        _workspace.AddFile(name);
        RefreshFiles();
        SelectedFile = name;
    }

    private void RemoveFile(string file)
    {
        _workspace?.RemoveFile(file);
        RefreshFiles();
    }

    private void RenameFile(string file)
    {
        if (_workspace == null)
            return;
        var dialog = new Kesmai.WorldForge.UI.Documents.InputDialog("Rename file", file);
        if (dialog.ShowDialog() == true)
        {
            var newName = dialog.Input;
            if (!string.IsNullOrWhiteSpace(newName) && newName != file)
            {
                _workspace.RenameFile(file, newName);
                RefreshFiles();
                SelectedFile = newName;
            }
        }
    }
}
