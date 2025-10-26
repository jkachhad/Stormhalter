using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;
using CommonServiceLocator;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Models;

public class ShaftComponent : ActiveTeleporter
{
	private int _slipChance;
		
	/// <summary>
	/// Gets or sets the chance to slip while interacting with this shaft.
	/// </summary>
	[Browsable(true)]
	public int SlipChance
	{
		get => _slipChance;
		set => _slipChance = value;
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="ShaftComponent"/> class.
	/// </summary>
	public ShaftComponent(int shaftId, int x, int y, int region, int slipChance) : base(shaftId, x, y, region)
	{
		_slipChance = slipChance;
	}
		
	public ShaftComponent(XElement element) : base(element)
	{
		var slipChanceElement = element.Element("slipChance");
			
		if (slipChanceElement != null)
			_slipChance = (int)slipChanceElement;
	}

	public override XElement GetSerializingElement()
	{
		var element = base.GetSerializingElement();

		if (_slipChance > 0)
			element.Add(new XElement("slipChance", _slipChance));
			
		return element;
	}
		
	public override TerrainComponent Clone()
	{
		return new ShaftComponent(GetSerializingElement());
	}
}