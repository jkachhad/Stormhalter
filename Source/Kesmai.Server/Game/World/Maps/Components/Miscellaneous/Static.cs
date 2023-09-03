using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;

namespace Kesmai.Server.Game;

[WorldForgeComponent("StaticComponent")]
public class Static : TerrainComponent
{
	private Terrain _static;

	public Static(int staticId)
	{
		_static = new Terrain(staticId);
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Static"/> class.
	/// </summary>
	public Static(XElement element) : base(element)
	{
		if (element.TryGetElement("static", out var staticElement))
			_static = Terrain.Get((int)staticElement, Color);
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(MobileEntity beholder)
	{
		if (_static != null)
			yield return _static;
	}
}