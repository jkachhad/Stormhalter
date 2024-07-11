using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("ObstructionComponent")]
public class Obstruction : TerrainComponent, IHandleVision, IHandlePathing
{
	internal class Cache : IComponentCache
	{
		private static readonly Dictionary<int, Obstruction> _cache = new Dictionary<int, Obstruction>();
	
		public TerrainComponent Get(XElement element)
		{
			var color = element.GetColor("color", Color.White);
			var obstructionId = element.GetInt("obstruction", 0);
			var blockVision = element.GetBool("blockVision", false);

			return Get(color, obstructionId, blockVision);
		}

		public Obstruction Get(Color color, int obstructionId, bool blockVision)
		{
			var hash = CalculateHash(color, obstructionId, blockVision);

			if (!_cache.TryGetValue(hash, out var component))
				_cache.Add(hash, (component = new Obstruction(color, obstructionId, blockVision)));

			return component;
		}

		private static int CalculateHash(Color color, int obstructionId, bool blockVision)
		{
			return HashCode.Combine(color, obstructionId, blockVision);
		}
	}
	
	/// <summary>
	/// Gets an instance of <see cref="Obstruction"/> that has been cached.
	/// </summary>
	public static Obstruction Construct(Color color, int obstructionId, bool blockVision)
	{
		if (TryGetCache(typeof(Obstruction), out var cache) && cache is Cache componentCache)
			return componentCache.Get(color, obstructionId, blockVision);

		return new Obstruction(color, obstructionId, blockVision);
	}
	
	private readonly Terrain _obstruction;
	private readonly bool _blocksVision;
	
	/// <inheritdoc />
	public int PathingPriority { get; } = 1;

	/// <summary>
	/// Gets a value indicating whether this instance blocks line-of-sight.
	/// </summary>
	public bool BlocksVision => _blocksVision;

	/// <summary>
	/// Initializes a new instance of the <see cref="Obstruction"/> class.
	/// </summary>
	private Obstruction(Color color, int obstructionId, bool blocksVision) : base(color)
	{
		_obstruction = Terrain.Get(obstructionId, Color);
		_blocksVision = blocksVision;
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(SegmentTile parent, MobileEntity beholder)
	{
		if (_obstruction != null)
			yield return _obstruction;
	}

	/// <inheritdoc />
	public virtual bool AllowMovementPath(SegmentTile parent, MobileEntity entity = default(MobileEntity))
	{
		return false;
	}
		
	/// <inheritdoc />
	public virtual bool AllowSpellPath(SegmentTile parent, MobileEntity entity = default(MobileEntity), Spell spell = default(Spell))
	{
		return true;
	}

	/// <summary>
	/// Handles pathing requests over this terrain.
	/// </summary>
	public virtual void HandleMovementPath(SegmentTile parent, PathingRequestEventArgs args)
	{
		args.Result = PathingResult.Rejected;
	}
}