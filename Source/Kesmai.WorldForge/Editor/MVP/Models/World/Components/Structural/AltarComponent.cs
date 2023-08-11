using CommonServiceLocator;
using Kesmai.WorldForge.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Models;

public class AltarComponent : TerrainComponent
{
	#region Static

	#endregion

	#region Fields

	private int _altar;

	#endregion

	#region Properties and Events

	/// <summary>
	/// Gets the altar terrain.
	/// </summary>
	[Browsable(true)]
	public int Altar
	{
		get { return _altar; }
		set { _altar = value; }
	}

	#endregion

	#region Constructors and Cleanup

	/// <summary>
	/// Initializes a new instance of the <see cref="AltarComponent"/> class.
	/// </summary>
	public AltarComponent(int altarId)
	{
		_altar = altarId;
	}
		
	public AltarComponent(XElement element) : base(element)
	{
		_altar = (int)element.Element("altar");
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public override IEnumerable<ComponentRender> GetTerrain()
	{
		var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();

		if (terrainManager.TryGetValue(_altar, out Terrain terrain))
			yield return new ComponentRender(terrain, Color);
	}
		
	public override XElement GetXElement()
	{
		var element = base.GetXElement();

		element.Add(new XElement("altar", _altar));

		return element;
	}
		
	public override TerrainComponent Clone()
	{
		return new AltarComponent(GetXElement());
	}

	#endregion
}