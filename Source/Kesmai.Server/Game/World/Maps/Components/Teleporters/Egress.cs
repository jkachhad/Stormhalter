using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous.WorldForge;

namespace Kesmai.Server.Game;

[WorldForgeComponent("EgressComponent")]
public class Egress : TerrainComponent, IHandleInteraction, IHandleMovement
{
	private Terrain _portal;

	private Point2D _destination;
	private int _segmentIndex;

	private SegmentTile _destinationTile;
	private SegmentRegion _destinationRegion;
	private Segment _destinationSegment;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Egress"/> class.
	/// </summary>
	public Egress(XElement element) : base(element)
	{
		if (element.TryGetElement("egress", out var egressElement))
			_portal = Terrain.Get((int)egressElement, Color);

		if (element.TryGetElement("destinationSegment", out var destinationElement))
			_segmentIndex = (int)destinationElement;
	}

	/// <summary>
	/// Initializes this instance.
	/// </summary>
	/// <param name="parent"></param>
	public override void Initialize(SegmentTile parent)
	{
		var facet = parent.Facet;

		_destinationSegment = facet.GetSegmentByIndex(_segmentIndex);

		if (_destinationSegment != null)
		{
			_destination = _destinationSegment.GetEntranceLocation();
			_destinationRegion = _destinationSegment.FindRegion(_destination.Region);

			if (_destinationRegion != null)
				_destinationTile = _destinationRegion.GetTile(_destination.X, _destination.Y);
		}
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(SegmentTile parent, MobileEntity beholder)
	{
		if (_portal != null)
			yield return _portal;
	}

	/// <summary>
	/// Called before the specified entity is teleported.
	/// </summary>
	public void OnBeforeTeleport(SegmentTile parent, MobileEntity entity)
	{
		parent.PlaySound(67, 3, 6);
	}

	/// <summary>
	/// Called after the specified entity is teleported.
	/// </summary>
	public void OnAfterTeleport(SegmentTile parent, MobileEntity entity)
	{
	}

	/// <summary>
	/// Handles interaction from the specified entity.
	/// </summary>
	public bool HandleInteraction(SegmentTile parent, MobileEntity entity, ActionType action)
	{
		if (action != ActionType.Use && action != ActionType.Look)
			return false;

		if (action == ActionType.Use)
		{
			if (entity.IsCarryingCorpse(out var corpse))
			{
				if (corpse.Owner is PlayerEntity player)
					entity.SendLocalizedMessage(6300356, player.Name); /* The anguished spirit of {0} prevents you from travelling. */
				else
					entity.SendLocalizedMessage(6300355); /* The anguished spirit prevents you from travelling. */

				return false;
			}

			Teleport(parent, entity);
		}
		else if (action == ActionType.Look)
		{
			var location = parent.Location;

			var distance = entity.GetDistanceToMax(location);

			if (distance > 1)
				entity.SendLocalizedMessage(Color.Red, 6300103); /* You are unable to look from here. */
			else
				entity.LookAt(parent);
		}

		entity.QueueMovementTimer();
		return true;
	}

	/// <summary>
	/// Teleports the specified entity.
	/// </summary>
	protected virtual void Teleport(SegmentTile parent, MobileEntity entity)
	{
		if (_destinationSegment != null && _destinationTile != null)
		{
			OnBeforeTeleport(parent, entity);
			entity.Teleport(_destinationTile.Location, _destinationSegment);
			OnAfterTeleport(parent, entity);
		}
	}
		
	/// <summary>
	/// Gets the movement cost for walking off this terrain.
	/// </summary>
	public virtual int GetMovementCost(MobileEntity entity) => 1;
		
	public void OnEnter(SegmentTile parent, MobileEntity entity, bool isTeleport)
	{
	}

	public void OnLeave(SegmentTile parent, MobileEntity entity, bool isTeleport)
	{
	}
}