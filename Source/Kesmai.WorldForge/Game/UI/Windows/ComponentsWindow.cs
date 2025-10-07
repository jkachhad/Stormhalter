using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Xml.Linq;
using CommonServiceLocator;
using DigitalRune;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Storages;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;
using Kesmai.WorldForge.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using HorizontalAlignment = DigitalRune.Game.UI.HorizontalAlignment;
using VerticalAlignment = DigitalRune.Game.UI.VerticalAlignment;

namespace Kesmai.WorldForge.Windows;

public class ComponentsPanel : StackPanel
{
    private SegmentTile _tile;
    private WorldGraphicsScreen _screen;
    private PropertyGrid _propertyGrid;
    private StackPanel _actionsPanel;
    private StackPanel _leftPanel;
    private SegmentTile _originalTile;
    private bool _confirmed = false;
    private bool _pendingRefresh;
    private int _refreshIndex = -1;
    private bool _deferredSelectPending = true;
    private bool _isRefreshing = false;

    public static readonly int SelectedItemPropertyId = CreateProperty (
        typeof ( ComponentsPanel ), "SelectedItem", GamePropertyCategories.Default, null, default ( ComponentFrame ),
        UIPropertyOptions.AffectsRender );

    public ComponentFrame SelectedItem
    {
        get => GetValue<ComponentFrame> ( SelectedItemPropertyId );
        set => SetValue ( SelectedItemPropertyId, value );
    }

    public ComponentsPanel(SegmentRegion region, SegmentTile tile, WorldGraphicsScreen screen)
    {
        _tile = tile;
        _screen = screen;
        Focusable = true;
        IsFocusScope = true;

        Orientation = Orientation.Horizontal;
        
        VerticalAlignment = VerticalAlignment.Stretch;
    }

    protected override void OnUnload ( )
    {
        base.OnUnload ( );

        if ( !_confirmed )
        {
            _tile.Components.Clear ( );
            foreach ( var component in _originalTile.Components )
                _tile.Components.Add ( component.Clone ( ) );

            _tile.UpdateTerrain ( );
            _screen.InvalidateRender ( );
        }
    }

    protected override void OnUpdate ( TimeSpan deltaTime )
    {
        base.OnUpdate ( deltaTime );

        if ( _deferredSelectPending && IsLoaded && _leftPanel.Children.Count > 0 )
        {
            _deferredSelectPending = false;
            var frame = _leftPanel.Children.OfType<ComponentFrame> ( ).FirstOrDefault ( );
            if ( frame != null )
            {
                System.Diagnostics.Debug.WriteLine ( "[Deferred Select] Applying selection after full load." );
                Select ( frame );
            }
        }

        if ( _pendingRefresh && IsLoaded && Screen?.Renderer != null )
        {
            _pendingRefresh = false;
            RunRefreshComponentListSafe ( _refreshIndex );
        }
    }

    protected override void OnLoad ( )
    {
        base.OnLoad ( );

        _originalTile = new SegmentTile ( _tile.X, _tile.Y );
        foreach ( var component in _tile.Components )
            _originalTile.Components.Add ( component.Clone ( ) );

        _leftPanel = new StackPanel { Background = Color.DarkRed };
        foreach ( var component in _tile.Components )
        {
            var frame = new ComponentFrame { Component = component };
            frame.Click += ( o, args ) => Select ( (ComponentFrame) o );
            _leftPanel.Children.Add ( frame );
        }

        _propertyGrid = new PropertyGrid { VerticalAlignment = VerticalAlignment.Stretch };
        _propertyGrid.PropertyChanged += ( o, args ) =>
        {
            _tile.UpdateTerrain ( );
            _screen.InvalidateRender ( );
            RefreshComponentList ( );
        };

        _actionsPanel = new StackPanel
        {
            Style = "DarkCanvas",
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            IsFocusScope = true,
            Focusable = true
        };

        var rightPanel = new StackPanel { Background = Color.Blue };
        rightPanel.Children.Add ( _propertyGrid );
        rightPanel.Children.Add ( _actionsPanel );
        rightPanel.Children.Add ( BuildConfirmPanel ( ) );
        
        Children.Add ( _leftPanel );
        Children.Add ( rightPanel );
        
        _deferredSelectPending = true;
    }

