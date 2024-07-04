using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous.WorldForge;

namespace Kesmai.Server.Game;

[WorldForgeComponent("FloorComponent")]
public class Floor : TerrainComponent, IHandleInteraction, IHandleMovement, IHandleItems
{
	internal class Cache : IComponentCache
	{
		private static readonly Dictionary<int, Floor> _cache = new Dictionary<int, Floor>();
	
		public TerrainComponent Get(XElement element)
		{
			var color = element.GetColor("color", Color.White);
			var groundId = element.GetInt("ground", 0);
			var movementCost = element.GetInt("movementCost", 1);

			return Get(color, groundId, movementCost);
		}

		public Floor Get(Color color, int groundId, int movementCost)
		{
			var hash = CalculateHash(color, groundId, movementCost);

			if (!_cache.TryGetValue(hash, out var component))
				_cache.Add(hash, (component = new Floor(color, groundId, movementCost)));

			return component;
		}

		private static int CalculateHash(Color color, int groundId, int movementCost)
		{
			return HashCode.Combine(color, groundId, movementCost);
		}
	}
	
	/// <summary>
	/// Gets an instance of <see cref="Floor"/> that has been cached.
	/// </summary>
	public static Floor Construct(Color color, int groundId, int movementCost)
	{
		if (TryGetCache(typeof(Floor), out var cache) && cache is Cache componentCache)
			return componentCache.Get(color, groundId, movementCost);

		return new Floor(color, groundId, movementCost);
	}
	
	private readonly Terrain _ground;
	private readonly int _movementCost;
	
	protected Floor(Color color, int floorId, int movementCost) : base(color)
	{
		_ground = Terrain.Get(floorId, color);
		_movementCost = movementCost;
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(SegmentTile parent, MobileEntity beholder)
	{
		if (_ground != null)
			yield return _ground;
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

	/// <summary>
	/// Gets the movement cost for walking off this terrain.
	/// </summary>
	public virtual int GetMovementCost(MobileEntity entity) => _movementCost;

	/// <summary>
	/// Called when a mobile entity steps on this component.
	/// </summary>
	public virtual void OnEnter(SegmentTile parent, MobileEntity entity, bool isTeleport)
	{
		/* Play a sound effect when walking over corpses. */
		// check if the parent has any items to iterate over.
		if (!parent.HasItems)
			return;

		// get the count of corpses on the parent.
		var corpses = parent.Items.OfType<Corpse>().Count();
		
		if (corpses > 0 && entity is PlayerEntity)
		{
			if (Utility.Random(1, 20) <= corpses)
			{
				var sound = 3001;

				if (corpses > 3)
					sound = Utility.RandomBool() ? 3002 : 3003;

				entity.PlaySound(sound);
			}
		}
	}

	/// <summary>
	/// Called when a mobile entity steps off this component.
	/// </summary>
	public virtual void OnLeave(SegmentTile parent, MobileEntity entity, bool isTeleport)
	{
	}

	/// <inheritdoc />
	public void OnItemAdded(SegmentTile parent, ItemEntity item, bool isTeleport)
	{
	}

	/// <inheritdoc />
	public void OnItemRemoved(SegmentTile parent, ItemEntity item, bool isTeleport)
	{
	}
}