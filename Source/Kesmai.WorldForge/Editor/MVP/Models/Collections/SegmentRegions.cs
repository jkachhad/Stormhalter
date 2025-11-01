using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.Editor;

public class SegmentRegionAdded(SegmentRegion region) : ValueChangedMessage<SegmentRegion>(region);
public class SegmentRegionRemoved(SegmentRegion region) : ValueChangedMessage<SegmentRegion>(region);

public class SegmentRegionsChanged(SegmentRegions regions) : ValueChangedMessage<SegmentRegions>(regions);

public class SegmentRegions : ObservableCollection<SegmentRegion>
{
	public string Name => "(Regions)";

	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		base.OnCollectionChanged(args);

		if (args.NewItems != null)
		{
			foreach (var newItem in args.NewItems.OfType<SegmentRegion>())
				WeakReferenceMessenger.Default.Send(new SegmentRegionAdded(newItem));
		}

		if (args.OldItems != null)
		{
			foreach (var oldItem in args.OldItems.OfType<SegmentRegion>())
				WeakReferenceMessenger.Default.Send(new SegmentRegionRemoved(oldItem));
		}

		WeakReferenceMessenger.Default.Send(new SegmentRegionsChanged(this));
	}
}