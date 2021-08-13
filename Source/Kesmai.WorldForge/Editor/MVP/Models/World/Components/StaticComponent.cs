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
	public class StaticComponent : TerrainComponent
    {
		#region Static

		#endregion

		#region Fields

	    private int _static;

		#endregion

		#region Properties and Events

	    /// <summary>
	    /// Gets the static terrain.
	    /// </summary>
	    [Browsable(true)]
	    public int Static
	    {
		    get => _static;
		    set => SetProperty(ref _static, value);
	    }

		#endregion

		#region Constructors and Cleanup

		/// <summary>
		/// Initializes a new instance of the <see cref="StaticComponent"/> class.
		/// </summary>
		public StaticComponent(int staticId)
	    {
		    _static = staticId;
	    }
		
		public StaticComponent(XElement element) : base(element)
		{
			_static = (int)element.Element("static");
		}

		#endregion

		#region Methods

	    /// <inheritdoc />
	    public override IEnumerable<ComponentRender> GetTerrain()
		{
			var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();

			if (terrainManager.TryGetValue(_static, out Terrain terrain))
				yield return new ComponentRender(terrain, Color);
		}
	    
	    public override XElement GetXElement()
	    {
		    var element = base.GetXElement();

		    element.Add(new XElement("static", _static));

		    return element;
	    }
	    
	    public override TerrainComponent Clone()
	    {
		    return new StaticComponent(GetXElement());
	    }

		#endregion
	}
}