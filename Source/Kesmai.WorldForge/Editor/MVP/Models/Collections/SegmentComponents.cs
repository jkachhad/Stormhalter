using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Editor;

public class SegmentComponentCreated(TerrainComponent component) : ValueChangedMessage<TerrainComponent>(component);
public class SegmentComponentDeleted(TerrainComponent component) : ValueChangedMessage<TerrainComponent>(component);
public class SegmentComponentsChanged(SegmentComponents locations) : ValueChangedMessage<SegmentComponents>(locations);

public class SegmentComponents : ObservableCollection<TerrainComponent>
{
	public string Name => "(Components)";
	
	public void Load(XElement element, Version version)
	{
		Clear();

		foreach (var componentElement in element.Elements())
		{
			var componentTypename = $"Kesmai.WorldForge.Models.{componentElement.Name}";
			var componentType = Type.GetType(componentTypename);

			if (componentType is null)
				throw new XmlException($"Component type '{componentTypename}' not found.");

			var ctor = componentType.GetConstructor([typeof(XElement)]);

			if (ctor is null)
				throw new XmlException(
					$"Component type '{componentTypename}' is missing constructor with XElement parameter.");

			var component = ctor.Invoke([componentElement]) as TerrainComponent;

			if (component is null)
				throw new XmlException($"Component type '{componentTypename}' failed to instantiate.");

			Add(component);
		}
	}
	
	public void Save(XElement element)
	{
		foreach (var component in this)
			element.Add(component.GetXElement());
	}
	
	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		base.OnCollectionChanged(args);
		
		if (args.NewItems != null)
		{
			foreach (var newItem in args.NewItems.OfType<TerrainComponent>())
				WeakReferenceMessenger.Default.Send(new SegmentComponentCreated(newItem));
		}
			
		if (args.OldItems != null)
		{
			foreach (var oldItem in args.OldItems.OfType<TerrainComponent>())
				WeakReferenceMessenger.Default.Send(new SegmentComponentDeleted(oldItem));
		}

		WeakReferenceMessenger.Default.Send(new SegmentComponentsChanged(this));
	}
}