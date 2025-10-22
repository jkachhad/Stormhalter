using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kesmai.WorldForge.Windows;

public class ComponentsPanel : StackPanel
{
    private SegmentTile _targetTile;    
    private SegmentTile _restoreTile;
    
    private WorldGraphicsScreen _screen;
    private StackPanel _framePanel;
    
    private bool _invalidated;

    public static readonly int SelectedItemPropertyId = CreateProperty(
        typeof(ComponentsPanel), nameof(SelectedItem), GamePropertyCategories.Default, null, default(IComponentProvider),
        UIPropertyOptions.AffectsRender);

    public IComponentProvider SelectedItem
    {
        get => GetValue<IComponentProvider> ( SelectedItemPropertyId );
        set => SetValue ( SelectedItemPropertyId, value );
    }

    public ComponentsPanel(SegmentRegion region, SegmentTile targetTile, WorldGraphicsScreen screen)
    {
        _targetTile = targetTile;
        _screen = screen;
        
        Focusable = true;
        IsFocusScope = true;

        Orientation = Orientation.Horizontal;
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        // copy original tile for reset
        _restoreTile = new SegmentTile(_targetTile.X, _targetTile.Y);

        foreach (var component in _targetTile.Providers)
            component.AddComponent(_restoreTile);

        // listen for changes to the components collection.
        _targetTile.Providers.CollectionChanged += (s, e) =>
        {
            Invalidate(); 
            
            _targetTile.UpdateTerrain();
            _screen.InvalidateRender();
        };

        // left panel with components
        if (_framePanel is null)
        {
            _framePanel = new StackPanel
            {
                HorizontalAlignment = DigitalRune.Game.UI.HorizontalAlignment.Stretch
            };
        }

        Children.Add(_framePanel);

        Invalidate();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
    }

    public void Invalidate()
    {
        _invalidated = true;
    }

    public void Update()
    {
        _framePanel.Children.Clear();

        for (int index = 0; index < _targetTile.Providers.Count; index++)
        {
            var provider = _targetTile.Providers[index];

            var frame = new ComponentFrame
            {
                Provider = provider,

                CanMoveUp = index > 0,
                CanMoveDown = index < (_targetTile.Providers.Count - 1),
                
                // only allow delete if there is more than one component.
                CanDelete = index > 0,
            };
            frame.OnClick += OnSelect;
            frame.OnMoveUp += OnMoveUp;
            frame.OnMoveDown += OnMoveDown;
            frame.OnDelete += OnDelete;
            frame.OnChange += OnChange;

            _framePanel.Children.Add(frame);
        }
    }

    private void OnChange(object sender, EventArgs e)
    {
        _targetTile.UpdateTerrain();
        _screen.InvalidateRender();
    }

    private void OnSelect(object sender, EventArgs e)
    {
        if (sender is not ComponentFrame frame)
            return;
        
        SelectedItem = frame.Provider;
    }

    private void OnMoveUp(object sender, EventArgs e)
    {
        if (sender is not ComponentFrame frame)
            return;

        MoveUp(frame.Provider);
    }

    private void MoveUp(IComponentProvider component)
    {
        var index = _targetTile.Providers.IndexOf(component);

        if (index > 0)
            _targetTile.Providers.Move(index, index - 1);
    }
    
    private void OnMoveDown(object sender, EventArgs e)
    {
        if (sender is not ComponentFrame frame)
            return;
        
        MoveDown(frame.Provider);
    }
    
    private void MoveDown(IComponentProvider component)
    {
        var index = _targetTile.Providers.IndexOf(component);

        if (index < _targetTile.Providers.Count - 1)
            _targetTile.Providers.Move(index, index + 1);
    }

    private void OnDelete(object sender, EventArgs e)
    {
        if (sender is not ComponentFrame frame)
            return;
        
        Delete(frame.Provider);
    }
    
    private void Delete(IComponentProvider component)
    {
        component.RemoveComponent(_targetTile);
    }

