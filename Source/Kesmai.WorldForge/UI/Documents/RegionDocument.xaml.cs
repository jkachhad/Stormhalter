using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using System.Windows.Controls;

namespace Kesmai.WorldForge.UI.Documents;

public partial class RegionDocument : UserControl
{
	public RegionDocument()
	{
		InitializeComponent();
		
		Loaded += (sender, args) =>
		{
			// update tiles when the document is loaded
			if (DataContext is SegmentRegion region)
				region.UpdateTiles();
			
			// Listen for filter changes
			WeakReferenceMessenger.Default.Register<RegionDocument, RegionFilterChanged>(this, OnFilterChanged);
		};
		
		Unloaded += (sender, args) =>
		{
			WeakReferenceMessenger.Default.Unregister<RegionFilterChanged>(this);
		};
	}

	private void OnFilterChanged(RegionDocument recipient, RegionFilterChanged message)
	{
		if (DataContext is SegmentRegion region)
			region.UpdateTiles();

		_presenter.WorldScreen.InvalidateRender();
	}
}