using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;
using Kesmai.WorldForge.Windows;

namespace Kesmai.WorldForge.Models;

public class CounterComponent : TerrainComponent
{
	#region Static

	#endregion

	#region Fields

	private int _counter;
	private Direction _accessDirection;

	#endregion

	#region Properties and Events

	/// <summary>
	/// Gets the counter terrain.
	/// </summary>
	[Browsable(true)]
	public int Counter
	{
		get { return _counter; }
		set { _counter = value; }
	}

	/// <summary>
	/// Gets or sets the access direction.
	/// </summary>
	[Browsable(true)]
	[ItemsSource(typeof(CardinalDirectionsItemsSource))]
	public Direction AccessDirection
	{
		get { return _accessDirection; }
		set { _accessDirection = value; }
	}

	#endregion

	#region Constructors and Cleanup

	/// <summary>
	/// Initializes a new instance of the <see cref="CounterComponent"/> class.
	/// </summary>
	public CounterComponent(int counterId, Direction accessDirection)
	{
		_counter = counterId;
		_accessDirection = accessDirection;
	}
		
	public CounterComponent(XElement element) : base(element)
	{
		_counter = (int)element.Element("counter");

		if (element.TryGetElement("direction", out var directionElement))
			_accessDirection = Direction.GetDirection((int)directionElement);
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public override IEnumerable<ComponentRender> GetRenders()
	{
		var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();

		if (terrainManager.TryGetValue(_counter, out Terrain terrain))
		{
			yield return new ComponentRender(terrain,
				((_accessDirection != null) ? Color : Microsoft.Xna.Framework.Color.Red));
		}
	}

	public override XElement GetXElement()
	{
		var element = base.GetXElement();

		element.Add(new XElement("counter", _counter));

		if (_accessDirection != null)
			element.Add(new XElement("direction", _accessDirection.Index));

		return element;
	}

	public override TerrainComponent Clone()
	{
		return new CounterComponent(GetXElement());
	}
		
	#endregion
}