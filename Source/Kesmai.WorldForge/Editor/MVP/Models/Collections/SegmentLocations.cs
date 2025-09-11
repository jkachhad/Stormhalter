using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.Editor;

public class SegmentLocationCreated(SegmentLocation location) : ValueChangedMessage<SegmentLocation>(location);
public class SegmentLocationDeleted(SegmentLocation location) : ValueChangedMessage<SegmentLocation>(location);
public class SegmentLocationsChanged(SegmentLocations locations) : ValueChangedMessage<SegmentLocations>(locations);

public class SegmentLocations : ObservableCollection<SegmentLocation>
{
	public string Name => "(Locations)";

	public void Load(XElement element, Version version)
	{
		Clear();
		
		foreach (var locationElement in element.Elements("location"))
			Add(new SegmentLocation(locationElement));
	}

	public void Save(XElement element)
	{
		foreach (var location in this)
			element.Add(location.GetXElement());
	}

	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		base.OnCollectionChanged(args);
		
		if (args.NewItems != null)
		{
			foreach (var newItem in args.NewItems.OfType<SegmentLocation>())
				WeakReferenceMessenger.Default.Send(new SegmentLocationCreated(newItem));
		}
			
		if (args.OldItems != null)
		{
			foreach (var oldItem in args.OldItems.OfType<SegmentLocation>())
				WeakReferenceMessenger.Default.Send(new SegmentLocationDeleted(oldItem));
		}

		WeakReferenceMessenger.Default.Send(new SegmentLocationsChanged(this));
	}
}