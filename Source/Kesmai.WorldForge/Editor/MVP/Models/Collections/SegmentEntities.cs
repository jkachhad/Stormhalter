using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.Editor;

public class SegmentEntityCreated(SegmentEntity segmentEntity) : ValueChangedMessage<SegmentEntity>(segmentEntity);
public class SegmentEntityDeleted(SegmentEntity segmentEntity) : ValueChangedMessage<SegmentEntity>(segmentEntity);

public class SegmentEntitiesReset();
public class SegmentEntitiesChanged(SegmentEntities entities) : ValueChangedMessage<SegmentEntities>(entities);

public class SegmentEntities : ObservableCollection<SegmentEntity>
{
	public void Load(XElement element, Version version)
	{
		Clear();
		
		foreach (var entityElement in element.Elements("entity"))
			Add(new SegmentEntity(entityElement));
	}
		
	public void Save(XElement element)
	{
		foreach (var entity in this)
		{
			var scriptsToString = entity.Scripts[0].ToString();

			if (scriptsToString.Contains("return new MobileEntity()"))
			{
				MessageBox.Show($"Make sure to add code for: {entity.Name}, otherwise compiliation errors will occur if you leave" +
				                $"{Environment.NewLine} return new MobileEntity(); in the code");

			}

			element.Add(entity.GetSerializingElement());
		}
	}
	
	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		base.OnCollectionChanged(args);
			
		if (args.NewItems != null)
		{
			foreach (var newItem in args.NewItems.OfType<SegmentEntity>())
				WeakReferenceMessenger.Default.Send(new SegmentEntityCreated(newItem));
		}
			
		if (args.OldItems != null)
		{
			foreach (var oldItem in args.OldItems.OfType<SegmentEntity>())
				WeakReferenceMessenger.Default.Send(new SegmentEntityDeleted(oldItem));
		}
		
		if (args.Action is NotifyCollectionChangedAction.Reset)
			WeakReferenceMessenger.Default.Send(new SegmentEntitiesReset());
			
		WeakReferenceMessenger.Default.Send(new SegmentEntitiesChanged(this));
	}
}
