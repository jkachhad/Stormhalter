using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;

namespace Kesmai.Server.Game;

[WorldForgeComponent("LockersComponent")]
public class Lockers : Static, IHandleInteraction
{
	internal new class Cache : IComponentCache
	{
		private static readonly ConcurrentDictionary<int, Lockers> _cache = new ConcurrentDictionary<int, Lockers>();
	
		public TerrainComponent Get(XElement element)
		{
			var color = element.GetColor("color", Color.White);
			var staticId = element.GetInt("static", 0);

			return Get(color, staticId);
		}

		public Lockers Get(Color color, int lockerId)
		{
			return _cache.GetOrAdd(CalculateHash(color, lockerId), 
				_ => new Lockers(color, lockerId));
		}

		private static int CalculateHash(Color color, int lockerId)
		{
			return HashCode.Combine(color, lockerId);
		}
	}
	
	/// <summary>
	/// Gets an instance of <see cref="Lockers"/> that has been cached.
	/// </summary>
	public new static Lockers Construct(Color color, int lockerId)
	{
		if (TryGetCache(typeof(Lockers), out var cache) && cache is Cache componentCache)
			return componentCache.Get(color, lockerId);

		return new Lockers(color, lockerId);
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Lockers"/> class.
	/// </summary>
	private Lockers(Color color, int lockerId) : base(color, lockerId)
	{
	}
	
	/// <summary>
	/// Handles interaction from the specified entity.
	/// </summary>
	public bool HandleInteraction(SegmentTile parent, MobileEntity entity, ActionType action)
	{
		if (action != ActionType.Open)
			return false;

		var player = entity as PlayerEntity;
		var location = parent.Location;

		if (player != null)
		{
			var distance = entity.GetDistanceToMax(location);

			if (distance > 1)
			{
				entity.SendLocalizedMessage(Color.Red, 6300103); /* You are unable to look from here. */
			}
			else
			{
				entity.LookAt(parent);
				entity.QueueRoundTimer();
			}
		}

		return true;
	}
}