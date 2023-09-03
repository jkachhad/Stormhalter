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
	public override void Initialize()
	{
		var region = _parent.Region;
		var segment = _parent.Segment;

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
	protected virtual void Teleport(WorldEntity entity)
	{
		if (_destinationTile != null)
		{
			OnBeforeTeleport(entity);

			if (entity is MobileEntity mobile)
				mobile.Teleport(_destinationTile.Location);
			else if (entity is ItemEntity item)
				item.Move(_destinationTile.Location, true);
				
			OnAfterTeleport(entity);
		}
	}

	/// <summary>
	/// Called after the specified entity is teleported.
	/// </summary>
	protected virtual void OnAfterTeleport(WorldEntity entity)
	{
	}
		
	protected virtual bool CanTeleport(MobileEntity entity)
	{
		return false;
	}
		
	protected virtual bool CanTeleport(ItemEntity entity)
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
	public virtual void OnEnter(MobileEntity entity)
	{
		if (CanTeleport(entity))
			Teleport(entity);
	}
		
	/// <summary>
	/// Called when a mobile entity steps off this component.
	/// </summary>
	public virtual void OnLeave(MobileEntity entity)
	{
	}
		
	/// <inheritdoc />
	public virtual bool AllowMovementPath(MobileEntity entity = default(MobileEntity))
	{
		return true;
	}
		
	/// <inheritdoc />
	public virtual bool AllowSpellPath(MobileEntity entity = default(MobileEntity), Spell spell = default(Spell))
	{
		return true;
	}

	/// <summary>
	/// Handles pathing requests over this terrain.
	/// </summary>
	public virtual void HandleMovementPath(PathingRequestEventArgs args)
	{
		if (CanTeleport(args.Entity))
			args.Result = PathingResult.Teleport;
		else
			args.Result = PathingResult.Allowed;
	}
		
	public void OnItemAdded(ItemEntity item)
	{
		if (CanTeleport(item))
			Teleport(item);
	}
		
	public void OnItemRemoved(ItemEntity item)
	{
	}
}