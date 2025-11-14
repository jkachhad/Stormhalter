using System.IO;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Models;

public class SkyComponent : PassiveTeleporterComponent
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SkyComponent"/> class.
	/// </summary>
	public SkyComponent(int skyId, int x, int y, int region) : base(skyId, x, y, region)
	{
	}
		
	public SkyComponent(XElement element) : base(element)
	{
	}

	public override TerrainComponent Clone()
	{
		return new SkyComponent(GetSerializingElement());
	}
}