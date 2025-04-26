using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("RuinsComponent")]
public class Ruins : TerrainComponent, IHandleMovement, IHandlePathing
{
	internal class Cache : IComponentCache
	{
		private static readonly ConcurrentDictionary<int, Ruins> _cache = new ConcurrentDictionary<int, Ruins>();
	
		public TerrainComponent Get(XElement element)
		{
			var color = element.GetColor("color", Color.White);
			var ruinsId = element.GetInt("ruins", 0);

			return Get(color, ruinsId);
		}

		public Ruins Get(Color color, int ruinsId)
		{
			return _cache.GetOrAdd(CalculateHash(color, ruinsId), 
				_ => new Ruins(color, ruinsId));
		}

		private static int CalculateHash(Color color, int ruinsId)
		{
			return HashCode.Combine(color, ruinsId);
		}
	}
	
	/// <summary>
	/// Gets an instance of <see cref="Ruins"/> that has been cached.
	/// </summary>
	public static Ruins Construct(Color color, int ruinsId)
	{
		if (TryGetCache(typeof(Ruins), out var cache) && cache is Cache componentCache)
			return componentCache.Get(color, ruinsId);

		return new Ruins(color, ruinsId);
	}
	
	private readonly Terrain _ruins;
	
	/// <inheritdoc />
	public int PathingPriority { get; } = 0;

	private Ruins(Color color, int ruinsId) : base(color)
	{
		_ruins = Terrain.Get(ruinsId, color);
	}
	
	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(SegmentTile parent, MobileEntity beholder)
	{
		if (_ruins != null)
			yield return _ruins;
	}
		
	/// <summary>
	/// Determines whether the specified entity can path over this component.
	/// </summary>
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
	/// Handles movement over this component.
	/// </summary>
	public void HandleMovementPath(SegmentTile parent, PathingRequestEventArgs args)
	{
		args.Result = PathingResult.Interrupted;
	}

	/// <summary>
	/// Gets the movement cost for walking off this terrain.
	/// </summary>
	public virtual int GetMovementCost(MobileEntity entity) => 3;
		
	/// <summary>
	/// Called when a mobile entity steps on this component.
	/// </summary>
	public virtual void OnEnter(SegmentTile parent, MobileEntity entity, bool isTeleport)
	{
	}

	/// <summary>
	/// Called when a mobile entity steps off this component.
	/// </summary>
	public virtual void OnLeave(SegmentTile parent, MobileEntity entity, bool isTeleport)
	{
	}
}