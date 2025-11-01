using System;
using System.Linq;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;

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

			if (subregion.Rectangles.Any())
				_presenter.SetBounds(subregion.Rectangles.First());
		});
    }

	private void RectanglesSelectionChanged(object sender, SelectionChangedEventArgs args)
	{
		var selectedBounds = args.AddedItems.OfType<SegmentBounds>().FirstOrDefault();

		if (selectedBounds is null && sender is DataGrid grid)
			selectedBounds = grid.SelectedItem as SegmentBounds;

		if (selectedBounds is null || !selectedBounds.IsValid)
			return;

		_presenter.SetBounds(selectedBounds);
	}
}

public class SubregionViewModel : ObservableRecipient
{
	public string Name => "(Subregions)";
}
