using System;
using System.Collections.Generic;
using CommonServiceLocator;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Editor.MVP.Models.World.Components;
using Kesmai.WorldForge.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Kesmai.WorldForge;

public class DrawTool : Tool
{
    private bool _isShiftDown;
    private bool _isAltDown;

    private List<(int X, int Y)> _actionBlacklist;

    public DrawTool ( ) : base ( "Draw", @"Editor-Icon-Pencil" )
    {
    }

    public override void OnActivate ( )
    {
        base.OnActivate ( );
        _actionBlacklist = new List<(int X, int Y)> ( );
    }

    public override void OnDeactivate ( )
    {
        base.OnDeactivate ( );
    }

    public override void OnHandleInput ( PresentationTarget target, IInputService inputService )
    {
        base.OnHandleInput ( target, inputService );
        var floorTypes = new List<string> {
    "FloorComponent",
    "WaterComponent",
    "IceComponent",
    "SkyComponent"
};


        if ( inputService.IsMouseOrTouchHandled )
            return;

        var services = ServiceLocator.Current;
        var presenter = services.GetInstance<ApplicationPresenter> ( );

        var worldScreen = target.WorldScreen;
        var region = target.Region;
        var selection = presenter.Selection;

        if ( !inputService.IsKeyboardHandled )
        {
            _isAltDown = inputService.IsDown ( Keys.LeftAlt ) || inputService.IsDown ( Keys.RightAlt );
            _isShiftDown = inputService.IsDown ( Keys.LeftShift ) || inputService.IsDown ( Keys.RightShift );

            if ( inputService.IsReleased ( Keys.Escape ) )
            {
                presenter.SelectTool ( default ( Tool ) );
                inputService.IsKeyboardHandled = true;
            }
        }

        var (mx, my) = worldScreen.ToWorldCoordinates ( (int) _position.X, (int) _position.Y );

        if ( inputService.IsReleased ( MouseButtons.Left ) )
        {
            _actionBlacklist.Clear ( );
        }
        else if ( inputService.IsDown ( MouseButtons.Left ) && !_actionBlacklist.Contains ( (mx, my) ) )
        {
            if ( selection.SurfaceArea <= 1 || selection.IsSelected ( mx, my, region ) )
            {
                var component = presenter.SelectedComponent;
                var selectedTile = region.GetTile ( mx, my );

                if ( selectedTile == null )
                    region.SetTile ( mx, my, selectedTile = new SegmentTile ( mx, my ) );

                if ( component != null )
                {
                    var prefab = component as TilePrefabComponent;
                    var isPrefab = prefab != null;

                    // If drawing a prefab, replace the entire tile
                    if ( isPrefab )
                    {
                        selectedTile.Components.Clear ( );
                        foreach ( var sub in prefab.Components )
                            selectedTile.AddComponent ( sub.Clone ( ) );
                    }
                    else
                    {
                        var componentType = component.GetType ( );

                        if ( !_isAltDown && !_isShiftDown )
                        {
                            if ( floorTypes.Contains ( componentType.Name ) )
                            {
                                var similar = selectedTile.GetComponents<TerrainComponent> ( c => floorTypes.Contains ( c.GetType ( ).Name ) );
                                foreach ( var existing in similar )
                                    selectedTile.RemoveComponent ( existing );
                            }
                            else
                            {
                                var similar = selectedTile.GetComponents<TerrainComponent> ( c => c.GetType ( ).IsAssignableFrom ( componentType ) );
                                foreach ( var existing in similar )
                                    selectedTile.RemoveComponent ( existing );
                            }
                        }
                        else if ( _isAltDown )
                        {
                            selectedTile.Components.Clear ( );
                        }

                        selectedTile.AddComponent ( component.Clone ( ) );
                    }


                    selectedTile.UpdateTerrain ( );
                    worldScreen.InvalidateRender ( );
                    inputService.IsMouseOrTouchHandled = true;
                }


                _actionBlacklist.Add ( (mx, my) );
            }
        }
    }

    public override void OnRender ( RenderContext context )
    {
        base.OnRender ( context );

        var services = ServiceLocator.Current;
        var presenter = services.GetInstance<ApplicationPresenter> ( );

        var graphicsService = context.GraphicsService;
        var spriteBatch = graphicsService.GetSpriteBatch ( );

        var presentationTarget = context.GetPresentationTarget ( );

        var worldScreen = presentationTarget.WorldScreen;
        var zoomFactor = worldScreen.ZoomFactor;

        var uiScreen = worldScreen.UI;
        var renderer = uiScreen.Renderer;
        var spriteFont = renderer.GetFontRenderer ( "Tahoma", 10 );

        var component = presenter.SelectedComponent;

        if ( component != null )
        {
            var viewRectangle = worldScreen.GetViewRectangle ( );
            var (mx, my) = worldScreen.ToWorldCoordinates ( (int) _position.X, (int) _position.Y );

            var rx = (int) Math.Floor ( ( mx - viewRectangle.Left ) * ( presenter.UnitSize * zoomFactor ) );
            var ry = (int) Math.Floor ( ( my - viewRectangle.Top ) * ( presenter.UnitSize * zoomFactor ) );

            var tileBounds = new Rectangle ( rx, ry, (int) Math.Floor ( presenter.UnitSize * zoomFactor ), (int) Math.Floor ( presenter.UnitSize * zoomFactor ) );
            var originalBounds = new Rectangle ( tileBounds.X - (int) Math.Floor ( 45 * zoomFactor ), tileBounds.Y - (int) Math.Floor ( 45 * zoomFactor ), (int) Math.Floor ( 100 * zoomFactor ), (int) Math.Floor ( 100 * zoomFactor ) );

            var terrains = component.GetTerrain ( );

            foreach ( var render in terrains )
            {
                foreach ( var layer in render.Terrain )
                {
                    var sprite = layer.Sprite;

                    if ( sprite != null )
                    {
                        var spriteBounds = originalBounds;

                        if ( sprite.Offset != Vector2F.Zero )
                            spriteBounds.Offset ( sprite.Offset.X, sprite.Offset.Y );

                        spriteBatch.Draw ( sprite.Texture, spriteBounds.Location.ToVector2 ( ), null, render.Color, 0, Vector2.Zero, zoomFactor / sprite.Resolution, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f );
                    }
                }
            }

            var text = String.Empty;

            if ( !_isAltDown & _isShiftDown )
                text = "Append";
            else if ( _isAltDown && !_isShiftDown )
                text = "Replace";

            var position = (Vector2) _position + new Vector2 ( 10.0f, -10.0f );

            spriteFont.DrawString ( spriteBatch, RenderTransform.Identity, text, position + new Vector2 ( 1f, 1f ),
                Color.Black );
            spriteFont.DrawString ( spriteBatch, RenderTransform.Identity, text, position,
                Color.Yellow );
        }
    }
}