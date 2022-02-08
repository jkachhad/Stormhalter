using CommonServiceLocator;
using Kesmai.WorldForge.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Models
{
	public class WallComponent : TerrainComponent
	{
		#region Static

		#endregion

		#region Fields

		private int _wall;
		private int _destroyed;
		private int _ruins;
		
		private bool _indestructible;

		#endregion

		#region Properties and Events

		/// <summary>
		/// Gets the static terrain.
		/// </summary>
		[Browsable(true)]
		public int Wall
		{
			get { return _wall; }
			set { _wall = value; }
		}
		
		[Browsable(true)]
		public int Destroyed
		{
			get { return _destroyed; }
			set { _destroyed = value; }
		}
		
		[Browsable(true)]
		public int Ruins
		{
			get { return _ruins; }
			set { _ruins = value; }
		}

		/// <summary>
		/// Gets a value indicating whether this component is indestructible.
		/// </summary>
		[Browsable(true)]
		public bool IsIndestructible
		{
			get { return _indestructible; }
			set { _indestructible = value; }
		}

		#endregion

		#region Constructors and Cleanup

		/// <summary>
		/// Initializes a new instance of the <see cref="WallComponent"/> class.
		/// </summary>
		public WallComponent(int wallId, int destroyedId, int ruinsId, bool indestructible = false)
		{
			_wall = wallId;
			_destroyed = destroyedId;
			_ruins = ruinsId;
			_indestructible = indestructible;
		}
		
		public WallComponent(XElement element) : base(element)
		{
			_wall = (int)element.Element("wall");
			_destroyed = (int)element.Element("destroyed");
			_ruins = (int)element.Element("ruins");

			var indestructibleElement = element.Element("indestructible");

			if (indestructibleElement != null)
				_indestructible = (bool)indestructibleElement;
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override IEnumerable<ComponentRender> GetTerrain()
		{
			var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();

			var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
			var visibility = presenter.Visibility;
			var showDestroyed = visibility.BreakWalls;

			Terrain terrain;

			if (showDestroyed && !IsIndestructible)
			{
				if (_ruins != 0 && terrainManager.TryGetValue(_ruins, out terrain))
					yield return new ComponentRender(terrain, Color);

				if (terrainManager.TryGetValue(_destroyed, out terrain))
					yield return new ComponentRender(terrain, Color);
			}
			else
			{
				if (terrainManager.TryGetValue(_wall, out terrain))
					yield return new ComponentRender(terrain, Color);
			}
		}
		
		public override XElement GetXElement()
		{
			var element = base.GetXElement();

			element.Add(new XElement("wall", _wall));
			element.Add(new XElement("destroyed", _destroyed));
			element.Add(new XElement("ruins", _ruins));

			if (_indestructible)
				element.Add(new XElement("indestructible", _indestructible));

			return element;
		}

		public override TerrainComponent Clone()
		{
			return new WallComponent(GetXElement());
		}
		
		#endregion
	}
}
