using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Models;

public class ItemTeleporter : HiddenTeleporterComponent
{
	[Browsable(true)]
	public string Type { get; set; }
		
	[Browsable(true)]
	public PathingResult OnFail { get; set; }
		
	public ItemTeleporter(int x, int y, int region) : base(x, y, region)
	{
	}

	public ItemTeleporter(XElement element) : base(element)
	{
		var typeElement = element.Element("type");
		var failElement = element.Element("fail");

		if (typeElement != null)
			Type = (string)typeElement;

		if (failElement != null)
			OnFail = (PathingResult)Enum.Parse(typeof(PathingResult), (string)failElement);
		else
			OnFail = PathingResult.Interrupted;
	}

	public override XElement GetXElement()
	{
		var element = base.GetXElement();

		if (!String.IsNullOrWhiteSpace(Type))
			element.Add(new XElement("type", Type));

		if (OnFail != PathingResult.Allowed)
			element.Add(new XElement("fail", OnFail));
			
		return element;
	}

	public override TerrainComponent Clone()
	{
		return new ItemTeleporter(GetXElement());
	}
}