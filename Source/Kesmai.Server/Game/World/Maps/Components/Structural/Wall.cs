using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("WallComponent")]
public class Wall : TerrainComponent, IHandleInteraction, IHandleVision, IHandlePathing, IDestructable
{
	internal class Cache : IComponentCache
	{
		private static readonly Dictionary<int, Wall> _cache = new Dictionary<int, Wall>();
	
		public TerrainComponent Get(XElement element)
		{
			var color = element.GetColor("color", Color.White);
			var wallId = element.GetInt("wall", 0);
			var destroyedId = element.GetInt("destroyed", 0);
			var ruinsId = element.GetInt("ruins", 0);
			var indestructible = element.GetBool("indestructible", false);

			return Get(color, wallId, destroyedId, ruinsId, indestructible, false);
		}

		public Wall Get(Color color, int wallId, int destroyedId, int ruinsId, bool indestructible, bool destroyed)
		{
			var hash = CalculateHash(color, wallId, destroyedId, ruinsId, indestructible, destroyed);

			if (!_cache.TryGetValue(hash, out var component))
				_cache.Add(hash, (component = new Wall(color, wallId, destroyedId, ruinsId, indestructible, destroyed)));

			return component;
		}

		private static int CalculateHash(Color color, int wallId, int destroyedId, int ruinsId, bool indestructible, bool destroyed)
		{
			return HashCode.Combine(color, wallId, destroyedId, ruinsId, indestructible, destroyed);
		}
	}
	
	/// <summary>
	/// Gets an instance of <see cref="Wall"/> that has been cached.
	/// </summary>
	public static Wall Construct(Color color, int wallId, int destroyedId, int ruinsId, bool indestructible = true, bool destroyed = false)
	{
		if (TryGetCache(typeof(Wall), out var cache) && cache is Cache componentCache)
			return componentCache.Get(color, wallId, destroyedId, ruinsId, indestructible, destroyed);

		return new Wall(color, wallId, destroyedId, ruinsId, indestructible, destroyed);
	}
	
	private readonly Terrain _wall;
	private readonly Terrain _destroyed;
	private readonly Terrain _ruins;

	private readonly bool _isIndestructible;
	private readonly bool _isDestroyed;

	/// <inheritdoc />
	public int PathingPriority { get; } = 0;

	/// <summary>
	/// Gets a value indicating whether this instance is indestructible.
	/// </summary>
	public bool IsIndestructible => _isIndestructible;

	/// <summary>
	/// Gets a value indicating whether this terrain is destroyed.
	/// </summary>
	public bool IsDestroyed => _isDestroyed;
	
	/// <summary>
	/// Gets a value indicating whether this instance blocks line-of-sight.
	/// </summary>
	public bool BlocksVision => !IsDestroyed;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Wall"/> class.
	/// </summary>
	private Wall(Color color, int wallId, int destroyedId, int ruinsId, bool indestructible = false, bool destroyed = false) : base(color)
	{
		_wall = Terrain.Get(wallId, color);
		_destroyed = Terrain.Get(destroyedId, color);
		_ruins = Terrain.Get(ruinsId, color);
		
		_isIndestructible = indestructible;
		_isDestroyed = destroyed;
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(SegmentTile parent, MobileEntity beholder)
	{
		if (_isDestroyed)
		{
			if (_destroyed != null)
				yield return _destroyed;
		}
		else
		{
			if (_wall != null)
				yield return _wall;
		}
	}

	/// <summary>
	/// Handles interaction from the specified entity.
	/// </summary>
	public bool HandleInteraction(SegmentTile parent, MobileEntity entity, ActionType action)
	{
		if (_isDestroyed || action != ActionType.Search)
			return false;

		entity.QueueRoundTimer();
		return true;
	}

	/// <summary>
	/// Determines whether the specified entity can path over this component.
	/// </summary>
	public virtual bool AllowMovementPath(SegmentTile parent, MobileEntity entity = default(MobileEntity))
	{
		return _isDestroyed;
	}

	/// <inheritdoc />
	public virtual bool AllowSpellPath(SegmentTile parent, MobileEntity entity = default(MobileEntity), Spell spell = default(Spell))
	{
		if (_isDestroyed || spell is CreatePortalSpell)
			return true;
			
		return false;
	}
		
	/// <summary>
	/// Handles pathing requests over this terrain.
	/// </summary>
	public void HandleMovementPath(SegmentTile parent, PathingRequestEventArgs args)
	{
		/* If this terrain is destroyed, there should exist a ground component
		 * to provide a pathing result and movement cost. */
		if (!_isDestroyed)
			args.Result = PathingResult.Daze;
	}

	public void Destroy(SegmentTile parent)
	{
		if (_isDestroyed || _isIndestructible)
			return;
		
		var currentState = this;
		var updatedState = Construct(_color, _wall.ID, _destroyed.ID, _ruins.ID, _isIndestructible, true);

		if (currentState != updatedState)
		{
			parent.Remove(currentState);
			parent.Add(updatedState);
		}

		if (_ruins != null)
			parent.Add(Ruins.Construct(_color, _ruins.ID));
			
		parent.Delta(TileDelta.Terrain);
	}
}