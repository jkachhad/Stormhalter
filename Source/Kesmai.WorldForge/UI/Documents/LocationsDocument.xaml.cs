using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Kesmai.WorldForge.Editor;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.UI.Documents
{
	public partial class LocationsDocument : UserControl
	{
		public LocationsDocument()
		{
			InitializeComponent();
			
			WeakReferenceMessenger.Default.Register<LocationsDocument, LocationsViewModel.SelectedLocationChangedMessage>
			(this, (r, m) => {
				var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
				var segment = segmentRequest.Response;

				var location = m.Value;

				if (location != null)
				{
					_presenter.Region = segment.GetRegion(location.Region);
					_presenter.SetLocation(location);
				}
			});

			WeakReferenceMessenger.Default.Register<LocationsDocument, UnregisterEvents>(this,
				(r, m) => { WeakReferenceMessenger.Default.UnregisterAll(this); });
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
		
		public RelayCommand AddLocationCommand { get; private set; }
		public RelayCommand<SegmentLocation> RemoveLocationCommand { get; private set; }
		public RelayCommand<SegmentLocation> CopyLocationCommand { get; private set; }
		public RelayCommand ImportLocationCommand { get; private set; }
		public RelayCommand<SegmentLocation> ExportLocationCommand { get; private set; }

		public LocationsViewModel(Segment segment)
		{
			_segment = segment ?? throw new ArgumentNullException(nameof(segment));
			
			AddLocationCommand = new RelayCommand(AddLocation);
			
			RemoveLocationCommand = new RelayCommand<SegmentLocation>(RemoveLocation, 
				(location) => SelectedLocation != null && !SelectedLocation.IsReserved);
			RemoveLocationCommand.DependsOn(() => SelectedLocation);
			
			CopyLocationCommand = new RelayCommand<SegmentLocation>(CopyLocation,
				(location) => SelectedLocation != null);
			CopyLocationCommand.DependsOn(() => SelectedLocation);

			ImportLocationCommand = new RelayCommand(ImportLocation);

			ExportLocationCommand = new RelayCommand<SegmentLocation>(ExportLocation,
				(location) => SelectedLocation != null && !SelectedLocation.IsReserved);
			ExportLocationCommand.DependsOn(() => SelectedLocation);
		}
		
		private void AddLocation()
		{
			var newLocation = new SegmentLocation()
			{
				Name = $"Location {_newLocationCount++}"
			};
			
			Locations.Add(newLocation);
			SelectedLocation = newLocation;
		}

		private void RemoveLocation(SegmentLocation location)
		{
			var result = MessageBox.Show($"Are you sure with to delete location '{location.Name}'?",
				"Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);

			if (result != MessageBoxResult.No)
				Locations.Remove(location);
		}

		private void CopyLocation(SegmentLocation location)
		{
			if (location.Clone() is SegmentLocation clonedLocation)
			{
				Locations.Add(clonedLocation);
				SelectedLocation = clonedLocation;
			}
		}

		public void ImportLocation ()
        {
			XDocument clipboard = null;
			try
			{
				clipboard = XDocument.Parse(Clipboard.GetText());
			}
			catch { }
			if (clipboard is null || clipboard.Root.Name.ToString() != "location")
				return;

			var newLocation = new SegmentLocation(clipboard.Root);
			Locations.Add(newLocation);
		}

		public void ExportLocation (SegmentLocation location)
        {
			Clipboard.SetText(location.GetXElement().ToString());
        }
	}
}