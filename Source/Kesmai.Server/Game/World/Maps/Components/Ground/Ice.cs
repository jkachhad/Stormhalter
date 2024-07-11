using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("IceComponent")]
public class Ice : Floor, IHandlePathing
{
	internal new class Cache : IComponentCache
	{
		private static readonly Dictionary<int, Ice> _cache = new Dictionary<int, Ice>();
	
		public TerrainComponent Get(XElement element)
		{
			var color = element.GetColor("color", Color.White);
			var groundId = element.GetInt("ground", 0);
			var movementCost = element.GetInt("movementCost", 1);

			return Get(color, groundId, movementCost);
		}

		public Ice Get(Color color, int iceId, int movementCost)
		{
			var hash = CalculateHash(color, iceId, movementCost);

			if (!_cache.TryGetValue(hash, out var component))
				_cache.Add(hash, (component = new Ice(color, iceId, movementCost)));

			return component;
		}

		private static int CalculateHash(Color color, int iceId, int movementCost)
		{
			return HashCode.Combine(color, iceId, movementCost);
		}
	}
	
	/// <summary>
	/// Gets an instance of <see cref="Ice"/> that has been cached.
	/// </summary>
	public new static Ice Construct(Color color, int groundId, int movementCost)
	{
		if (TryGetCache(typeof(Ice), out var cache) && cache is Cache componentCache)
			return componentCache.Get(color, groundId, movementCost);

		return new Ice(color, groundId, movementCost);
	}
	
	/// <inheritdoc />
	public int PathingPriority { get; } = 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="Ice"/> class.
	/// </summary>
	private Ice(Color color, int iceId, int movementCost) : base(color, iceId, movementCost)
	{
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
		args.Result = PathingResult.Allowed;

		if (args.Entity is PlayerEntity player)
		{
			var willpower = player.Stats[EntityStat.Willpower].Value;

			if (Utility.Random(1, 20) > willpower)
			{
				args.Entity.SendLocalizedMessage(6100050); /* You slipped on the ice. */
				args.Result = PathingResult.Interrupted;
			}
		}
	}
}