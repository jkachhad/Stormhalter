using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

public abstract class Teleporter : TerrainComponent, IHandleMovement, IHandlePathing, IHandleItems
{
	protected Point2D _destination;

	protected SegmentTile _destinationTile;
	protected SegmentRegion _destinationRegion;

	protected int _elevationDelta;

	public Point2D Destination => _destination;
	public SegmentRegion DestinationRegion => _destinationRegion;
	
	/// <inheritdoc />
	public int PathingPriority { get; } = 0;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Teleporter"/> class.
	/// </summary>
	public Teleporter(XElement element) : base(element)
	{
		if (element.TryGetElement("destinationX", out var destinationXElement)
		    && element.TryGetElement("destinationY", out var destinationYElement)
		    && element.TryGetElement("destinationRegion", out var destinationRegionElement))
		{
			_destination = new Point2D((int)destinationXElement, (int)destinationYElement, (int)destinationRegionElement);
		}
	}

	/// <summary>
	/// Initializes this instance.
	/// </summary>
	/// <param name="parent"></param>
	public override void Initialize(SegmentTile parent)
	{
		var region = parent.Region;
		var segment = parent.Segment;

		_destinationRegion = segment.FindRegion(_destination.Region);

		if (_destinationRegion != null)
		{
			_destinationTile = _destinationRegion.GetTile(_destination.X, _destination.Y);

			var targetElevation = _destinationRegion.Elevation;
			var elevation = region.Elevation;

			_elevationDelta = targetElevation - elevation;
		}
	}
		
	/// <summary>
	/// Called before the specified entity is teleported.
	/// </summary>
	protected virtual void OnBeforeTeleport(WorldEntity entity)
	{
	}

	/// <summary>
	/// Teleports the specified entity.
	/// </summary>
	protected virtual void Teleport(SegmentTile parent, WorldEntity entity)
	{
		if (_destinationTile != null)
		{
			OnBeforeTeleport(entity);

			if (entity is MobileEntity mobile)
				mobile.Teleport(_destinationTile.Location);
			else if (entity is ItemEntity item)
				item.Move(_destinationTile.Location, true);
				
			OnAfterTeleport(parent, entity);
		}
	}

	/// <summary>
	/// Called after the specified entity is teleported.
	/// </summary>
	protected virtual void OnAfterTeleport(SegmentTile parent, WorldEntity entity)
	{
	}
		
	protected virtual bool CanTeleport(SegmentTile parent, MobileEntity entity)
	{
		return false;
	}
		
	protected virtual bool CanTeleport(SegmentTile parent, ItemEntity entity)
	{
		return false;
	}
		
	/// <summary>
	/// Gets the movement cost for walking off this terrain.
	/// </summary>
	public virtual int GetMovementCost(MobileEntity entity) => 1;
		
	/// <summary>
	/// Called when a mobile entity steps on this component.
	/// </summary>
	public virtual void OnEnter(SegmentTile parent, MobileEntity entity, bool isTeleport)
	{
		if (!isTeleport && CanTeleport(parent, entity))
			Teleport(parent, entity);
	}
		
	/// <summary>
	/// Called when a mobile entity steps off this component.
	/// </summary>
	public virtual void OnLeave(SegmentTile parent, MobileEntity entity, bool isTeleport)
	{
	}
		
	/// <inheritdoc />
	public virtual bool AllowMovementPath(SegmentTile parent, MobileEntity entity = default(MobileEntity))
	{
		return true;
	}
		
	/// <inheritdoc />
	public virtual bool AllowSpellPath(SegmentTile parent, MobileEntity entity = default(MobileEntity),
		Spell spell = default(Spell))
	{
		return true;
	}

	/// <summary>
	/// Handles pathing requests over this terrain.
	/// </summary>
	public virtual void HandleMovementPath(SegmentTile parent, PathingRequestEventArgs args)
	{
		if (CanTeleport(parent, args.Entity))
			args.Result = PathingResult.Teleport;
		else
			args.Result = PathingResult.Allowed;
	}
		
	public void OnItemAdded(SegmentTile parent, ItemEntity item, bool isTeleport)
	{
		if (!isTeleport && CanTeleport(parent, item))
			Teleport(parent, item);
	}
		
	public void OnItemRemoved(SegmentTile parent, ItemEntity item, bool isTeleport)
	{
	}
}