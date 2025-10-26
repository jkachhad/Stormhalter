using CommonServiceLocator;
using Kesmai.WorldForge.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Models;

public class FloorComponent : TerrainComponent
{
	#region Static

	#endregion

	#region Fields

	private int _ground;
	private int _movementCost;

	#endregion

	#region Properties and Events

	/// <summary>
	/// Gets or sets the ground id.
	/// </summary>
	[Browsable(true)]
	public int Ground
	{
		get => _ground;
		set => _ground = value;
	}

	/// <summary>
	/// Gets the movement cost for walking off this terrain.
	/// </summary>
	[Browsable(true)]
	public int MovementCost
	{
		get => _movementCost;
		set => _movementCost = value;
	}

	#endregion

	#region Constructors and Cleanup

	/// <summary>
	/// Initializes a new instance of the <see cref="FloorComponent"/> class.
	/// </summary>
	public FloorComponent(int groundId, int movementCost)
	{
		Ground = groundId;
		MovementCost = movementCost;
	}
		
	public FloorComponent(XElement element) : base(element)
	{
		_ground = (int)element.Element("ground");

		var movementCostElement = element.Element("movementCost");

		if (movementCostElement != null)
			_movementCost = (int)movementCostElement;
		else
			_movementCost = 1;
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public override IEnumerable<ComponentRender> GetRenders()
	{
		var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();

		if (terrainManager.TryGetValue(_ground, out Terrain terrain))
			yield return new ComponentRender(terrain, Color);
	}
		
	public override XElement GetSerializingElement()
	{
		var element = base.GetSerializingElement();

		element.Add(new XElement("ground", _ground));

		if (_movementCost != 1)
			element.Add(new XElement("movementCost", _movementCost));

		return element;
	}
	
	public override TerrainComponent Clone()
	{
		return new FloorComponent(GetSerializingElement());
	}

	#endregion
}