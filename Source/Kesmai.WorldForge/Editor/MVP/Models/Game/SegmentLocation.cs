using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Kesmai.WorldForge.UI.Documents;

namespace Kesmai.WorldForge.Editor;

public class SegmentLocationChanged(SegmentLocation location) : ValueChangedMessage<SegmentLocation>(location);

public class SegmentLocation : ObservableObject, ICloneable, ISegmentObject
{
	private string _name;

	private int _x;
	private int _y;
	private int _region;

	[Category("Identity")]
	public string Name
	{
		get => _name;
		set
		{
			// only trigger a change on name to update project references.
			if (SetProperty(ref _name, value))
				WeakReferenceMessenger.Default.Send(new SegmentLocationChanged(this));
		}
	}

	[Category("Coordinates")]
	public int X
	{
		get => _x;
		set => SetProperty(ref _x, value);
	}

	[Category("Coordinates")]
	public int Y
	{
		get => _y;
		set => SetProperty(ref _y, value);
	}

	[Category("Coordinates")]
	public int Region
	{
		get => _region;
		set => SetProperty(ref _region, value);
	}
	
	[ReadOnly(true)]
	public bool IsReserved { get; set; }

	public SegmentLocation()
	{
	}
		
	public SegmentLocation(XElement element)
	{
		_name = (string)element.Attribute("name");
			
		_x = (int)element.Attribute("x");
		_y = (int)element.Attribute("y");

		_region = (int)element.Attribute("region");
	}
	
	public void Present(ApplicationPresenter presenter)
	{
		var locationViewModel = presenter.Documents.OfType<LocationsViewModel>().FirstOrDefault();

		if (locationViewModel is null)
			presenter.Documents.Add(locationViewModel = new LocationsViewModel());

		if (presenter.ActiveDocument != locationViewModel)
			presenter.SetActiveDocument(locationViewModel);
					
		presenter.SetActiveContent(this);
	}
	
	public void Copy(Segment target)
	{
		if (Clone() is SegmentLocation clonedLocation)
			target.Locations.Add(clonedLocation);
	}
	
	public XElement GetXElement()
	{
		return new XElement("location", 
			new XAttribute("name", _name), 
			new XAttribute("x", _x), 
			new XAttribute("y", _y),
			new XAttribute("region", _region));
	}

	public object Clone()
	{
		return new SegmentLocation(GetXElement())
		{
			Name = $"Copy of {_name}"
		};
	}
}