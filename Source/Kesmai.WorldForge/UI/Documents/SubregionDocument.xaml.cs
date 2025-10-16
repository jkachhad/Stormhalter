using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using System.Windows.Controls;

namespace Kesmai.WorldForge.UI.Documents;

public partial class SubregionDocument : UserControl
{
	public SubregionDocument()
	{
		InitializeComponent();

		var messenger = WeakReferenceMessenger.Default;
        
		messenger.Register<ActiveContentChanged>(this, (_, message) =>
		{
			if (message.Value is not SegmentSubregion subregion)
				return;
            
			var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
			var segment = segmentRequest.Response;
            
			_presenter.Region = segment.GetRegion(subregion.Region);
			_presenter.SetSubregion(subregion);
		});
    }
}

public class SubregionViewModel : ObservableRecipient
{
	public string Name => "(Subregions)";
}