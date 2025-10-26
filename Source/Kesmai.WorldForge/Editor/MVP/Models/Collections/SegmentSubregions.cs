using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.Editor;

public class SegmentSubregionAdded(SegmentSubregion subregion) : ValueChangedMessage<SegmentSubregion>(subregion);
public class SegmentSubregionRemoved(SegmentSubregion subregion) : ValueChangedMessage<SegmentSubregion>(subregion);
public class SegmentSubregionsChanged(SegmentSubregions subregions) : ValueChangedMessage<SegmentSubregions>(subregions);

public class SegmentSubregions : ObservableCollection<SegmentSubregion>
{
	public void Load(XElement element, Version version)
	{
		Clear();
		
		foreach (var subregionElement in element.Elements("subregion"))
			Add(new SegmentSubregion(subregionElement));
	}

	public void Save(XElement element)
	{
		foreach (var subregion in this)
			element.Add(subregion.GetSerializingElement());
	}
	
	protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
	{
		base.OnCollectionChanged(args);
		
		if (args.NewItems != null)
		{
			foreach (var newItem in args.NewItems.OfType<SegmentSubregion>())
				WeakReferenceMessenger.Default.Send(new SegmentSubregionAdded(newItem));
		}
			
		if (args.OldItems != null)
		{
			foreach (var oldItem in args.OldItems.OfType<SegmentSubregion>())
				WeakReferenceMessenger.Default.Send(new SegmentSubregionRemoved(oldItem));
		}
		
		WeakReferenceMessenger.Default.Send(new SegmentSubregionsChanged(this));
	}
}