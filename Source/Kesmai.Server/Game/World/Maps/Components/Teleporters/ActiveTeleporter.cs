using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Xml.Linq;

namespace Kesmai.Server.Game;

public abstract class ActiveTeleporter : Teleporter, IHandleInteraction
{
	protected Terrain _teleporter;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="ActiveTeleporter"/> class.
	/// </summary>
	public ActiveTeleporter(XElement element) : base(element)
	{
		if (element.TryGetElement("teleporterId", out var teleporterIdElement))
			_teleporter = Terrain.Get((int)teleporterIdElement, Color);
	}
		
	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(SegmentTile parent, MobileEntity beholder)
	{
		if (_teleporter != null)
			yield return _teleporter;
	}

	/// <summary>
	/// Handles interaction from the specified entity.
	/// </summary>
	public bool HandleInteraction(SegmentTile parent, MobileEntity entity, ActionType action)
	{
		if (action != ActionType.Look)
		{
			if (!CheckTeleport(parent, entity, action))
				return false;

			Teleport(parent, entity);

			entity.QueueMovementTimer();
			return true;
		}

		var location = parent.Location;
		var distance = entity.GetDistanceToMax(location);

		if (distance > 1)
			entity.SendLocalizedMessage(Color.Red, 6300103); /* You are unable to look from here. */
		else
			entity.LookAt(parent);
			
		return true;
	}
		
	/// <summary>
	/// Handles pathing requests over this terrain.
	/// </summary>
	public override void HandleMovementPath(SegmentTile parent, PathingRequestEventArgs args)
	{
		args.Result = PathingResult.Allowed;
	}

	/// <summary>
	/// Checks the action to perform a teleport.
	/// </summary>
	protected virtual bool CheckTeleport(SegmentTile parent, MobileEntity entity, ActionType action)
	{
		return false;
	}
}