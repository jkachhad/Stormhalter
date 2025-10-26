using CommonServiceLocator;
using Kesmai.WorldForge.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Models;

public class RuinsComponent : TerrainComponent
{
	#region Static

	#endregion

	#region Fields

	private int _ruins;

	#endregion

	#region Properties and Events

	/// <summary>
	/// Gets or sets the ruins id.
	/// </summary>
	[Browsable(true)]
	public int Ruins
	{
		get => _ruins;
		set => _ruins = value;
	}

	#endregion

	#region Constructors and Cleanup

	/// <summary>
	/// Initializes a new instance of the <see cref="RuinsComponent"/> class.
	/// </summary>
	public RuinsComponent(int ruinsId)
	{
		_ruins = ruinsId;
	}
		
	public RuinsComponent(XElement element) : base(element)
	{
		_ruins = (int)element.Element("ruins");
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public override IEnumerable<ComponentRender> GetRenders()
	{
		var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();

		if (terrainManager.TryGetValue(_ruins, out Terrain terrain))
			yield return new ComponentRender(terrain, Color);
	}

	public override XElement GetSerializingElement()
	{
		var element = base.GetSerializingElement();

		element.Add(new XElement("ruins", _ruins));

		return element;
	}

	public override TerrainComponent Clone()
	{
		return new RuinsComponent(GetSerializingElement());
	}

	#endregion
}