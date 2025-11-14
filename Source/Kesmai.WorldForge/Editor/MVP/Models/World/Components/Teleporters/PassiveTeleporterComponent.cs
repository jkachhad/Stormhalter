using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;
using CommonServiceLocator;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Models;

public class PassiveTeleporterComponent : TeleportComponent
{
	private int _teleporterId;

	[Browsable(true)]
	public int TeleporterId
	{
		get => _teleporterId;
		set => _teleporterId = value;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PassiveTeleporterComponent"/> class.
	/// </summary>
	public PassiveTeleporterComponent(int teleporterId, int x, int y, int region) : base(x, y, region)
	{
		_teleporterId = teleporterId;
	}
		
	public PassiveTeleporterComponent(XElement element) : base(element)
	{
		_teleporterId = (int)element.Element("teleporterId");
	}
		
	/// <inheritdoc />
	public override IEnumerable<ComponentRender> GetRenders()
	{
		var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();

		if (terrainManager.TryGetValue(_teleporterId, out Terrain terrain))
			yield return new ComponentRender(terrain, (IsValid() ? Color : Microsoft.Xna.Framework.Color.Red));
	}
		
	public override XElement GetSerializingElement()
	{
		var element = base.GetSerializingElement();

		element.Add(new XElement("teleporterId", _teleporterId));

		return element;
	}
	
	public override TerrainComponent Clone()
	{
		return new PassiveTeleporterComponent(GetSerializingElement());
	}
}