    private StackPanel BuildConfirmPanel ( )
    {
        var panel = new StackPanel
        {
            Style = "DarkCanvas",
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var okButton = BuildButton ( "OK", Color.LightGreen, ( ) => { _confirmed = true; } );

        panel.Children.Add ( okButton );
        return panel;
    }

    private void RefreshComponentList ( )
    {
        if ( _isRefreshing || !IsLoaded || Screen == null )
            return;

        _isRefreshing = true;

        var targetId = SelectedItem?.Component?.Id;
        _leftPanel.Children.Clear ( );

        foreach ( var component in _tile.Components )
        {
            var frame = new ComponentFrame { Component = component };
            frame.Click += ( o, args ) => Select ( (ComponentFrame) o );
            _leftPanel.Children.Add ( frame );
        }

        var newFrame = _leftPanel.Children
            .OfType<ComponentFrame> ( )
            .FirstOrDefault ( f => f.Component.Id == targetId )
            ?? _leftPanel.Children.OfType<ComponentFrame> ( ).FirstOrDefault ( );

        if ( newFrame != null )
            Select ( newFrame );

        _isRefreshing = false;
    }

    public void Select ( ComponentFrame frame )
    {
        if ( _isRefreshing || frame?.Component == null )
            return;

        System.Diagnostics.Debug.WriteLine ( $"Select() called for: {frame.Component.GetType ( ).Name}" );
        SelectedItem = frame;
        _actionsPanel.Children.Clear ( );

        _propertyGrid.Item = frame.Component;

        foreach ( var button in frame.Component.GetInspectorActions ( ) )
        {
            _actionsPanel.Children.Add ( button );
        }

        AddStandardComponentButtons ( frame.Component );
    }

    private void AddStandardComponentButtons ( TerrainComponent component )
    {
        _actionsPanel.Children.Add ( BuildButton ( "Delete", Color.OrangeRed, ( ) =>
        {
            var index = _tile.Components.IndexOf ( component );
            if ( index >= 0 )
            {
                _tile.RemoveComponent ( component );
                _refreshIndex = index;
                _pendingRefresh = true;
            }
        } ) );

        _actionsPanel.Children.Add ( BuildButton ( "Move up", Color.OrangeRed, ( ) =>
        {
            var index = _tile.Components.IndexOf ( component );
            if ( index > 0 )
            {
                _tile.Components.Move ( index, index - 1 );
                RefreshComponentList ( );
            }
        } ) );

        _actionsPanel.Children.Add ( BuildButton ( "Move down", Color.OrangeRed, ( ) =>
        {
            var index = _tile.Components.IndexOf ( component );
            if ( index < _tile.Components.Count - 1 )
            {
                _tile.Components.Move ( index, index + 1 );
                RefreshComponentList ( );
            }
        } ) );

        _actionsPanel.Children.Add ( BuildButton ( "Save as Prefab", Color.LightGreen, ( ) =>
        {
            var inputWindow = new TextInputWindow ( "Enter prefab name:", "Save Prefab" );
            inputWindow.Closed += ( s, _ ) =>
            {
                if ( inputWindow.IsConfirmed && !string.IsNullOrWhiteSpace ( inputWindow.InputText ) )
                    SavePrefab ( inputWindow.InputText.Trim ( ) );
            };
            inputWindow.Show ( Screen );
        } ) );

        if ( component is TeleportComponent teleport )
        {
            _actionsPanel.Children.Add ( BuildButton ( "Select Destination", Color.OrangeRed, ( ) =>
            {
                var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter> ( );
                presenter.ConfiguringTeleporter = teleport;
            } ) );
        }
    }

    private Button BuildButton ( string label, Color color, Action onClick )
    {
        var button = new Button
        {
            Focusable = true,
            Content = new TextBlock
            {
                Text = label,
                Font = "Tahoma",
                FontSize = 10,
                FontStyle = MSDFStyle.Outline,
                Foreground = color,
                Stroke = Color.Black,
                Margin = new Vector4F ( 3 )
            }
        };
        button.Click += ( s, e ) =>
        {
            System.Diagnostics.Debug.WriteLine ( $"Button '{label}' clicked." );
            onClick ( );
        };
        return button;
    }

    private void SavePrefab ( string prefabName )
    {
        var prefabElement = new XElement ( "prefab", new XAttribute ( "name", prefabName ) );
        foreach ( var c in _tile.Components )
            prefabElement.Add ( c.GetXElement ( ) );

        var path = $"{Core.CustomArtPath}\\Data\\TilePrefabs.xml";

        try
        {
            XDocument doc;
            if ( File.Exists ( path ) )
            {
                doc = XDocument.Load ( path );
                var existing = doc.Root.Elements ( "prefab" )
                    .FirstOrDefault ( p => (string) p.Attribute ( "name" ) == prefabName );
                existing?.Remove ( );
                doc.Root.Add ( prefabElement );
            }
            else
            {
                Directory.CreateDirectory ( System.IO.Path.GetDirectoryName ( path ) );
                doc = new XDocument ( new XElement ( "prefabs", prefabElement ) );
            }
            doc.Save ( path );
            System.Windows.MessageBox.Show ( "Prefab saved successfully!", "Success", MessageBoxButton.OK );
        }
        catch ( Exception ex )
        {
            System.Windows.MessageBox.Show ( $"Failed to save prefab:\n{ex.Message}", "Error", MessageBoxButton.OK );
        }
    }



    private void RunRefreshComponentListSafe ( int index )
    {
        _leftPanel.Children.Clear ( );
        if ( _tile.Components == null ) return;

        ComponentFrame frameToSelect = null;

        foreach ( var component in _tile.Components )
        {
            var frame = new ComponentFrame { Component = component };
            frame.Click += ( o, args ) => Select ( (ComponentFrame) o );
            _leftPanel.Children.Add ( frame );

            if ( SelectedItem?.Component == component )
                frameToSelect = frame;
        }

        if ( frameToSelect == null && _leftPanel.Children.Count > 0 )
        {
            var safeIndex = Math.Min ( index, _leftPanel.Children.Count - 1 );
            frameToSelect = _leftPanel.Children[safeIndex] as ComponentFrame;
        }

        if ( frameToSelect != null )
            Select ( frameToSelect );

        _screen.InvalidateRender ( );
    }

    protected override void OnRender ( UIRenderContext context )
    {
        base.OnRender ( context );
        var selected = SelectedItem;
        if ( selected != null )
        {
            var selectedBounds = selected.ActualBounds.ToRectangle ( true );
            var renderer = ( context.Screen ?? Screen ).Renderer;
            var spriteBatch = renderer.SpriteBatch;
            spriteBatch.FillRectangle ( selectedBounds, Color.FromNonPremultiplied ( 0, 100, 255, 25 ) );
        }
    }

    protected override void OnHandleInput ( InputContext context )
    {
        if ( !IsVisible )
            return;

        base.OnHandleInput ( context );

        var inputService = InputService;
        if ( inputService == null )
            return;

        if ( !inputService.IsKeyboardHandled )
        {
            if ( ( inputService.IsDown ( Keys.LeftControl ) || inputService.IsDown ( Keys.RightControl ) ) )
            {
                if (inputService.IsReleased(Keys.C))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // copy=false means don't call FlushClipboard
                        Clipboard.SetDataObject(SelectedItem.Component.GetXElement().ToString(), false);
                    });

                    inputService.IsKeyboardHandled = true;
                }
                else if ( inputService.IsReleased ( Keys.V ) )
                {
                    try
                    {
                        var clipboard = Clipboard.GetText ( );
                        if ( !string.IsNullOrWhiteSpace ( clipboard ) )
                        {
                            var element = XDocument.Parse ( clipboard );
                            if ( element.Root.Name == "component" )
                            {
                                var componentTypename = $"Kesmai.WorldForge.Models.{element.Root.Attribute ( "type" ).Value}";
                                var componentType = Assembly.GetExecutingAssembly ( ).GetType ( componentTypename, true );
                                if ( Activator.CreateInstance ( componentType, element.Root ) is TerrainComponent newComponent )
                                {
                                    _tile.AddComponent ( newComponent );
                                    RefreshComponentList ( );
                                    _deferredSelectPending = true;
                                }
                            }
                        }
                    }
                    catch { }
                    
                    inputService.IsKeyboardHandled = true;
                }
            }
        }
        
        // catch all mouse events to prevent bleeding into the underlying map.
        if (IsMouseOver)
            inputService.IsMouseOrTouchHandled = true;
    }
}
