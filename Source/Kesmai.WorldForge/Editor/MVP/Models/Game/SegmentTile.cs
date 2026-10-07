using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Editor;

public class SegmentTile : ObservableObject, IEnumerable<IComponentProvider>
{
    private int _x;
    private int _y;

    private List<TerrainRender> _renders;

    /// <summary>
    /// Incremented whenever any tile's terrain is rebuilt or a tile is placed in a region,
    /// so screens can tell when segment-wide caches derived from tiles are stale.
    /// </summary>
    public static int Revision { get; private set; }

    internal static void IncrementRevision ( ) => Revision++;

    private static ComponentPalette _componentPalette;
    private static RegionFilters _regionFilters;

    // application-lifetime services registered at startup. cached because tiles look them up
    // for every tile loaded and every terrain update; null results are retried on the next call.
    private static ComponentPalette CachedComponentPalette => _componentPalette ??= ServiceLocator.Current.GetInstance<ComponentPalette>();
    private static RegionFilters CachedRegionFilters => _regionFilters ??= ServiceLocator.Current.GetInstance<RegionFilters>();

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
        _x = (int)element.Attribute("x");
        _y = (int)element.Attribute("y");

        Providers = new ObservableCollection<IComponentProvider>();
        
        var componentPalette = CachedComponentPalette;
        
        if (componentPalette is null)
            throw new InvalidOperationException("ComponentPalette service is not available.");

        foreach (var componentElement in element.Elements())
        {
            if (componentPalette.TryGetComponent(componentElement, out var component))
                Providers.Add(component);
        }

        UpdateTerrain();
    }

    /// <summary>
    /// Gets an XML element that describes this instance.
    /// </summary>
    public XElement GetSerializingElement()
    {
        var tileElement = new XElement("tile");

        foreach (var provider in Providers)
            tileElement.Add(provider.GetReferencingElement());

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
        var providers = Providers;

        for ( var index = 0; index < providers.Count; index++ )
        {
            foreach ( var provider in providers[index].GetComponents ( ) )
            {
                if ( selector.IsValid ( provider ) )
                {
                    foreach ( var render in provider.GetRenders() )
                        yield return selector.TransformRender ( this, provider, render );
                }
            }
        }
    }

    public void AddComponent ( IComponentProvider provider )
    {
        if ( provider == null )
            return;
        
        provider.AddComponent(this.Providers);
        
        UpdateTerrain ( );
    }

    public void RemoveComponent ( IComponentProvider component )
    {
        if ( component != null && Providers.Contains ( component ) )
            component.RemoveComponent(this.Providers);

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
        var regionFilters = CachedRegionFilters;

        if (regionFilters != null)
            UpdateTerrain(regionFilters.SelectedFilter);
    }

    public void UpdateTerrain ( TerrainSelector selector )
    {
        var componentRenders = GetRenderableTerrain ( selector );
        var renders = new List<TerrainRender> ( );

        foreach ( var render in componentRenders )
        {
            var terrain = render.Terrain;

            for ( var index = 0; index < terrain.Count; index++ )
                renders.Add ( new TerrainRender ( terrain[index], render.Color ) );
        }

        // stable insertion sort by layer order, in place. layers with equal order must keep their
        // draw order (as OrderBy did, unlike List.Sort), and a tile only holds a few layers.
        for ( var index = 1; index < renders.Count; index++ )
        {
            var render = renders[index];
            var order = render.Layer.Order;
            var position = index - 1;

            while ( position >= 0 && renders[position].Layer.Order > order )
            {
                renders[position + 1] = renders[position];
                position--;
            }

            renders[position + 1] = render;
        }

        _renders = renders;

        IncrementRevision ( );
    }
}
