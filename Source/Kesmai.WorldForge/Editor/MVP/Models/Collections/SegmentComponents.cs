﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Editor;

public class SegmentComponentCreated(SegmentComponent component) : ValueChangedMessage<SegmentComponent>(component);
public class SegmentComponentDeleted(SegmentComponent component) : ValueChangedMessage<SegmentComponent>(component);
public class SegmentComponentsChanged(SegmentComponents locations) : ValueChangedMessage<SegmentComponents>(locations);

public class SegmentComponents : ObservableCollection<SegmentComponent>
{
	public string Name => "(Components)";
	
	public void Load(XElement element, Version version)
	{
		Clear();

		foreach (var componentElement in element.Elements())
		{
			var componentTypeAttribute = componentElement.Attribute("type");
			
			if (componentTypeAttribute is null)
				throw new XmlException("Component element is missing type attribute.");
			
			var componentTypename = $"Kesmai.WorldForge.Models.{componentTypeAttribute.Value}";
			var componentType = Type.GetType(componentTypename);

			if (componentType is null)
				throw new XmlException($"Component type '{componentTypename}' not found.");

			var ctor = componentType.GetConstructor([typeof(XElement)]);

			if (ctor is null)
				throw new XmlException(
					$"Component type '{componentTypename}' is missing constructor with XElement parameter.");

			var terrainComponent = ctor.Invoke([componentElement]) as TerrainComponent;

			if (terrainComponent is null)
				throw new XmlException($"Component type '{componentTypename}' failed to instantiate.");

			var segmentComponent = new SegmentComponent()
			{
				Name = terrainComponent.Name,
				Element = new XElement(componentElement),
			};
			
			segmentComponent.UpdateComponent();
			
			Add(segmentComponent);
		}
	}
	
	public void Save(XElement element)
	{
		foreach (var segmentComponent in this)
			element.Add(segmentComponent.Element);
	}
	
	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		base.OnCollectionChanged(args);
		
		if (args.NewItems != null)
		{
			foreach (var newItem in args.NewItems.OfType<SegmentComponent>())
				WeakReferenceMessenger.Default.Send(new SegmentComponentCreated(newItem));
		}
			
		if (args.OldItems != null)
		{
			foreach (var oldItem in args.OldItems.OfType<SegmentComponent>())
				WeakReferenceMessenger.Default.Send(new SegmentComponentDeleted(oldItem));
		}

		WeakReferenceMessenger.Default.Send(new SegmentComponentsChanged(this));
	}
}