using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.Editor;

public class SegmentTreasureAdded(SegmentTreasure treasure) : ValueChangedMessage<SegmentTreasure>(treasure);
public class SegmentTreasureRemoved(SegmentTreasure treasure) : ValueChangedMessage<SegmentTreasure>(treasure);

public class SegmentTreasuresReset();
public class SegmentTreasuresChanged(SegmentTreasures treasures) : ValueChangedMessage<SegmentTreasures>(treasures);
	
public class SegmentTreasures : ObservableCollection<SegmentTreasure>
{
	public string Name => "(Treasures)";

	public void Load(XElement element, Version version)
	{
		Clear();
		
		foreach (var treasureElement in element.Elements("treasure"))
		{
			var isHoardAttribute = treasureElement.Attribute("hoard");

			if (isHoardAttribute != null && (bool)isHoardAttribute)
				Add(new SegmentHoard(treasureElement));
			else
				Add(new SegmentTreasure(treasureElement));
		}
	}
		
	public void Save(XElement element)
	{
		foreach (var treasure in this)
		{
			var treasureElement = treasure.GetSerializingElement();

			if (treasure is SegmentHoard)
				treasureElement.Add(new XAttribute("hoard", true));
			
			element.Add(treasureElement);
		}
	}
	
	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		base.OnCollectionChanged(args);
			
		if (args.NewItems != null)
		{
			foreach (var newItem in args.NewItems.OfType<SegmentTreasure>())
				WeakReferenceMessenger.Default.Send(new SegmentTreasureAdded(newItem));
		}
			
		if (args.OldItems != null)
		{
			foreach (var oldItem in args.OldItems.OfType<SegmentTreasure>())
				WeakReferenceMessenger.Default.Send(new SegmentTreasureRemoved(oldItem));
		}
		
		if (args.Action is NotifyCollectionChangedAction.Reset)
			WeakReferenceMessenger.Default.Send(new SegmentTreasuresReset());

		WeakReferenceMessenger.Default.Send(new SegmentTreasuresChanged(null));
	}
}