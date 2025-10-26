using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Editor.MVP.Models.World.Components
{
    public class TilePrefabComponent : TerrainComponent
    {
        public List<TerrainComponent> Components { get; } = new();
        public SegmentTile PreviewTile { get; set; } = new SegmentTile( 0, 0 );

        public TilePrefabComponent() : base() { }
        private static readonly Dictionary<string, Type> _componentTypeCache =
    typeof( TerrainComponent ).Assembly
        .GetTypes()
        .Where( t => typeof( TerrainComponent ).IsAssignableFrom( t ) )
        .ToDictionary( t => t.Name, t => t );

        public TilePrefabComponent( XElement element ) : base( element )
        {
            Name = (string?)element.Attribute( "name" ) ?? "Unnamed Prefab";
            System.Diagnostics.Debug.WriteLine( $"[TilePrefab] Loading prefab '{Name}'" );


            foreach( var child in element.Elements() )
            {
                System.Diagnostics.Debug.WriteLine( $"[TilePrefab] Inspecting child <{child.Name}>" );

                // Must be a <component type="XYZ"> element
                if( child.Name != "component" || child.Attribute( "type" ) is not XAttribute typeAttr )
                {
                    System.Diagnostics.Debug.WriteLine( $"[TilePrefab] Skipped invalid child: {child}" );
                    continue;
                }

                string typeName = typeAttr.Value;
                string fullTypeName = $"Kesmai.WorldForge.Models.{typeName}";
                if( !_componentTypeCache.TryGetValue( typeName, out var componentType ) )
                {
                    // Optionally fallback to full name if needed
                    componentType = typeof( TerrainComponent ).Assembly.GetType( fullTypeName );
                }


                // Fallback: try to find by type name anywhere in the assembly
                if( componentType == null )
                {
                    componentType = typeof( TerrainComponent ).Assembly
                        .GetTypes()
                        .FirstOrDefault( t => t.Name == typeName && typeof( TerrainComponent ).IsAssignableFrom( t ) );
                }

                if( componentType == null )
                {
                    System.Diagnostics.Debug.WriteLine( $"[TilePrefab] Failed to resolve component type '{typeName}'" );
                    continue;
                }

                try
                {
                    if( Activator.CreateInstance( componentType, child ) is TerrainComponent component )
                    {
                        Components.Add( component );
                        System.Diagnostics.Debug.WriteLine( $"[TilePrefab] Added subcomponent: {component.GetType().Name}" );
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine( $"[TilePrefab] Activator returned null or non-TerrainComponent: {typeName}" );
                    }
                }
                catch( Exception ex )
                {
                    System.Diagnostics.Debug.WriteLine( $"[TilePrefab] Exception while creating '{typeName}': {ex.Message}" );
                }
            }

            System.Diagnostics.Debug.WriteLine( $"[TilePrefab] Total subcomponents loaded: {Components.Count}" );
        }


        public override TerrainComponent Clone()
        {
            var clone = new TilePrefabComponent
            {
                Name = this.Name,
                Color = this.Color,
                Comment = this.Comment,
            };

            // foreach( var component in Components )
            //     clone.Components.Add( component.Component );

            return clone;
        }

        public override XElement GetSerializingElement()
        {
            var element = new XElement( "TilePrefab" );
            element.Add( new XAttribute( "name", Name ) );

            foreach( var component in Components )
                element.Add( component.GetSerializingElement() );

            return element;
        }
    }
}
