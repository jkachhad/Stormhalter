using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Kesmai.Server.Game;

public abstract class TerrainComponent : IDisposable
{
	protected SegmentTile _parent;
		
	/// <summary>
	/// Gets or sets the map tile that contains this component.
	/// </summary>
	public SegmentTile Parent
	{
		get => _parent;
		set => _parent = value;
	}
		
	private Color _color;

	/// <summary>
	/// Gets or sets the component color.
	/// </summary>
	public virtual Color Color
	{
		get { return _color; }
	}

	public TerrainComponent()
	{
		_color = Color.White;
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="TerrainComponent"/> class.
	/// </summary>
	public TerrainComponent(XElement element)
	{
		if (element.TryGetElement("color", out var colorElement))
		{
			var colorR = (int)colorElement.Attribute("r");
			var colorG = (int)colorElement.Attribute("g");
			var colorB = (int)colorElement.Attribute("b");
			var colorA = (int)colorElement.Attribute("a");

			_color = Color.FromArgb(colorA, colorR, colorG, colorB);
		}
		else
		{
			_color = Color.White;
		}
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public virtual IEnumerable<Terrain> GetTerrain(MobileEntity beholder)
	{
		yield break;
	}

	/// <summary>
	/// Initializes this terrain.
	/// </summary>
	public virtual void Initialize()
	{
	}
		
	#region IDisposable
		
	protected bool _disposed = false;

	public void Dispose()
	{
		if (_disposed)
			return;
		
		OnDispose(true);
		
		_disposed = true;
	}
		
	protected virtual void OnDispose(bool disposing)
	{
		if (disposing)
			_parent = null;
	}
		
	#endregion
}