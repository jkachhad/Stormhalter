using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.UI.Documents;

public partial class HoardDocument : UserControl
{
	public HoardDocument()
	{
		InitializeComponent();
	}
}

public class HoardViewModel : ObservableRecipient
{
	private SegmentHoard _hoard;
	private TreasureEntry _selectedTreasureEntry;

	public string Name => "(Hoard)";

	public HoardViewModel()
	{
		AddEntryCommand = new RelayCommand(AddEntry, () => ActiveHoard != null);
		RemoveEntryCommand = new RelayCommand(RemoveEntry, () => ActiveHoard != null && SelectedTreasureEntry != null);
	}

	public HoardViewModel(SegmentHoard hoard) : this()
	{
		ActiveHoard = hoard;
	}

	public IRelayCommand AddEntryCommand { get; }
	public IRelayCommand RemoveEntryCommand { get; }

	public SegmentHoard ActiveHoard
	{
		get => _hoard;
		set
		{
			if (SetProperty(ref _hoard, value))
			{
				OnPropertyChanged(nameof(Name));

				SelectedTreasureEntry = _hoard?.Entries.FirstOrDefault();
				AddEntryCommand.NotifyCanExecuteChanged();
				RemoveEntryCommand.NotifyCanExecuteChanged();
			}
		}
	}

	public TreasureEntry SelectedTreasureEntry
	{
		get => _selectedTreasureEntry;
		set
		{
			if (SetProperty(ref _selectedTreasureEntry, value))
				RemoveEntryCommand.NotifyCanExecuteChanged();
		}
	}

	private void AddEntry()
	{
		if (ActiveHoard is null)
			return;

		var entry = new TreasureEntry(ActiveHoard);
		ActiveHoard.Entries.Add(entry);
		SelectedTreasureEntry = entry;
	}

	private void RemoveEntry()
	{
		if (ActiveHoard is null || SelectedTreasureEntry is null)
			return;

		var result = MessageBox.Show("Are you sure you want to delete the selected entry?", "Delete Entry",
			MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

		if (result != MessageBoxResult.Yes)
			return;

		var currentIndex = ActiveHoard.Entries.IndexOf(SelectedTreasureEntry);

		if (currentIndex < 0)
			return;

		ActiveHoard.Entries.RemoveAt(currentIndex);

		if (ActiveHoard.Entries.Count == 0)
		{
			SelectedTreasureEntry = null;
			return;
		}

		if (currentIndex >= ActiveHoard.Entries.Count)
			currentIndex = ActiveHoard.Entries.Count - 1;

		SelectedTreasureEntry = ActiveHoard.Entries.ElementAt(currentIndex);
	}
}
