using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;

namespace Kesmai.Server.Game;

[WorldForgeComponent("PassiveTeleporterComponent")]
public class PassiveTeleporter : Teleporter
{
	private Terrain _teleporter;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="PassiveTeleporter"/> class.
	/// </summary>
	public PassiveTeleporter(XElement element) : base(element)
	{
		if (element.TryGetElement("teleporterId", out var teleporterIdElement))
			_teleporter = Terrain.Get((int)teleporterIdElement, Color);
	}
		
	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(MobileEntity beholder)
	{
		if (_teleporter != null)
			yield return _teleporter;
	}
		
	protected override bool CanTeleport(MobileEntity entity)
	{
		return true;
	}
		
	protected override bool CanTeleport(ItemEntity entity)
	{
		return true;
	}
}