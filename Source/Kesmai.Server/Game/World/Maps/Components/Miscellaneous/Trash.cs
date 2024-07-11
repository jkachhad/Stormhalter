using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous.WorldForge;

namespace Kesmai.Server.Game;

[WorldForgeComponent("TrashComponent")]
public class Trash : Static, IHandleInteraction, IHandleItems
{
	internal new class Cache : IComponentCache
	{
		private static readonly Dictionary<int, Trash> _cache = new Dictionary<int, Trash>();
	
		public TerrainComponent Get(XElement element)
		{
			var color = element.GetColor("color", Color.White);
			var staticId = element.GetInt("static", 0);

			return Get(color, staticId);
		}

		public Trash Get(Color color, int trashId)
		{
			var hash = CalculateHash(color, trashId);

			if (!_cache.TryGetValue(hash, out var component))
				_cache.Add(hash, (component = new Trash(color, trashId)));

			return component;
		}

		private static int CalculateHash(Color color, int trashId)
		{
			return HashCode.Combine(color, trashId);
		}
	}
	
	/// <summary>
	/// Gets an instance of <see cref="Trash"/> that has been cached.
	/// </summary>
	public new static Trash Construct(Color color, int trashId)
	{
		if (TryGetCache(typeof(Trash), out var cache) && cache is Cache componentCache)
			return componentCache.Get(color, trashId);

		return new Trash(color, trashId);
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Trash"/> class.
	/// </summary>
	private Trash(Color color, int trashId = 88) : base(color, trashId)
	{
	}

	/// <summary>
	/// Called when an item is added.
	/// </summary>
	public void OnItemAdded(SegmentTile parent, ItemEntity item, bool isTeleport)
	{
		if (item is Corpse)
			return;

		item.Delete();
	}

	/// <summary>
	/// Called when an item is removed.
	/// </summary>
	public void OnItemRemoved(SegmentTile parent, ItemEntity item, bool isTeleport)
	{
	}

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
			entity.SendLocalizedMessage(Color.Red, 6300103); /* You are unable to look from here. */
		else
			entity.LookAt(parent);

		return true;
	}
}