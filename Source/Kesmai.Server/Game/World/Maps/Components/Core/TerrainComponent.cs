using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;

namespace Kesmai.Server.Game;

public abstract class TerrainComponent
{
	private static readonly Dictionary<Type, IComponentCache> _componentCaches = new()
	{
		[typeof(Floor)] = new Floor.Cache(),
		[typeof(Wall)] = new Wall.Cache(),
		[typeof(Static)] = new Static.Cache(),
		[typeof(Ice)] = new Ice.Cache(),
		[typeof(Fire)] = new Fire.Cache(),
		[typeof(Water)] = new Water.Cache(),
		[typeof(PoisonedWater)] = new PoisonedWater.Cache(),
		[typeof(Ruins)] = new Ruins.Cache(),
		[typeof(Trash)] = new Trash.Cache(),
		[typeof(Lockers)] = new Lockers.Cache(),
		[typeof(Tree)] = new Tree.Cache(),
		[typeof(Altar)] = new Altar.Cache(),
		[typeof(Counter)] = new Counter.Cache(),
		[typeof(Obstruction)] = new Obstruction.Cache(),
		
		[typeof(Web)] = new Web.Cache(),
		[typeof(Darkness)] = new Darkness.Cache(),
	};

	public static bool TryGetCache(Type type, out IComponentCache cache)
		=> _componentCaches.TryGetValue(type, out cache);
	
	protected readonly Color _color;

	/// <summary>
	/// Gets the component color.
	/// </summary>
	public Color Color => _color;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="TerrainComponent"/> class.
	/// </summary>
	protected TerrainComponent(XElement element)
	{
		_color = element.GetColor("color", Color.White);
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="TerrainComponent"/> class.
	/// </summary>
	public TerrainComponent() : this(Color.White)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TerrainComponent"/> class.
	/// </summary>
	public TerrainComponent(Color color)
	{
		_color = color;
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public virtual IEnumerable<Terrain> GetTerrain(SegmentTile parent, MobileEntity beholder)
	{
		yield break;
	}

	/// <summary>
	/// Initializes this terrain.
	/// </summary>
	/// <param name="parent"></param>
	public virtual void Initialize(SegmentTile parent)
	{
	}
		
	#region IDisposable
		
	private bool _disposed = false;

	public void Dispose(SegmentTile parent)
	{
		if (_disposed)
			return;
		
		OnDispose(parent, true);
		
		_disposed = true;
	}
		
	protected virtual void OnDispose(SegmentTile parent, bool disposing)
	{
	}
		
	#endregion
}