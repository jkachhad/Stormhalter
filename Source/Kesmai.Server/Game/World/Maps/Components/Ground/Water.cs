using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("WaterComponent")]
public class Water : Floor, IHandlePathing
{
	internal new class Cache : IComponentCache
	{
		private static readonly Dictionary<int, Water> _cache = new Dictionary<int, Water>();
	
		public TerrainComponent Get(XElement element)
		{
			var color = element.GetColor("color", Color.White);
			var groundId = element.GetInt("ground", 0);
			var movementCost = element.GetInt("movementCost", 1);
			var depth = element.GetInt("depth", 3);

			return Get(color, groundId, movementCost, depth);
		}

		public Water Get(Color color, int waterId, int movementCost, int depth)
		{
			var hash = CalculateHash(color, waterId, movementCost, depth);

			if (!_cache.TryGetValue(hash, out var component))
				_cache.Add(hash, (component = new Water(color, waterId, movementCost, depth)));

			return component;
		}

		private static int CalculateHash(Color color, int waterId, int movementCost, int depth)
		{
			return HashCode.Combine(color, waterId, movementCost, depth);
		}
	}
	
	/// <summary>
	/// Gets an instance of <see cref="Water"/> that has been cached.
	/// </summary>
	public new static Water Construct(Color color, int groundId, int movementCost, int depth)
	{
		if (TryGetCache(typeof(Water), out var cache) && cache is Cache componentCache)
			return componentCache.Get(color, groundId, movementCost, depth);

		return new Water(color, groundId, movementCost, depth);
	}
	
	private readonly int _depth;
		
	/// <inheritdoc />
	public int PathingPriority { get; } = 0;
	
	/// <summary>
	/// Gets the depth.
	/// </summary>
	public int Depth => _depth;

	/// <summary>
	/// Gets a value indicating if an entity can be drowned.
	/// </summary>
	public bool CanDrown => (_depth >= 3);

	/// <summary>
	/// Initializes a new instance of the <see cref="Water"/> class.
	/// </summary>
	protected Water(Color color, int waterId, int movementCost, int depth) : base(color, waterId, movementCost)
	{
		_depth = depth;
	}
		
	/// <inheritdoc />
	public virtual bool AllowMovementPath(SegmentTile parent, MobileEntity entity = default(MobileEntity))
	{
		return true;
	}
		
	/// <inheritdoc />
	public virtual bool AllowSpellPath(SegmentTile parent, MobileEntity entity = default(MobileEntity), Spell spell = default(Spell))
	{
		return true;
	}

	/// <summary>
	/// Handles pathing requests over this terrain.
	/// </summary>
	public void HandleMovementPath(SegmentTile parent, PathingRequestEventArgs args)
	{
		var entity = args.Entity;

		if ((entity is CreatureEntity creature && creature.CanFly) || entity.HasStatus(typeof(WaterWalkingStatus)))
			args.Result = PathingResult.Allowed;
		else
			args.Result = PathingResult.Interrupted;
	}

	/// <inheritdoc />
	/// <remarks>
	/// Water that has depth less than 3 will have reduced costs. Otherwise,
	/// the cost is always 3, unless the player can water walk.
	/// </remarks>
	public override int GetMovementCost(MobileEntity entity)
	{
		var cost = base.GetMovementCost(entity);

		if (entity.HasStatus(typeof(WaterWalkingStatus)))
			return 1;
			
		return Math.Min(_depth, cost);
	}

	/// <summary>
	/// Called when a mobile entity steps on this component.
	/// </summary>
	public override void OnEnter(SegmentTile parent, MobileEntity entity, bool isTeleport)
	{
		if (!entity.IsAlive)
			return;
			
		/* Check for any open bottles in their hands. */
		var hands = entity.Hands;

		if (hands != null)
		{
			var bottles = hands.OfType<Bottle>().Where(b => b.IsOpen);

			foreach (var bottle in bottles)
			{
				bottle.Content = new ConsumableWater();
				bottle.Owner = entity;
			}
		}

		var silent = (entity is CreatureEntity creature && (creature.CanSwim || creature.CanFly));	

		if (!silent)
			parent.PlaySound(59, 3, 6);
			
		var drowning = true;

		if (entity is CreatureEntity swimmer)
			drowning = !swimmer.CanFly;
		
		if (entity.HasStatus(typeof(BreatheWaterStatus)))
			return;

		var region = parent.Region;

		if (CanDrown && drowning && !region.IsInactive)
			entity.QueueWaterTimer(_depth);
	}

	/// <summary>
	/// Called when a mobile entity steps off this component.
	/// </summary>
	public override void OnLeave(SegmentTile parent, MobileEntity entity, bool isTeleport)
	{
		entity.StopWaterTimer();
	}
}