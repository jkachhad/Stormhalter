using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.Editor;

public class SegmentBrushEntry : ObservableObject
{
	private IComponentProvider _component;
	private int _weight = 1;
	private float _chance = 1.0f;
	
	private SegmentBrush _brush;

	public IComponentProvider Component
	{
		get => _component;
		set => SetProperty(ref _component, value);
	}

	public int Weight
	{
		get => _weight;
		set => SetProperty(ref _weight, value);
	}
	
	public float Chance
	{
		get => _chance;
		set => SetProperty(ref _chance, value);
	}
	
	public SegmentBrushEntry(SegmentBrush brush, XElement element) : this(brush)
	{
		var weightAttribute = element.Attribute("weight");
		
		if (weightAttribute != null)
			_weight = (int)weightAttribute;
		
		var componentElement = element.Elements().FirstOrDefault();

		if (componentElement is null)
			return;
		
		var componentPalette = ServiceLocator.Current.GetInstance<ComponentPalette>();
		
		if (componentPalette is null)
			throw new ArgumentNullException(nameof(componentPalette));
		
		if (componentPalette.TryGetComponent(componentElement, out var namedProvider))
			_component = namedProvider;
	}

	public SegmentBrushEntry(SegmentBrush brush)
	{
		_brush = brush;
	}

	protected override void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);

		if (e.PropertyName != nameof(Weight))
			return;
		
		if (_brush != null)
			_brush.UpdateChances();
	}

	public XElement GetSerializingElement()
	{
		return new XElement("entry", new XAttribute("weight", Weight), 
			_component.GetReferencingElement());
	}
}
