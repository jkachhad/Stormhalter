using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommonServiceLocator;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Editor.MVP.Models.World.Components;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.UI;

public class ComponentImage : Image
{
    public bool EnableTinting { get; set; } = false;

    public static readonly DependencyProperty ComponentProperty = DependencyProperty.Register (
        nameof ( Component ), typeof ( TerrainComponent ), typeof ( ComponentImage ),
        new PropertyMetadata ( default ( TerrainComponent ), OnTextureChanged ) );

    public TerrainComponent Component
    {
        get => (TerrainComponent) GetValue ( ComponentProperty );
        set => SetValue ( ComponentProperty, value );
    }

    private static void OnTextureChanged ( DependencyObject sender, DependencyPropertyChangedEventArgs args )
    {
        if ( sender is ComponentImage componentImage && args.NewValue is TerrainComponent component )
            componentImage.UpdateComponent ( component );
    }

    static ComponentImage ( )
    {
        WidthProperty.OverrideMetadata ( typeof ( ComponentImage ), new FrameworkPropertyMetadata ( (double) 100 ) );
        HeightProperty.OverrideMetadata ( typeof ( ComponentImage ), new FrameworkPropertyMetadata ( (double) 100 ) );
    }

    public ComponentImage ( )
    {
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
    }

    internal void UpdateComponent ( TerrainComponent component )
    {
        if ( component == null )
        {
            return;
        }
        if ( component is StaticComponent staticComponent && staticComponent.CachedPreview != null )
        {
            //System.Diagnostics.Debug.WriteLine ( $"[ComponentImage] Using cached preview for static component: {staticComponent.Static}" );
            Source = staticComponent.CachedPreview;
            return;
        }
        var maxWidth = 100;
        var maxHeight = 100;

        var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter> ( );
        var selector = presenter?.SelectedFilter;

        var renderList = new List<TerrainRender> ( );
        if ( component is TilePrefabComponent tilePrefab )
        {
            foreach ( var subComponent in tilePrefab.Components )
            {
                foreach ( var render in subComponent.GetTerrain ( ) )
                {
                    {
                        var transform = selector.TransformRender ( null, subComponent, render );
                        var tintColor = transform.Color;
                        renderList.AddRange ( transform.Terrain.Select ( layer => new TerrainRender ( layer, transform.Color ) ) );

                    }

                }
            }
        }
        else
        {
            foreach ( var render in component.GetTerrain ( ) )
            {
                var transform = selector.TransformRender ( null, component, render );
                renderList.AddRange ( transform.Terrain.Select ( layer => new TerrainRender ( layer, transform.Color ) ) );
            }

        }
        var terrainList = renderList.OrderBy ( r => r.Layer.Order ).ToList ( );

        foreach ( var terrain in terrainList )
        {
            var layer = terrain.Layer;
            var sprite = layer.Sprite;
            if ( sprite == null || sprite.Bitmap == null )
            {
                maxWidth = Math.Max ( maxWidth, sprite.Texture.Width );
                maxHeight = Math.Max ( maxHeight, sprite.Texture.Height );
            }
        }


        var writeableBitmap = BitmapFactory.New ( maxWidth, maxHeight );

        writeableBitmap.Lock ( );

        foreach ( var terrain in terrainList )
        {
            var tint = terrain.Color;
            var layer = terrain.Layer;

            var sprite = layer.Sprite;
            if ( sprite?.Bitmap != null )
            {
                var spriteBounds = new Vector2F ( 0, 0 );

                if ( sprite.Offset != Vector2F.Zero )
                    spriteBounds += new Vector2F ( sprite.Offset.X, sprite.Offset.Y );

                WriteableBitmap bitmapToUse = sprite.Bitmap;

                if ( EnableTinting || component is TilePrefabComponent )

                {
                    bitmapToUse = sprite.Bitmap.Clone ( );

                    bitmapToUse.ForEach ( ( x, y, color ) =>
                    {
                        // Alpha-blended tint (multiplicative for RGB, preserve original alpha)
                        byte r = (byte) ( ( color.R * tint.R ) / 255 );
                        byte g = (byte) ( ( color.G * tint.G ) / 255 );
                        byte b = (byte) ( ( color.B * tint.B ) / 255 );
                        byte a = color.A; // preserve original alpha (or use tint.A if you want override)

                        return Color.FromArgb ( a, r, g, b );
                    } );

                }

                writeableBitmap.Blit (
                    new Rect ( spriteBounds.X, spriteBounds.Y, maxWidth, maxHeight ),
                    bitmapToUse,
                    new Rect ( 0, 0, sprite.Texture.Width, sprite.Texture.Height ),
                    WriteableBitmapExtensions.BlendMode.Alpha );
            }
        }


        writeableBitmap.Unlock ( );
        writeableBitmap.Freeze ( );
        if ( component is StaticComponent sc )
            sc.CachedPreview = writeableBitmap;

        Source = writeableBitmap;
    }
}