using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Kesmai.WorldForge.UI.Documents;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kesmai.WorldForge.Editor;

public class SegmentSubregionChanged(SegmentSubregion subregion) : ValueChangedMessage<SegmentSubregion>(subregion);

public class SegmentSubregion : ObservableObject, ISegmentObject
{
	private static Color _pink = Color.FromNonPremultiplied(255, 192, 203, 75);
	private static Color _pinkBorder = Color.Pink;
		
	private static Color _green = Color.FromNonPremultiplied(152, 251, 152, 75);
	private static Color _greenBorder = Color.PaleGreen;
		
	private static Color _gray = Color.FromNonPremultiplied(176, 196, 222, 75);
	private static Color _grayBorder = Color.LightSteelBlue;
		
	private static Color _orange = Color.FromNonPremultiplied(255, 165, 0, 75);
	private static Color _orangeBorder = Color.Orange;
		
	private string _name;
		
	private int _region;

	private SubregionType _type;
		
	public string Name
	{
		get => _name;
		set
		{
			if (SetProperty(ref _name, value))
				WeakReferenceMessenger.Default.Send(new SegmentSubregionChanged(this));
		}
	}

	public void Present(ApplicationPresenter presenter)
	{
		var subregionViewModel = presenter.Documents
			.OfType<SubregionViewModel>().FirstOrDefault();

		if (subregionViewModel is null)
			presenter.Documents.Add(subregionViewModel = new SubregionViewModel(presenter.Segment));

		if (presenter.ActiveDocument != subregionViewModel)
			presenter.SetActiveDocument(subregionViewModel);

		presenter.SetActiveContent(this);
		subregionViewModel.SelectedSubregion = this;
	}

	public Color Color { get; set; }
	public Color Border { get; set; }
		
	public SubregionType Type
	{
		get => _type;
		set
		{
			SetProperty(ref _type, value);
			UpdateColor();
		}
	}
		
	public int Region
	{
		get => _region;
		set => SetProperty(ref _region, value);
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public IEnumerable<SubregionType> Types => Enum.GetValues(typeof(SubregionType)).Cast<SubregionType>();

	private ObservableCollection<SegmentBounds> _rectangles = new ObservableCollection<SegmentBounds>();

	public ObservableCollection<SegmentBounds> Rectangles
	{
		get => _rectangles;
		set => SetProperty(ref _rectangles, value);
	}

	public SegmentSubregion()
	{
		UpdateColor();
	}
		
	public SegmentSubregion(XElement element)
	{
		Name = (string)element.Attribute("name");

		if (Enum.TryParse((string)element.Attribute("type"), true, out SubregionType result))
			Type = result;

		Region = (int)element.Attribute("region");

		var rectanglesElement = element.Element("rectangles");

		if (rectanglesElement != null)
		{
			foreach (var rectangleElement in rectanglesElement.Elements("rectangle"))
			{
				Rectangles.Add(new SegmentBounds(
					(int)rectangleElement.Attribute("left"), 
					(int)rectangleElement.Attribute("top"), 
					(int)rectangleElement.Attribute("right"), 
					(int)rectangleElement.Attribute("bottom")));
			}
		}

		UpdateColor();
	}
		
	public XElement GetXElement()
	{
		var element = new XElement("subregion", 
			new XAttribute("name", _name), 
			new XAttribute("type", _type.ToString()),
			new XAttribute("region", _region));

		var rectanglesElement = new XElement("rectangles");
			
		foreach (var rectangle in Rectangles)
		{
			rectanglesElement.Add(new XElement("rectangle",
				new XAttribute("left", rectangle.Left),
				new XAttribute("top", rectangle.Top),
				new XAttribute("right", rectangle.Right),
				new XAttribute("bottom", rectangle.Bottom)));
		}

		element.Add(rectanglesElement);
			
		return element;
	}

	private void UpdateColor()
	{
		switch (Type)
		{
			case SubregionType.None:
			{
				Color = _gray;
				Border = _grayBorder;
				break;
			}
			case SubregionType.Town:
			{
				Color = _green;
				Border = _greenBorder;
				break;
			}
			case SubregionType.Lair:
			{
				Color = _orange;
				Border = _orangeBorder;
				break;
			}
		}
	}
}

public enum SubregionType
{
	None = 0,
		
	Town = 1,
	Lair = 2,
}