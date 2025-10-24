using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.UI.Documents;

public partial class SegmentBrushDocument : UserControl
{
	public SegmentBrushDocument()
	{
		InitializeComponent();
	}
}

public class SegmentBrushViewModel : ObservableRecipient
{
	public string Name => "(Brushes)";
}
