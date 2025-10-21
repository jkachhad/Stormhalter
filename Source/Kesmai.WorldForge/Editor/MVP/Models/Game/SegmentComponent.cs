using System;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;
using Kesmai.WorldForge.UI.Documents;

namespace Kesmai.WorldForge;

public interface IComponentProvider
{
	string Name { get; }
	
	TerrainComponent Component { get; }

	void AddComponent(SegmentTile segmentTile);
	void RemoveComponent(SegmentTile segmentTile);
}

public class SegmentComponentChanged(SegmentComponent segmentComponent) : ValueChangedMessage<SegmentComponent>(segmentComponent);

public class SegmentComponent : ObservableObject, ISegmentObject, IComponentProvider
{
	private string _name;
	private XElement _element;
	private TerrainComponent _component;
	
	public string Name
	{
		get => _name;
		set
		{
			if (SetProperty(ref _name, value))
				WeakReferenceMessenger.Default.Send(new SegmentComponentChanged(this));
		}
	}
	
	public XElement Element
	{
		get => _element;
		set 
		{
			if (SetProperty(ref _element, value))
			{
				WeakReferenceMessenger.Default.Send(new SegmentComponentChanged(this));
			}
		}
	}

	public TerrainComponent Component => _component;

	public SegmentComponent()
	{
	}

	public SegmentComponent(XElement element)
	{
		// get the name attribute
		var nameAttribute = element.Attribute("name");
		
		_name = nameAttribute?.Value ?? "Unnamed Component";
		_element = element;
		
		UpdateComponent();
	}

	public IComponentProvider Clone()
	{
		var clone = new SegmentComponent()
		{
			Name = $"Copy of {_name}",
			Element = _element,
		};
		
		return clone;
	}

	protected override void OnPropertyChanged(PropertyChangedEventArgs args)
	{
		base.OnPropertyChanged(args);
		
		// Handle changes to the Element property
		if (args.PropertyName != nameof(Element))
			return;
		
		UpdateComponent();
	}

	public void UpdateComponent()
	{
		var componentTypeAttribute = _element.Attribute("type");

		if (componentTypeAttribute is null)
			throw new XmlException("Component element is missing type attribute.");

		var componentTypename = $"Kesmai.WorldForge.Models.{componentTypeAttribute.Value}";
		var componentType = Type.GetType(componentTypename);

		if (componentType is null)
			throw new XmlException($"Component type '{componentTypename}' not found.");

		var ctor = componentType.GetConstructor([typeof(XElement)]);

		if (ctor is null)
			throw new XmlException($"Component type '{componentTypename}' is missing constructor with XElement parameter.");

		var component = ctor.Invoke([_element]) as TerrainComponent;

		if (component is null)
			throw new XmlException($"Component type '{componentTypename}' failed to instantiate.");
		
		_component = component;
	}

	public void Present(ApplicationPresenter presenter)
	{
		var componentViewModel = presenter.Documents.OfType<ComponentViewModel>().FirstOrDefault();

		if (componentViewModel is null)
			presenter.Documents.Add(componentViewModel = new ComponentViewModel());

		if (presenter.ActiveDocument != componentViewModel)
			presenter.SetActiveDocument(componentViewModel);

		presenter.SetActiveContent(this);
	}

	public void Copy(Segment target)
	{
		if (Clone() is SegmentComponent segmentComponent)
			target.Components.Add(segmentComponent);
	}
	
	public void AddComponent(SegmentTile segmentTile)
	{
		// add this specific provider.
		segmentTile.Components.Add(this);
	}

	public void RemoveComponent(SegmentTile segmentTile)
	{
		// remove this specific component.
		segmentTile.Components.Remove(this);
	}
}
