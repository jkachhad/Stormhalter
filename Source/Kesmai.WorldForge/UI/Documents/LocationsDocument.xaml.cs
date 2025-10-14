using System;
using System.Windows.Controls;
using Kesmai.WorldForge.Editor;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Kesmai.WorldForge.UI.Documents;

public partial class LocationsDocument : UserControl
{
	public LocationsDocument()
	{
		InitializeComponent();
		
		var messenger = WeakReferenceMessenger.Default;
		
		messenger.Register<ActiveContentChanged>(this, (_, message) =>
		{
			if (message.Value is not SegmentLocation location)
				return;
			
			var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
			var segment = segmentRequest.Response;

			_presenter.Region = segment.GetRegion(location.Region);
			_presenter.Location = location;
			_presenter.SetCameraLocation(location);

			_presenter.Focus();
		});
	}
}

public class LocationsViewModel : ObservableRecipient
{
	public string Name => "(Locations)";
}