using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.UI.Documents;

public partial class EntitiesDocument : UserControl
{
	private EntitiesViewModel? _viewModel;
	
	public ObservableCollection<SegmentSpawner> Spawns { get; } = new ObservableCollection<SegmentSpawner>();
	
	public EntitiesDocument()
	{
		InitializeComponent();
		
		DataContextChanged += OnDataContextChanged;
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
	{
		if (_viewModel != null)
			_viewModel.PropertyChanged -= OnViewModelPropertyChanged;

		_viewModel = args.NewValue as EntitiesViewModel;

		if (_viewModel != null)
			_viewModel.PropertyChanged += OnViewModelPropertyChanged;

		UpdateSpawns();
	}
	
	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
	{
		if (_viewModel == null)
			return;

		if (args.PropertyName == nameof(EntitiesViewModel.Entity))
			UpdateSpawns();
	}

	private void UpdateSpawns()
	{
		Spawns.Clear();

		if (_viewModel.Entity != null)
		{
			var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
			var segment = presenter.Segment;

			if (segment is null)
				return;

			var spawns = segment.Spawns.GetSpawns(_viewModel.Entity);
				
			foreach (var spawn in spawns)
				Spawns.Add(spawn);
		}
	}
	
	private void SpawnerButtonClick(object sender, RoutedEventArgs e)
	{
		if (sender is not Button { DataContext: SegmentSpawner spawner })
			return;
		
		var applicationPresenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();

		if (applicationPresenter != null)
			spawner.Present(applicationPresenter);
	}
}

public class EntitiesViewModel : ObservableRecipient
{
	public string Name => "(Entities)";

	private SegmentEntity? _entity;
	private Script? _selectedScript;

	public SegmentEntity? Entity
	{
		get => _entity;
		set
		{
			if (!SetProperty(ref _entity, value))
				return;

			if (_entity != null)
			{
				if (_selectedScript is null || !_entity.Scripts.Contains(_selectedScript))
					SelectedScript = _entity.Scripts.FirstOrDefault(s => s.IsEnabled) ?? _entity.Scripts.FirstOrDefault();
			}
			else
			{
				SelectedScript = null;
			}
		}
	}

	public Script? SelectedScript
	{
		get => _selectedScript;
		set => SetProperty(ref _selectedScript, value);
	}
}
