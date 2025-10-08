using System.Windows;
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
			
			// Listen for changes to the region that require a redraw
			WeakReferenceMessenger.Default.Register<RegionDocument, RegionFilterChanged>(this, (_, _) => Update());
			WeakReferenceMessenger.Default.Register<RegionDocument, RegionVisibilityChanged>(this, (_, _) => Update());
			
			WeakReferenceMessenger.Default.Register<RegionDocument, ToolSelected>(this, (_, message) =>
			{
				if (message.Value is (DrawTool or PaintTool))
					_componentsPanel.Visibility = Visibility.Visible;
				else
					_componentsPanel.Visibility = Visibility.Collapsed;
			});
		};
		
		Unloaded += (sender, args) =>
		{
			WeakReferenceMessenger.Default.UnregisterAll(this);
		};
	}

	private void Update()
	{
		if (DataContext is SegmentRegion region)
			region.UpdateTiles();

		_presenter.WorldScreen.InvalidateRender();
	}
}