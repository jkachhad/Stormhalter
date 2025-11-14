using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.UI.Documents;

public partial class TreasureDocument : UserControl
{
	private TreasureViewModel? _viewModel;

	public TreasureDocument()
	{
		InitializeComponent();

		DataContextChanged += OnDataContextChanged;
	}
	
	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (e.OldValue is TreasureViewModel oldViewModel)
			oldViewModel.PropertyChanged -= OnViewModelPropertyChanged;

		_viewModel = e.NewValue as TreasureViewModel;

		if (_viewModel != null)
			_viewModel.PropertyChanged += OnViewModelPropertyChanged;
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
	}

	private void OnAddEntryClick(object sender, RoutedEventArgs e)
	{
		if (_viewModel is null || _viewModel.Treasure is null)
			return;
		
		var entry = new TreasureEntry(_viewModel.Treasure);
		
		_viewModel.Treasure.Entries.Add(entry);
		_viewModel.SelectedTreasureEntry = entry;
	}

	private void OnRemoveEntryClick(object sender, RoutedEventArgs e)
	{
		if (_viewModel is null || _viewModel.Treasure is null || _viewModel.SelectedTreasureEntry is null)
			return;
		
		var result = MessageBox.Show("Are you sure you want to delete the selected entry?", "Delete Entry",
			MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

		if (result != MessageBoxResult.Yes)
			return;

		var treasure = _viewModel.Treasure;
		var selectedEntry = _viewModel.SelectedTreasureEntry;

		var currentIndex = treasure.Entries.IndexOf(selectedEntry);

		if (currentIndex < 0)
			return;

		treasure.Entries.RemoveAt(currentIndex);

		if (treasure.Entries.Count > 0)
		{
			if (currentIndex >= treasure.Entries.Count)
				currentIndex = treasure.Entries.Count - 1;

			_viewModel.SelectedTreasureEntry = treasure.Entries.ElementAt(currentIndex);
		}
		else
		{
			_viewModel.SelectedTreasureEntry = null;
		}
	}
}

public class TreasureViewModel : ObservableRecipient
{
	private SegmentTreasure? _treasure;
	private TreasureEntry? _selectedTreasureEntry;

	public string Name => "(Treasure)";
	
	public SegmentTreasure? Treasure
	{
		get => _treasure;
		set
		{
			if (SetProperty(ref _treasure, value))
			{
				if (_treasure != null)
					SelectedTreasureEntry = _treasure.Entries.FirstOrDefault();
			}
		}
	}

	public TreasureEntry? SelectedTreasureEntry
	{
		get => _selectedTreasureEntry;
		set => SetProperty(ref _selectedTreasureEntry, value);
	}
}
