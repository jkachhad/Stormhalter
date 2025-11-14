using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Models;

public class WaterComponent : FloorComponent
{
	#region Static

	#endregion

	#region Fields

	private int _depth;

	#endregion

	#region Properties and Events

	/// <summary>
	/// Gets or sets a depth value.
	/// </summary>
	[Browsable(true)]
	public int Depth
	{
		get { return _depth; }
		set { _depth = value; }
	}

	#endregion

	#region Constructors and Cleanup

	/// <summary>
	/// Initializes a new instance of the <see cref="WaterComponent"/> class.
	/// </summary>
	public WaterComponent(int waterId, int depth) : base(waterId, Math.Min(depth, 3))
	{
		_depth = depth;
	}
		
	public WaterComponent(XElement element) : base(element)
	{
		var depthElement = element.Element("depth");

		if (depthElement != null)
			_depth = (int)depthElement;
		else
			_depth = 3;
	}

	#endregion

	#region Methods

	public override XElement GetSerializingElement()
	{
		var element = base.GetSerializingElement();

		if (_depth != 3)
			element.Add(new XElement("depth", _depth));

		return element;
	}
	
	public override TerrainComponent Clone()
	{
		return new WaterComponent(GetSerializingElement());
	}

	#endregion
}