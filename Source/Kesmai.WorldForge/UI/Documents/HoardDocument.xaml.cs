using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.UI.Documents;

public partial class HoardDocument : UserControl
{
	private HoardViewModel? _viewModel;
	
	public HoardDocument()
	{
		InitializeComponent();

		DataContextChanged += OnDataContextChanged;
	}
	
	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (e.OldValue is HoardViewModel oldViewModel)
			oldViewModel.PropertyChanged -= OnViewModelPropertyChanged;

		_viewModel = e.NewValue as HoardViewModel;

		if (_viewModel != null)
			_viewModel.PropertyChanged += OnViewModelPropertyChanged;
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
	}
	
	private void OnAddEntryClick(object sender, RoutedEventArgs e)
	{
		if (_viewModel is null || _viewModel.Hoard is null)
			return;

		var entry = new TreasureEntry(_viewModel.Hoard);
		
		_viewModel.Hoard.Entries.Add(entry);
		_viewModel.SelectedTreasureEntry = entry;
	}

	private void OnRemoveEntryClick(object sender, RoutedEventArgs e)
	{
		if (_viewModel is null || _viewModel.Hoard is null || _viewModel.SelectedTreasureEntry is null)
			return;

		var result = MessageBox.Show("Are you sure you want to delete the selected entry?", "Delete Entry",
			MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

		if (result != MessageBoxResult.Yes)
			return;

		var hoard = _viewModel.Hoard;
		var selectedEntry = _viewModel.SelectedTreasureEntry;

		var currentIndex = hoard.Entries.IndexOf(selectedEntry);

		if (currentIndex < 0)
			return;

		hoard.Entries.RemoveAt(currentIndex);

		if (hoard.Entries.Count > 0)
		{
			if (currentIndex >= hoard.Entries.Count)
				currentIndex = hoard.Entries.Count - 1;

			_viewModel.SelectedTreasureEntry = hoard.Entries.ElementAt(currentIndex);
		}
		else
		{
			_viewModel.SelectedTreasureEntry = null;
		}
	}
}

public class HoardViewModel : ObservableRecipient
{
	private SegmentHoard? _hoard;
	private TreasureEntry? _selectedTreasureEntry;

	public string Name => "(Hoard)";

	public SegmentHoard? Hoard
	{
		get => _hoard;
		set
		{
			if (SetProperty(ref _hoard, value))
			{
				if (_hoard != null)
					SelectedTreasureEntry = _hoard.Entries.FirstOrDefault();
			}
		}
	}

	public TreasureEntry? SelectedTreasureEntry
	{
		get => _selectedTreasureEntry;
		set => SetProperty(ref _selectedTreasureEntry, value);
	}
}
