using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.Editor;

public class SegmentBrushAdded(SegmentBrush brush) : ValueChangedMessage<SegmentBrush>(brush);
public class SegmentBrushRemoved(SegmentBrush brush) : ValueChangedMessage<SegmentBrush>(brush);
public class SegmentBrushesChanged(SegmentBrushes brushes) : ValueChangedMessage<SegmentBrushes>(brushes);

public class SegmentBrushes : ObservableCollection<SegmentBrush>
{
	public string Name => "(Brushes)";

	public void Load(XElement element, Version version)
	{
		Clear();
		
		foreach (var brushElement in element.Elements("brush"))
			Add(new SegmentBrush(brushElement));
	}

	public void Save(XElement element)
	{
		foreach (var brush in this)
			element.Add(brush.GetSerializingElement());
	}

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
