using System.Linq;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
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

	public string Name => "(Treasure)";

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

	public TreasureViewModel(SegmentTreasure treasure)
	{
		ActiveTreasure = treasure;
	}
}
