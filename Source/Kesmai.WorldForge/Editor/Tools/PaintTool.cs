using System;
using System.Linq;
using System.Windows.Input;
using CommonServiceLocator;
using DigitalRune.Game.Input;
using DigitalRune.Graphics;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;
using Microsoft.Xna.Framework.Input;
using DigitalRune.Game.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using DigitalRune.Game.UI.Rendering;
using Kesmai.WorldForge.Windows;

namespace Kesmai.WorldForge;

public class PaintTool : Tool
{
    private bool _isShiftDown;
    private bool _isAltDown;

    public PaintTool ( ) : base ( "Paint", "Editor-Icon-Paint" )
    {
    }
    private IEnumerable<TerrainComponent> GetSimilarComponents ( SegmentTile tile, Type componentType )
    {
        var floorTypes = new List<string> ( ) {
        "FloorComponent", "WaterComponent", "IceComponent", "SkyComponent"
    };

        if ( floorTypes.Contains ( componentType.Name ) )
        {
            return tile.GetComponents<TerrainComponent> ( c =>
                c is FloorComponent || c is WaterComponent || c is IceComponent || c is SkyComponent );
        }
        else
        {
            return tile.GetComponents<TerrainComponent> ( c =>
                c.GetType ( ).IsAssignableFrom ( componentType ) );
        }
    }


    public override void OnHandleInput ( WorldPresentationTarget target, IInputService inputService )
    {
        base.OnHandleInput ( target, inputService );

        if ( inputService.IsMouseOrTouchHandled )
            return;

        var services = ServiceLocator.Current;
        var presenter = services.GetInstance<ApplicationPresenter>();
        var regionToolbar = services.GetInstance<RegionToolbar>();

        var graphicsScreen = target.WorldScreen;
        var region = target.Region;
        var selection = presenter.Selection;

        if ( !inputService.IsKeyboardHandled )
        {

            _isShiftDown = inputService.IsDown ( Keys.LeftShift ) || inputService.IsDown ( Keys.RightShift );
            _isAltDown = inputService.IsDown ( Keys.LeftAlt ) || inputService.IsDown ( Keys.RightAlt );

            if ( inputService.IsReleased ( Keys.Escape ) )
            {
                regionToolbar.SelectTool(null);
                inputService.IsKeyboardHandled = true;
            }
        }

        var (cx, cy) = graphicsScreen.ToWorldCoordinates ( (int) _position.X, (int) _position.Y );

        if ( !selection.Any ( ) )
            return;

        if ( inputService.IsReleased ( MouseButtons.Left ) && selection.IsSelected ( cx, cy, region ) )
        {
            var component = presenter.SelectedComponent;

            var baseComponent = presenter.SelectedComponent;

            // TODO: Refactor later since we no longer have a component window.
            // This should be resolved by templates.
            /*if ( baseComponent != null )
            {
                // Create a temporary tile and insert the component clone
                var tempTile = new SegmentTile ( cx, cy );
                tempTile.Components.Add ( baseComponent.Clone ( ) );
                tempTile.UpdateTerrain ( );

                var componentWindow = new ComponentsWindow ( region, tempTile, graphicsScreen );
                componentWindow.Show ( graphicsScreen.UI );
                componentWindow.Center ( );

                componentWindow.Closed += ( s, e ) =>
                {
                    var configuredComponent = tempTile.Components.FirstOrDefault ( );

                    if ( configuredComponent == null )
                        return;

                    foreach ( var area in selection )
                    {
                        for ( var x = area.Left; x < area.Right; x++ )
                            for ( var y = area.Top; y < area.Bottom; y++ )
                            {
                                var selectedTile = region.GetTile ( x, y );

                                if ( selectedTile == null )
                                    region.SetTile ( x, y, selectedTile = new SegmentTile ( x, y ) );

                                if ( !_isShiftDown && !_isAltDown )
                                {
                                    var similar = GetSimilarComponents ( selectedTile, baseComponent.GetType ( ) );
                                    foreach ( var similarComponent in similar )
                                        selectedTile.RemoveComponent ( similarComponent );
                                }
                                else if ( _isAltDown )
                                {
                                    selectedTile.Components.Clear ( );
                                }

                                selectedTile.Components.Add ( configuredComponent.Clone ( ) );
                                selectedTile.UpdateTerrain ( );
                            }
                    }

                    graphicsScreen.InvalidateRender ( );
                };
            }*/


        }
    }

    public override void OnActivate ( )
    {
        base.OnActivate ( );
        _cursor = Cursors.Arrow;
    }

    public override void OnDeactivate ( )
    {
        base.OnDeactivate ( );
    }

    public override void OnRender ( RenderContext context )
    {
        base.OnRender ( context );

        var graphicsService = context.GraphicsService;
        var spriteBatch = graphicsService.GetSpriteBatch ( );

        if (context.PresentationTarget is not WorldPresentationTarget presentationTarget)
            return;

        var worldScreen = presentationTarget.WorldScreen;
        var uiScreen = worldScreen.UI;
        var renderer = uiScreen.Renderer;
        var spriteFont = renderer.GetFontRenderer ( "Tahoma", 10 );

        var text = String.Empty;

        if ( _isShiftDown )
            text = "Append";
        if ( _isAltDown )
            text = "Replace";

        var position = (Vector2) _position + new Vector2 ( 10.0f, -10.0f );

        spriteFont.DrawString ( spriteBatch, RenderTransform.Identity, text, position + new Vector2 ( 1f, 1f ),
            Color.Black );
        spriteFont.DrawString ( spriteBatch, RenderTransform.Identity, text, position,
            Color.Yellow );
    }
}