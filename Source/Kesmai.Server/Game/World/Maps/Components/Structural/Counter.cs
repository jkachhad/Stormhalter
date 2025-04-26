using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("CounterComponent")]
public class Counter : TerrainComponent, IHandlePathing, IHandleInteraction
{
	internal class Cache : IComponentCache
	{
		private static readonly ConcurrentDictionary<int, Counter> _cache = new ConcurrentDictionary<int, Counter>();
	
		public TerrainComponent Get(XElement element)
		{
			var color = element.GetColor("color", Color.White);
			var counterId = element.GetInt("counter", 0);
			var direction = element.GetDirection("direction", Direction.None);

			return Get(color, counterId, direction);
		}

		public Counter Get(Color color, int counterId, Direction direction)
		{
			return _cache.GetOrAdd(CalculateHash(color, counterId, direction), 
				_ => new Counter(color, counterId, direction));
		}

		private static int CalculateHash(Color color, int counterId, Direction direction)
		{
			return HashCode.Combine(color, counterId, direction);
		}
	}
	
	/// <summary>
	/// Gets an instance of <see cref="Counter"/> that has been cached.
	/// </summary>
	public static Counter Construct(Color color, int counterId, Direction direction)
	{
		if (TryGetCache(typeof(Counter), out var cache) && cache is Cache componentCache)
			return componentCache.Get(color, counterId, direction);

		return new Counter(color, counterId, direction);
	}
	
	private readonly Terrain _counter;
	private readonly Direction _accessDirection;
	
	/// <inheritdoc />
	public int PathingPriority { get; } = 0;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Counter"/> class.
	/// </summary>
	private Counter(Color color, int counterId, Direction direction) : base(color)
	{
		_counter = Terrain.Get(counterId, color);
		_accessDirection = direction;
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(SegmentTile parent, MobileEntity beholder)
	{
		if (_counter != null)
			yield return _counter;
	}

	/// <summary>
	/// Handles interaction from the specified entity.
	/// </summary>
	public bool HandleInteraction(SegmentTile parent, MobileEntity entity, ActionType action)
	{
		if (action != ActionType.Look)
			return false;

		if (!IsAccessibleFrom(parent, entity.Location))
			entity.SendLocalizedMessage(Color.Red, 6300103); /* You are unable to look from here. */
		else
			entity.LookAt(parent);

		return true;
	}

	/// <inheritdoc />
	public virtual bool AllowMovementPath(SegmentTile parent, MobileEntity entity = default(MobileEntity))
	{
		return false;
	}
		
	/// <inheritdoc />
	public virtual bool AllowSpellPath(SegmentTile parent, MobileEntity entity = default(MobileEntity), Spell spell = default(Spell))
	{
		return false;
	}

	/// <summary>
	/// Handles pathing requests over this terrain.
	/// </summary>
	public virtual void HandleMovementPath(SegmentTile parent, PathingRequestEventArgs args)
	{
		if (!AllowMovementPath(parent, args.Entity))
			args.Result = PathingResult.Daze;
		else
			args.Result = PathingResult.Allowed;
	}

	/// <summary>
	/// Checks if the counter is accessible from the specified location.
	/// </summary>
	public bool IsAccessibleFrom(SegmentTile parent, Point2D sourceLocation)
	{
		var location = parent.Location;
			
		var distance = sourceLocation.GetDistanceToMax(location);
		var direction = Direction.GetDirection(location, sourceLocation);

		if (distance > 1 || direction != _accessDirection)
			return false;

		return true;
	}
}