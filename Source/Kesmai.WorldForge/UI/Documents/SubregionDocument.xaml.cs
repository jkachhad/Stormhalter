using System;
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
	public partial class SubregionDocument : UserControl
	{
		public SubregionDocument()
		{
			InitializeComponent();
			
			WeakReferenceMessenger.Default.Register<SubregionDocument, SubregionViewModel.SelectedSubregionChangedMessage>
			(this, (r, m) => {
				var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
				var segment = segmentRequest.Response;

				var subregion = m.Value;

				if (subregion != null)
				{
					_presenter.Region = segment.GetRegion(subregion.Region);
					_presenter.SetSubregion(subregion);
				}
			});

			WeakReferenceMessenger.Default.Register<SubregionDocument, UnregisterEvents>(this,
				(r, m) => { WeakReferenceMessenger.Default.UnregisterAll(this); });
		}
	}

	public class SubregionViewModel : ObservableRecipient
	{
		public class SelectedSubregionChangedMessage : ValueChangedMessage<SegmentSubregion>
		{
			public SelectedSubregionChangedMessage(SegmentSubregion value) : base(value)
			{
			}
		}
		
		private int _newSubregionCount = 1;
		
		public string Name => "(Subregions)";
		
		private Segment _segment;
		private SegmentSubregion _selectedSubregion;
		
		public SegmentSubregion SelectedSubregion
		{
			get => _selectedSubregion;
			set
			{
				SetProperty(ref _selectedSubregion, value, true);
					
				if (value != null)
					WeakReferenceMessenger.Default.Send(new SelectedSubregionChangedMessage(value));
			}
		}
		
		public SegmentSubregions Subregions => _segment.Subregions;
		
		public RelayCommand AddSubregionCommand { get; private set; }
		public RelayCommand<SegmentSubregion> RemoveSubregionCommand { get; private set; }

		public RelayCommand ImportSubregionCommand { get; private set; }
		public RelayCommand<SegmentSubregion> ExportSubregionCommand { get; private set; }

		public SubregionViewModel(Segment segment)
		{
			_segment = segment ?? throw new ArgumentNullException(nameof(segment));
			
			AddSubregionCommand = new RelayCommand(AddSubregion);
			
			RemoveSubregionCommand = new RelayCommand<SegmentSubregion>(RemoveSubregion, 
				(location) => SelectedSubregion != null);
			RemoveSubregionCommand.DependsOn(() => SelectedSubregion);

			ImportSubregionCommand = new RelayCommand(ImportSubregion);

			ExportSubregionCommand = new RelayCommand<SegmentSubregion>(ExportSubregion,
				(location) => SelectedSubregion != null);
			ExportSubregionCommand.DependsOn(() => SelectedSubregion);
		}
		
		private void AddSubregion()
		{
			var newSubregion = new SegmentSubregion()
			{
				Name = $"Subregion {_newSubregionCount++}"
			};
			
			Subregions.Add(newSubregion);
			SelectedSubregion = newSubregion;
		}

		private void RemoveSubregion(SegmentSubregion subregion)
		{
			var result = MessageBox.Show($"Are you sure with to delete subregion '{subregion.Name}'?",
				"Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);

			if (result != MessageBoxResult.No)
				Subregions.Remove(subregion);
		}

		private void ImportSubregion()
        {
			XDocument clipboard = null;
			try
			{
				clipboard = XDocument.Parse(Clipboard.GetText());
			}
			catch { }
			if (clipboard is null || clipboard.Root.Name.ToString() != "subregion")
				return;

			var newSubregion = new SegmentSubregion(clipboard.Root);
			Subregions.Add(newSubregion);
		}

		private void ExportSubregion(SegmentSubregion subregion)
        {
			Clipboard.SetText(subregion.GetXElement().ToString());
        }
	}
}