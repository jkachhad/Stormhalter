using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;

namespace Kesmai.WorldForge.UI.Documents;

public partial class RegionSpawnDocument : UserControl
{
    private RegionSpawnViewModel? _viewModel;

    public RegionSpawnDocument()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_viewModel != null)
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

        _viewModel = e.NewValue as RegionSpawnViewModel;

        if (_viewModel != null)
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        UpdatePresenter();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RegionSpawnViewModel.Spawner) || e.PropertyName == nameof(RegionSpawnViewModel.Region))
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

public class RegionSpawnViewModel : ObservableRecipient
{
    public string Name => "(Spawn)";

    private RegionSegmentSpawner? _spawner;
    private SegmentRegion? _region;
    private Script? _selectedScript;

    public RegionSegmentSpawner? Spawner
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