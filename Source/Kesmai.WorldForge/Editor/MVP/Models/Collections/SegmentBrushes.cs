using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.Editor;

public class SegmentBrushAdded(SegmentBrush brush) : ValueChangedMessage<SegmentBrush>(brush);
public class SegmentBrushRemoved(SegmentBrush brush) : ValueChangedMessage<SegmentBrush>(brush);
public class SegmentBrushesChanged(SegmentBrushes brushes) : ValueChangedMessage<SegmentBrushes>(brushes);

public class SegmentBrushes : ObservableCollection<SegmentBrush>
{
	public string Name => "(Brushes)";

	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		base.OnCollectionChanged(args);

		if (args.NewItems != null)
		{
			foreach (var newItem in args.NewItems.OfType<SegmentBrush>())
				WeakReferenceMessenger.Default.Send(new SegmentBrushAdded(newItem));
		}

		if (args.OldItems != null)
		{
			foreach (var oldItem in args.OldItems.OfType<SegmentBrush>())
				WeakReferenceMessenger.Default.Send(new SegmentBrushRemoved(oldItem));
		}

		WeakReferenceMessenger.Default.Send(new SegmentBrushesChanged(this));
	}
}
