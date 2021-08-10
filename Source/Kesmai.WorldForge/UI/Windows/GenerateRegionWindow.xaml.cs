using Kesmai.WorldForge.Editor;
using System;
using System.IO;
using System.Text;
using System.Windows;
using CommonServiceLocator;
using DigitalRune.ServiceLocation;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Kesmai.WorldForge
{
	/// <summary>
	/// Interaction logic for GenerateRegionWindow.xaml
	/// </summary>
	public partial class GenerateRegionWindow : Window
    {
#if (CanImport)
	    private MapGENImporter _mapGENImporter;
#endif
	    
		public GenerateRegionWindow()
		{
			InitializeComponent();
		}
		
		private void BrowseMapGENData(object sender, RoutedEventArgs e)
		{
#if (CanImport)
			var dialog = new Microsoft.Win32.OpenFileDialog()
			{
				DefaultExt = ".def",
				Filter = "Map Gen Definition (*.def)|*.def",
			};

			var result = dialog.ShowDialog();

			if (!result.HasValue || result != true)
				return;

			if (DataContext is ApplicationPresenter presenter)
			{
				_mapGENImporter = new MapGENImporter();
				_mapGENImporter.Import(_mapGENPath.Text = dialog.FileName);
					
				_mapGENData.DataContext = _mapGENImporter;
			}
#endif
		}
		
		private void ImportMapGENRegions(object sender, RoutedEventArgs e)
		{
#if (CanImport)
			var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
			var segment = segmentRequest.Response;
			
			foreach (var convertibleRegion in _mapGENImporter.Regions)
			{
				if (convertibleRegion.Import && convertibleRegion.Region != null)
					segment.Regions.Add(convertibleRegion.Region);
			}

			Close();
#endif
		}
	}
}
