using System;
using System.Drawing;
using System.Collections.Generic;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("AltarComponent")]
public class Altar : TerrainComponent, IHandlePathing, IHandleInteraction
{
	internal class Cache : IComponentCache
	{
		private static readonly Dictionary<int, Altar> _cache = new Dictionary<int, Altar>();
	
		public TerrainComponent Get(XElement element)
		{
			var color = element.GetColor("color", Color.White);
			var alterId = element.GetInt("altar", 0);

			return Get(color, alterId);
		}

		public Altar Get(Color color, int alterId)
		{
			var hash = CalculateHash(color, alterId);

			if (!_cache.TryGetValue(hash, out var component))
				_cache.Add(hash, (component = new Altar(color, alterId)));

			return component;
		}

		private static int CalculateHash(Color color, int alterId)
		{
			return HashCode.Combine(color, alterId);
		}
	}
	
	/// <summary>
	/// Gets an instance of <see cref="Altar"/> that has been cached.
	/// </summary>
	public static Altar Construct(Color color, int alterId)
	{
		if (TryGetCache(typeof(Altar), out var cache) && cache is Cache componentCache)
			return componentCache.Get(color, alterId);

		return new Altar(color, alterId);
	}
	
	private readonly Terrain _altar;

	/// <inheritdoc />
	public int PathingPriority { get; } = 0;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Altar"/> class.
	/// </summary>
	private Altar(Color color, int alterId) : base(color)
	{
		_altar = Terrain.Get(alterId, color);
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(SegmentTile parent, MobileEntity beholder)
	{
		if (_altar != null)
			yield return _altar;
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
}