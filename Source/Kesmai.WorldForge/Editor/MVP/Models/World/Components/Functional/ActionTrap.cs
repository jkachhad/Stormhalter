using System;
using System.ComponentModel;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Models;

public class ActionTrap : TrapComponent
{
	private string _action;
	
	[Browsable(true)]
	public string Action
	{
		get => _action;
		set => _action = value;
	}
	
	public ActionTrap(XElement element) : base(element)
	{
		var damageElement = element.Element("action");

		if (damageElement != null)
			_action = (string)damageElement;
	}
	
	public override XElement GetSerializingElement()
	{
		var element = base.GetSerializingElement();

		if (!String.IsNullOrEmpty(_action))
			element.Add(new XElement("action", _action));
		
		return element;
	}
	
	public override TerrainComponent Clone()
	{
		return new ActionTrap(GetSerializingElement());
	}
}