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
        
        var componentPalette = ServiceLocator.Current.GetInstance<ComponentPalette>();
        
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
