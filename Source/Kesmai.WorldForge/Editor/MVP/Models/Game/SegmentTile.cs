using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Editor.MVP.Models.World.Components;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Editor;

public class SegmentTile : ObservableObject, IEnumerable<IComponentProvider>
{
    private int _x;
    private int _y;

    private List<TerrainRender> _renders;

    public int X => _x;
    public int Y => _y;

    public ObservableCollection<IComponentProvider> Providers { get; set; }

    public List<TerrainRender> Renders => _renders;

    public SegmentTile ( int x, int y )
    {
        _x = x;
        _y = y;

        Providers = new ObservableCollection<IComponentProvider> ( );
    }

    public SegmentTile ( XElement element )
    {
        _x = (int) element.Attribute ( "x" );
        _y = (int) element.Attribute ( "y" );

        Providers = new ObservableCollection<IComponentProvider> ( );

        foreach ( var componentElement in element.Elements ( "component" ) )
        {
            var component = InstantiateComponent ( componentElement );

            if ( component != null )
                Providers.Add ( component );
        }

        UpdateTerrain ( );
    }

    /// <summary>
    /// Gets an XML element that describes this instance.
    /// </summary>
    public XElement GetXElement ( )
    {
        var tileElement = new XElement ( "tile" );

        foreach ( var component in Providers.SelectMany(c => c.GetComponents()) )
            tileElement.Add ( component.GetXElement ( ) );

        return tileElement;
    }

    IEnumerator IEnumerable.GetEnumerator ( )
    {
        return GetEnumerator ( );
    }

    /// <inheritdoc />
    public IEnumerator<IComponentProvider> GetEnumerator ( )
    {
        return Providers.GetEnumerator ( );
    }

    public IEnumerable<ComponentRender> GetRenderableTerrain ( TerrainSelector selector )
    {
        foreach ( var provider in Providers.SelectMany(c => c.GetComponents()) )
        {
            if ( selector.IsValid ( provider ) )
            {
                foreach ( var render in provider.GetRenders() )
                    yield return selector.TransformRender ( this, provider, render );
            }
        }
    }

    public void AddComponent ( IComponentProvider provider )
    {
        if ( provider == null )
            return;

        if ( provider is TilePrefabComponent prefabComponent )
        {
            foreach ( var subComponent in prefabComponent.Components )
            {
                Providers.Add ( subComponent );
            }
        }
        else
        {
            provider.AddComponent(this);
        }

        UpdateTerrain ( );
    }

    public void RemoveComponent ( IComponentProvider component )
    {
        if ( component != null && Providers.Contains ( component ) )
            component.RemoveComponent(this);

        UpdateTerrain ( );
    }

    public void InsertComponent ( int index, IComponentProvider component )
    {
        if ( component != null )
            Providers.Insert ( index, component );

        UpdateTerrain ( );
    }

    public void ReplaceComponent ( int index, IComponentProvider overwrite )
    {
        if ( overwrite == null )
            return;

        Providers.RemoveAt ( index );
        Providers.Insert ( index, overwrite );

        UpdateTerrain ( );
    }

    public void ReplaceComponent ( TerrainComponent original, IComponentProvider overwrite )
    {
        if ( overwrite == null || original == null || !Providers.Contains ( original ) )
            return;

        var index = Providers.IndexOf ( original );

        Providers.Remove ( original );
        Providers.Insert ( index, overwrite );

        UpdateTerrain ( );
    }

    private TerrainComponent InstantiateComponent ( XElement element )
    {
        var type = typeof ( StaticComponent );
        var typeAttribute = element.Attribute ( "type" );

        if ( typeAttribute != null )
            type = Type.GetType ( String.Format ( "Kesmai.WorldForge.Models.{0}", (string) typeAttribute ) );

        if ( type == null )
            throw new ArgumentNullException ( "Unable to find the specified component type" );

        var ctor = type.GetConstructor ( new Type[] { typeof ( XElement ) } );
        var component = ctor.Invoke ( new object[] { element } ) as TerrainComponent;

        return component;
    }

    public IEnumerable<T> GetComponents<T> ( )
    {
        return Providers.OfType<T> ( ).ToArray<T> ( );
    }

    public IEnumerable<T> GetComponents<T> ( Func<T, bool> predicate )
    {
        return Providers.OfType<T> ( ).Where<T> ( predicate ).ToArray<T> ( );
    }

    public void UpdateTerrain ( )
    {
        var regionFilters = ServiceLocator.Current.GetInstance<RegionFilters>();

        if (regionFilters != null)
            UpdateTerrain(regionFilters.SelectedFilter);
    }

    public void UpdateTerrain ( TerrainSelector selector )
    {
        var componentRenders = GetRenderableTerrain ( selector );
        var renders = new List<TerrainRender> ( );

        foreach ( var render in componentRenders )
            renders.AddRange ( render.Terrain.Select ( layer => new TerrainRender ( layer, render.Color ) ) );

        _renders = renders.OrderBy ( render => render.Layer.Order ).ToList ( );
    }
}