using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("SkyComponent")]
public class Sky : PassiveTeleporter, IHandleInteraction
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Sky"/> class.
	/// </summary>
	public Sky(XElement element) : base(element)
	{
	}
		
	/// <inheritdoc />
	public override int GetMovementCost(MobileEntity entity) => 128;

	/// <summary>
	/// Handles interaction from the specified entity.
	/// </summary>
	public bool HandleInteraction(SegmentTile parent, MobileEntity entity, ActionType action)
	{
		if (action != ActionType.Look)
			return false;

		var location = parent.Location;
		var distance = entity.GetDistanceToMax(location);

		if (distance > 1)
		{
			entity.SendLocalizedMessage(Color.Red, 6300103); /* You are unable to look from here. */
		}
		else
		{
			entity.LookAt(parent);
			entity.SendLocalizedMessage(Color.Yellow, 6300273, Math.Abs(_elevationDelta).ToString()); /* It looks like a {0} foot drop. */
		}

		return true;
	}

	public override void OnEnter(SegmentTile parent, MobileEntity entity, bool isTeleport)
	{
		if (!CanTeleport(parent, entity))
		{
			if (entity is PlayerEntity player && (player.LeftHand is BrightFeather feather))
				feather.Delete();
		}
		else
		{
			if (!isTeleport)
				Teleport(parent, entity);
		}
	}

	protected override bool CanTeleport(SegmentTile parent, MobileEntity entity)
	{
		if (entity is CreatureEntity creature)
			return !creature.CanFly;

		if (entity is PlayerEntity player && (player.LeftHand is BrightFeather feather))
			return false;

		return base.CanTeleport(parent, entity);
	}

	/// <summary>
	/// Called after the specified entity is teleported.
	/// </summary>
	protected override void OnAfterTeleport(SegmentTile parent, WorldEntity entity)
	{
		if (entity is MobileEntity mobile)
		{
			mobile.Fall(Math.Abs(_elevationDelta), parent, _destinationTile);

			if (Math.Abs(_elevationDelta) > 4500)
		   		mobile.Kill();
		}	
	}
}