    public void Reset()
    {
        // restore original tile.
        _targetTile.Providers.Clear();
        
        foreach (var provider in _restoreTile.Providers)
            provider.AddComponent(_targetTile);

        _targetTile.UpdateTerrain();
        
        _screen.InvalidateRender();
    }

    protected override void OnUpdate(TimeSpan deltaTime)
    {
        if (_invalidated)
        {
            Update();
            
            _invalidated = false;
        }
        
        base.OnUpdate(deltaTime);
    }

    protected override void OnRender(UIRenderContext context)
    {
        var selected = SelectedItem;
        
        foreach (var frame in _framePanel.Children.OfType<ComponentFrame>())
        {
            if (selected != frame.Provider)
                continue;
            
            var selectedBounds = frame.ActualBounds.ToRectangle(true);
            var renderer = (context.Screen ?? Screen).Renderer;
            var spriteBatch = renderer.SpriteBatch;

            spriteBatch.FillRectangle(selectedBounds, Color.OrangeRed);
        }

        base.OnRender(context);
    }

    protected override void OnHandleInput(InputContext context)
    {
        if (!IsVisible)
            return;

        base.OnHandleInput(context);

        var inputService = InputService;
        if (inputService == null)
            return;

        if (!inputService.IsKeyboardHandled)
        {
            var isShiftDown = inputService.IsDown(Keys.LeftShift) || inputService.IsDown(Keys.RightShift);
            var isControlDown = inputService.IsDown(Keys.LeftControl) || inputService.IsDown(Keys.RightControl);
            
            if (inputService.IsReleased(Keys.Delete))
            {
                if (SelectedItem != null)
                    Delete(SelectedItem);
                
                inputService.IsKeyboardHandled = true;
            }
            
            if (isShiftDown)
            {
                if (inputService.IsReleased(Keys.Up))
                {
                    if (SelectedItem != null)
                        MoveUp(SelectedItem);
                    
                    inputService.IsKeyboardHandled = true;
                }
                else if (inputService.IsReleased(Keys.Down))
                {
                    if (SelectedItem != null)
                        MoveDown(SelectedItem);
                    
                    inputService.IsKeyboardHandled = true;
                }
            }
            
            if (isControlDown)
            {
                if (inputService.IsReleased(Keys.Z))
                {
                    Reset();
                    
                    inputService.IsKeyboardHandled = true;
                }
                else if (inputService.IsReleased(Keys.C))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Clipboard.SetDataObject(SelectedItem.GetXElement().ToString(), false);
                    });

                    inputService.IsKeyboardHandled = true;
                }
                else if (inputService.IsReleased(Keys.V))
                {
                    try
                    {
                        var clipboard = Clipboard.GetText();
                    
                        if (!String.IsNullOrWhiteSpace(clipboard))
                        {
                            var element = XDocument.Parse(clipboard);
                            var rootElement = element.Root;

                            if (rootElement is null)
                                throw new XmlException("XML has no root element.");
                        
                            if (rootElement.Name == "component")
                            {
                                var typeAttribute = rootElement.Attribute("type");
                                
                                if (typeAttribute is null)
                                    throw new XmlException("Component XML has no type attribute.");
                                
                                var componentTypename = $"Kesmai.WorldForge.Models.{typeAttribute.Value}";
                                var componentType = Assembly.GetExecutingAssembly().GetType(componentTypename, false);

                                if (componentType is null)
                                    throw new NullReferenceException($"Could not find component type: {componentTypename}");
                                
                                if (Activator.CreateInstance(componentType, element.Root) is not TerrainComponent newComponent)
                                    throw new InvalidCastException($"Could not create component of type: {componentTypename}");
                                    
                                _targetTile.AddComponent(SelectedItem = newComponent);
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                    
                    inputService.IsKeyboardHandled = true;
                }
            }
        }

        // catch all mouse events to prevent bleeding into the underlying map.
        if (IsMouseOver)
            inputService.IsMouseOrTouchHandled = true;
    }
}
