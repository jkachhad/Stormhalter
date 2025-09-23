using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Kesmai.WorldForge.Editor;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

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
			_presenter.SetLocation(location);
		});
	}
}

public class LocationsViewModel : ObservableRecipient
{
	public class SelectedLocationChangedMessage : ValueChangedMessage<SegmentLocation>
	{
		public SelectedLocationChangedMessage(SegmentLocation value) : base(value)
		{
		}
	}
		
	private int _newLocationCount = 1;
		
	public string Name => "(Locations)";

	private Segment _segment;
	private SegmentLocation _selectedLocation;
		
	public SegmentLocation SelectedLocation
	{
		get => _selectedLocation;
		set
		{
			SetProperty(ref _selectedLocation, value, true);
					
			if (value != null)
				WeakReferenceMessenger.Default.Send(new SelectedLocationChangedMessage(value));
		}
	}

	public SegmentLocations Locations => _segment.Locations;

	public LocationsViewModel(Segment segment)
	{
		_segment = segment ?? throw new ArgumentNullException(nameof(segment));
	}
}