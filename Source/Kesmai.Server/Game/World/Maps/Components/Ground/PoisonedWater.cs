using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("PoisonedWaterComponent")]
public class PoisonedWater : Water
{
	internal new class Cache : IComponentCache
	{
		private static readonly Dictionary<int, PoisonedWater> _cache = new Dictionary<int, PoisonedWater>();
	
		public TerrainComponent Get(XElement element)
		{
			var color = element.GetColor("color", Color.White);
			var groundId = element.GetInt("ground", 0);
			var movementCost = element.GetInt("movementCost", 1);
			var depth = element.GetInt("depth", 3);
			var potency = element.GetInt("potency", 3);

			return Get(color, groundId, movementCost, depth, potency);
		}

		public PoisonedWater Get(Color color, int waterId, int movementCost, int depth, int potency)
		{
			var hash = CalculateHash(color, waterId, movementCost, depth, potency);

			if (!_cache.TryGetValue(hash, out var component))
				_cache.Add(hash, (component = new PoisonedWater(color, waterId, movementCost, depth, potency)));

			return component;
		}

		private static int CalculateHash(Color color, int waterId, int movementCost, int depth, int potency)
		{
			return HashCode.Combine(color, waterId, movementCost, depth, potency);
		}
	}
	
	/// <summary>
	/// Gets an instance of <see cref="PoisonedWater"/> that has been cached.
	/// </summary>
	public new static PoisonedWater Construct(Color color, int groundId, int movementCost, int depth, int potency)
	{
		if (TryGetCache(typeof(PoisonedWater), out var cache) && cache is Cache componentCache)
			return componentCache.Get(color, groundId, movementCost, depth, potency);

		return new PoisonedWater(color, groundId, movementCost, depth, potency);
	}
	
	private readonly int _potency;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="PoisonedWater"/> class.
	/// </summary>
	private PoisonedWater(Color color, int waterId, int movementCost, int depth, int potency) : base(color, waterId, movementCost, depth)
	{
		_potency = potency;
	}

	/// <inheritdoc />
	public override void OnEnter(SegmentTile parent, MobileEntity entity, bool isTeleport)
	{
		base.OnEnter(parent, entity, isTeleport);
			
		/* Poison the entity. */
		entity.Poison(null, new Poison(TimeSpan.Zero, _potency));
			
		/* Check for any open bottles in their hands. */
		var hands = entity.Hands;

		if (hands is null)
			return;
			
		var bottles = hands.OfType<Bottle>().Where(b => b.IsOpen);

		foreach (var bottle in bottles)
		{
			bottle.Content = new ConsumablePoison(_potency);
			bottle.Owner = entity;
		}
	}
}