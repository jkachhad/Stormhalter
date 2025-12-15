using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;

namespace Kesmai.WorldForge.UI.Documents;

public partial class LocationSpawnDocument : UserControl
{
    private LocationSpawnViewModel? _viewModel;

    public LocationSpawnDocument()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_viewModel != null)
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

        _viewModel = e.NewValue as LocationSpawnViewModel;

        if (_viewModel != null)
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        UpdatePresenter();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LocationSpawnViewModel.Spawner) || e.PropertyName == nameof(LocationSpawnViewModel.Region))
        {
            UpdatePresenter();
        }
    }

    private void UpdatePresenter()
    {
        if (_viewModel == null || _viewModel.Spawner is null || _viewModel.Region is null)
            return;

        _presenter.Region = _viewModel.Region;
        _presenter.Spawner = _viewModel.Spawner;
        _presenter.SetSpawner(_viewModel.Spawner);
    }
}

public class LocationSpawnViewModel : ObservableRecipient
{
    public string Name => "(Spawn)";

    private LocationSegmentSpawner? _spawner;
    private SegmentRegion? _region;
    private Script? _selectedScript;

    public LocationSegmentSpawner? Spawner
    {
        get => _spawner;
        set
        {
            if (!SetProperty(ref _spawner, value))
                return;

            if (_spawner != null)
            {
                if (_selectedScript is null || !_spawner.Scripts.Contains(_selectedScript))
                    SelectedScript = _spawner.Scripts.FirstOrDefault(s => s.IsEnabled) ?? _spawner.Scripts.FirstOrDefault();
            }
            else
            {
                SelectedScript = null;
            }
        }
    }

    public SegmentRegion? Region
    {
        get => _region;
        set => SetProperty(ref _region, value);
    }

    public Script? SelectedScript
    {
        get => _selectedScript;
        set => SetProperty(ref _selectedScript, value);
    }
}
