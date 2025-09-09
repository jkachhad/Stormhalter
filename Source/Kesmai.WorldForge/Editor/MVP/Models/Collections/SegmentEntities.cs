using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.Editor;

public class SegmentEntityCreated(Entity entity) : ValueChangedMessage<Entity>(entity);
public class SegmentEntityDeleted(Entity entity) : ValueChangedMessage<Entity>(entity);
public class SegmentEntitiesChanged(Entity entity) : ValueChangedMessage<Entity>(entity);

public class SegmentEntities : ObservableCollection<Entity>
{
	public void Load(XElement element, Version version)
	{
		foreach (var entityElement in element.Elements("entity"))
		{
			Add(new Entity(entityElement));
		}
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

			element.Add(entity.GetXElement());
		}
	}
	
	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		base.OnCollectionChanged(args);
			
		if (args.NewItems != null)
		{
			foreach (var newItem in args.NewItems.OfType<Entity>())
				WeakReferenceMessenger.Default.Send(new SegmentEntityCreated(newItem));
		}
			
		if (args.OldItems != null)
		{
			foreach (var oldItem in args.OldItems.OfType<Entity>())
				WeakReferenceMessenger.Default.Send(new SegmentEntityDeleted(oldItem));
		}
			
		foreach (var item in this)
			WeakReferenceMessenger.Default.Send(new SegmentEntitiesChanged(item));
	}
}