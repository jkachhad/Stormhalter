using System.Linq;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
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

	public SegmentHoard ActiveHoard
	{
		get => _hoard;
		set
		{
			if (SetProperty(ref _hoard, value))
			{
				OnPropertyChanged(nameof(Name));

				if (_hoard != null)
					SelectedTreasureEntry = _hoard.Entries.FirstOrDefault();
			}
		}
	}

	public TreasureEntry SelectedTreasureEntry
	{
		get => _selectedTreasureEntry;
		set => SetProperty(ref _selectedTreasureEntry, value);
	}

	public HoardViewModel(SegmentHoard hoard)
	{
		ActiveHoard = hoard;
	}
}
