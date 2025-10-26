using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using CommonServiceLocator;

namespace Kesmai.WorldForge.Models;

public class StaticComponent : TerrainComponent
{

    private int _static;

    #region Properties and Events

    /// <summary>
    /// Gets the static terrain.
    /// </summary>
    [Browsable( true )]
    public int Static
    {
        get => _static;
        set => SetProperty( ref _static, value );
    }

    #endregion

    #region Constructors and Cleanup

    // To help with performance, we cache the preview image of the static component. This is mostly used to help with prefab but it can be utilized for any component.
    public WriteableBitmap? CachedPreview { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StaticComponent"/> class.
    /// </summary>
    public StaticComponent( int staticId )
    {
        _static = staticId;
    }

    public StaticComponent( XElement element ) : base( element )
    {
        _static = (int?)element.Element( "static" )
            ?? throw new ArgumentException( "Missing <static> element", nameof( element ) );
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override IEnumerable<ComponentRender> GetRenders()
    {
        var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();

        if( terrainManager.TryGetValue( _static, out Terrain terrain ) )
            yield return new ComponentRender( terrain, Color );
    }

    public override XElement GetSerializingElement()
    {
        var element = base.GetSerializingElement();

        element.Add( new XElement( "static", _static ) );

        return element;
    }

    public override TerrainComponent Clone() => new StaticComponent( GetSerializingElement() );


    #endregion
}