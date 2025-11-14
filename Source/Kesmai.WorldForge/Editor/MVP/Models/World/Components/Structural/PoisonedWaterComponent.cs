using System.ComponentModel;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Models;

public class PoisonedWaterComponent : WaterComponent
{
	private int _potency;
		
	/// <summary>
	/// Gets or sets a potency value.
	/// </summary>
	[Browsable(true)]
	public int Potency
	{
		get => _potency;
		set => _potency = value;
	}
		
	public PoisonedWaterComponent(int waterId, int depth) : base(waterId, depth)
	{
	}

	public PoisonedWaterComponent(XElement element) : base(element)
	{
		if (element.TryGetElement("potency", out var potencyElement))
			_potency = (int)potencyElement;
		else
			_potency = 3;
	}
		
	public override XElement GetSerializingElement()
	{
		var element = base.GetSerializingElement();

		if (_potency != 3)
			element.Add(new XElement("potency", _potency));

		return element;
	}
		
	public override TerrainComponent Clone()
	{
		return new PoisonedWaterComponent(GetSerializingElement());
	}
}