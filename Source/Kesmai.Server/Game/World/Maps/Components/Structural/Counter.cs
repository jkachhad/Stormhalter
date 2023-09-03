using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("CounterComponent")]
public class Counter : TerrainComponent, IHandlePathing, IHandleInteraction
{
	private Terrain _counter;
	private Direction _accessDirection;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Counter"/> class.
	/// </summary>
	public Counter(XElement element) : base(element)
	{
		if (element.TryGetElement("counter", out var counterElement))
			_counter = Terrain.Get((int)counterElement, Color);
			
		if (element.TryGetElement("direction", out var directionElement))
			_accessDirection = Direction.GetDirection((int)directionElement);
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(MobileEntity beholder)
	{
		if (_counter != null)
			yield return _counter;
	}

	/// <summary>
	/// Handles interaction from the specified entity.
	/// </summary>
	public bool HandleInteraction(MobileEntity entity, ActionType action)
	{
		if (action != ActionType.Look)
			return false;

		if (!IsAccessibleFrom(entity.Location))
			entity.SendLocalizedMessage(Color.Red, 6300103); /* You are unable to look from here. */
		else
			entity.LookAt(_parent);

		return true;
	}

	/// <inheritdoc />
	public virtual bool AllowMovementPath(MobileEntity entity = default(MobileEntity))
	{
		return false;
	}
		
	/// <inheritdoc />
	public virtual bool AllowSpellPath(MobileEntity entity = default(MobileEntity), Spell spell = default(Spell))
	{
		return false;
	}

	/// <summary>
	/// Handles pathing requests over this terrain.
	/// </summary>
	public virtual void HandleMovementPath(PathingRequestEventArgs args)
	{
		if (!AllowMovementPath(args.Entity))
			args.Result = PathingResult.Daze;
		else
			args.Result = PathingResult.Allowed;
	}

	/// <summary>
	/// Checks if the counter is accessible from the specified location.
	/// </summary>
	public bool IsAccessibleFrom(Point2D sourceLocation)
	{
		var location = _parent.Location;
			
		var distance = sourceLocation.GetDistanceToMax(location);
		var direction = Direction.GetDirection(location, sourceLocation);

		if (distance > 1 || direction != _accessDirection)
			return false;

		return true;
	}
}