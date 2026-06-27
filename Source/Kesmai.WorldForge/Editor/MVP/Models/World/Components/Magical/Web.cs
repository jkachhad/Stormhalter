using System.ComponentModel;
using System.IO;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Models;

public class Web : StaticComponent
{
	private bool _allowDispel;
		
	[Browsable(true)]
	public bool AllowDispel
	{
		get => _allowDispel;
		set => _allowDispel = value;
	}
		
	public Web(bool allowDispel) : base(131)
	{
		_allowDispel = allowDispel;
	}

	public Web(XElement element) : base(EnsureStaticElement(element))
	{
		var allowDispel = element.Element("allowDispel");

		if (allowDispel != null)
			_allowDispel = (bool)allowDispel;
	}

	private static XElement EnsureStaticElement(XElement element)
	{
		if (element.Element("static") != null)
			return element;

		var copy = new XElement(element);
		copy.Add(new XElement("static", 131));
		return copy;
	}
		
	public override XElement GetSerializingElement()
	{
		var element = base.GetSerializingElement();

		if (_allowDispel)
			element.Add(new XElement("allowDispel", _allowDispel));

		return element;
	}

	public override TerrainComponent Clone()
	{
		return new Web(GetSerializingElement());
	}
}
