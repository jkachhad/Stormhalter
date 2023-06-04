using System;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using CommonServiceLocator;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.Editor;

public class SegmentLocation : ObservableObject, ICloneable
{
	private string _name;

	private int _x;
	private int _y;
	private int _region;

	public string Name
	{
		get => _name;
		set => SetProperty(ref _name, value);
	}

	public int X
	{
		get => _x;
		set => SetProperty(ref _x, value);
	}

	public int Y
	{
		get => _y;
		set => SetProperty(ref _y, value);
	}

	public int Region
	{
		get => _region;
		set => SetProperty(ref _region, value);
	}
		
	public bool IsReserved { get; set; }

	public SegmentLocation()
	{
	}
		
	public SegmentLocation(XElement element)
	{
		Name = (string)element.Attribute("name");
			
		X = (int)element.Attribute("x");
		Y = (int)element.Attribute("y");

		Region = (int)element.Attribute("region");
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