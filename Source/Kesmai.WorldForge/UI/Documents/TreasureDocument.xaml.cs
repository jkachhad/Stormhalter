using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.UI.Documents;

public partial class TreasureDocument : UserControl
{
	public TreasureDocument()
	{
		InitializeComponent();
	}
}

public class TreasureViewModel : ObservableRecipient
{
	private SegmentTreasure _treasure;
	private TreasureEntry _selectedTreasureEntry;

	public string Name => _treasure?.Name ?? "(Treasure)";

	public TreasureViewModel()
	{
		AddEntryCommand = new RelayCommand(AddEntry, () => ActiveTreasure != null);
		AddEntryCommand.DependsOn(() => ActiveTreasure);
		
		RemoveEntryCommand = new RelayCommand(RemoveEntry, () => ActiveTreasure != null && SelectedTreasureEntry != null);
		RemoveEntryCommand.DependsOn(() => SelectedTreasureEntry);
	}

	public TreasureViewModel(SegmentTreasure treasure) : this()
	{
		ActiveTreasure = treasure;
	}

	public RelayCommand AddEntryCommand { get; }
	public RelayCommand RemoveEntryCommand { get; }

	public SegmentTreasure ActiveTreasure
	{
		get => _treasure;
		set
		{
			if (SetProperty(ref _treasure, value))
			{
				OnPropertyChanged(nameof(Name));

				if (_treasure != null)
					SelectedTreasureEntry = _treasure.Entries.FirstOrDefault();
			}
		}
	}

	public TreasureEntry SelectedTreasureEntry
	{
		get => _selectedTreasureEntry;
		set => SetProperty(ref _selectedTreasureEntry, value);
	}

	private void AddEntry()
	{
		if (ActiveTreasure is null)
			return;

		var entry = new TreasureEntry(ActiveTreasure);
		
		ActiveTreasure.Entries.Add(entry);
		SelectedTreasureEntry = entry;
	}

	private void RemoveEntry()
	{
		if (ActiveTreasure is null || SelectedTreasureEntry is null)
			return;

		var result = MessageBox.Show("Are you sure you want to delete the selected entry?", "Delete Entry",
			MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

		if (result != MessageBoxResult.Yes)
			return;

		var currentIndex = ActiveTreasure.Entries.IndexOf(SelectedTreasureEntry);

		if (currentIndex < 0)
			return;

		ActiveTreasure.Entries.RemoveAt(currentIndex);

		if (ActiveTreasure.Entries.Count > 0)
		{
			if (currentIndex >= ActiveTreasure.Entries.Count)
				currentIndex = ActiveTreasure.Entries.Count - 1;

			SelectedTreasureEntry = ActiveTreasure.Entries.ElementAt(currentIndex);
		}
		else
		{
			SelectedTreasureEntry = null;
		}
	}
}
