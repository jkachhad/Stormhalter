using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;

namespace Kesmai.Server.Game;

[WorldForgeComponent("StaticComponent")]
public class Static : TerrainComponent
{
	internal class Cache : IComponentCache
	{
		private static readonly ConcurrentDictionary<int, Static> _cache = new ConcurrentDictionary<int, Static>();
	
		public TerrainComponent Get(XElement element)
		{
			var color = element.GetColor("color", Color.White);
			var staticId = element.GetInt("static", 0);

			return Get(color, staticId);
		}

		public Static Get(Color color, int staticId)
		{
			return _cache.GetOrAdd(CalculateHash(color, staticId), 
				_ => new Static(color, staticId));
		}

		private static int CalculateHash(Color color, int staticId)
		{
			return HashCode.Combine(color, staticId);
		}
	}
	
	/// <summary>
	/// Gets an instance of <see cref="Static"/> that has been cached.
	/// </summary>
	public static Static Construct(Color color, int staticId)
	{
		if (TryGetCache(typeof(Static), out var cache) && cache is Cache componentCache)
			return componentCache.Get(color, staticId);

		return new Static(color, staticId);
	}
	
	private readonly Terrain _static;

	/// <summary>
	/// Initializes a new instance of the <see cref="Static"/> class.
	/// </summary>
	protected Static(Color color, int staticId) : base(color)
	{
		_static = Terrain.Get(staticId, color);
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(SegmentTile parent, MobileEntity beholder)
	{
		if (_static != null)
			yield return _static;
	}
}