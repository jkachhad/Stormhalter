using System;
using System.Collections.Generic;
using System.Linq;
using CommonServiceLocator;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.ServiceLocation;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;
using CommunityToolkit.Mvvm.Messaging;
using DigitalRune.Game.Interop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kesmai.WorldForge;

public class SubregionsGraphicsScreen : WorldGraphicsScreen
{
	private SegmentSubregion _subregion;
		
	public SubregionsGraphicsScreen(IGraphicsService graphicsService, WorldPresentationTarget worldPresentationTarget) : base(graphicsService, worldPresentationTarget)
	{
	}

	public void SetSubregion(SegmentSubregion subregion)
	{
		_subregion = subregion;

		var first = _subregion.Rectangles.FirstOrDefault();

		if (first != null)
			CenterCameraOn(first.Left, first.Top);
	}

	protected override void OnAfterRender(SpriteBatch spriteBatch)
	{
		base.OnAfterRender(spriteBatch);

		if (_subregion != null)
		{
			var viewRectangle = GetViewRectangle();
			var rectangles = _subregion.Rectangles;

			foreach (var rectangle in rectangles)
			{
				if (!viewRectangle.Intersects(rectangle.ToRectangle()))
					continue;

				var bounds = GetRenderRectangle(viewRectangle, rectangle.ToRectangle());

				spriteBatch.FillRectangle(bounds, _subregion.Color);
				spriteBatch.DrawRectangle(bounds, _subregion.Border);

				_font.DrawString(spriteBatch, RenderTransform.Identity, _subregion.Name,
					new Vector2(bounds.X + 5, bounds.Y + 5), Color.White);
			}
		}
	}
}

public class SpawnsGraphicsScreen : WorldGraphicsScreen
{
	private Spawner _spawner;
	private Color _inclusionBorder = Color.FromNonPremultiplied(200, 255, 50, 255);
	private Color _inclusionFill = Color.FromNonPremultiplied(200, 255, 50, 50);
	private Color _exclusionBorder = Color.FromNonPremultiplied(0, 0, 0, 255);
	private Color _exclusionFill = Color.FromNonPremultiplied(50, 50, 50, 200);
	private Color _locationBorder = Color.FromNonPremultiplied(0, 255, 255, 200);

	public SpawnsGraphicsScreen(IGraphicsService graphicsService, WorldPresentationTarget worldPresentationTarget) : base(graphicsService, worldPresentationTarget)
	{
	}

	public void SetSpawner(Spawner spawner)
	{
		_spawner = spawner;
		
		switch (_spawner)
		{
			case LocationSpawner ls:
			{
				CenterCameraOn(ls.X, ls.Y);
				break;
			}
			case RegionSpawner rs:
			{
				var inclusion = rs.Inclusions.FirstOrDefault();

				if (inclusion != null)
					CenterCameraOn((int)(inclusion.Left + inclusion.Width / 2),
						(int)(inclusion.Top + inclusion.Height / 2));
				
				break;
			}
		}
	}

	protected override void OnAfterRender(SpriteBatch spriteBatch)
	{
		base.OnAfterRender(spriteBatch);

		if (_spawner is RegionSpawner rs)
		{
			var viewRectangle = GetViewRectangle();

			var inclusions = rs.Inclusions;
			foreach (var rectangle in inclusions)
			{
				if (!viewRectangle.Intersects(rectangle.ToRectangle()))
					continue;

				var bounds = GetRenderRectangle(viewRectangle, rectangle.ToRectangle());

				spriteBatch.FillRectangle(bounds, _inclusionFill);
				spriteBatch.DrawRectangle(bounds, _inclusionBorder);

				_font.DrawString(spriteBatch, RenderTransform.Identity, _spawner.Name,
					new Vector2(bounds.X + 5, bounds.Y + 5), Color.White);
			}

			var exclusions = rs.Exclusions;
			foreach (var rectangle in exclusions)
			{
				if (rectangle is { Left: 0, Top: 0, Right:0, Bottom:0 } || !viewRectangle.Intersects(rectangle.ToRectangle()))
					continue;

				var bounds = GetRenderRectangle(viewRectangle, rectangle.ToRectangle());

				spriteBatch.FillRectangle(bounds, _exclusionFill);
				spriteBatch.DrawRectangle(bounds, _exclusionBorder);

				_font.DrawString(spriteBatch, RenderTransform.Identity, "Exclusion",
					new Vector2(bounds.X + 5, bounds.Y + 5), Color.White);
			}
		}
		if (_spawner is LocationSpawner ls)
		{
            var viewRectangle = GetViewRectangle();
			var _mx = ls.X;
			var _my = ls.Y;
			if (viewRectangle.Contains(_mx, _my))
			{
				var bounds = GetRenderRectangle(viewRectangle, _mx, _my);
				var innerRectangle = new Rectangle(bounds.Left, bounds.Top, 55, 20);

				spriteBatch.DrawRectangle(bounds, _locationBorder);
				spriteBatch.FillRectangle(innerRectangle, _locationBorder);

				_font.DrawString(spriteBatch, RenderTransform.Identity, $"{_mx}, {_my}",
					new Vector2(bounds.Left + 4, bounds.Top + 3), Color.Black);
			}
		}
	}
}

public class LocationsGraphicsScreen : WorldGraphicsScreen
{
	private static Color _highlightColor = Color.FromNonPremultiplied(0, 255, 255, 200);
		
	private int _mx;
	private int _my;
		
	public LocationsGraphicsScreen(IGraphicsService graphicsService, WorldPresentationTarget worldPresentationTarget) : base(graphicsService, worldPresentationTarget)
	{
		DrawGrid = true;
		Gridcolor = Color.FromNonPremultiplied(154, 205, 50, 200);
	}

	public void SetLocation(int mx, int my)
	{
		_mx = mx;
		_my = my;
			
		CenterCameraOn(_mx, _my);
	}

	protected override void OnAfterRender(SpriteBatch spriteBatch)
	{
		base.OnAfterRender(spriteBatch);

		var viewRectangle = GetViewRectangle();

		if (viewRectangle.Contains(_mx, _my))
		{
			var bounds = GetRenderRectangle(viewRectangle, _mx, _my);
			var innerRectangle = new Rectangle(bounds.Left, bounds.Top, 55, 20);
				
			spriteBatch.DrawRectangle(bounds, _highlightColor);
			spriteBatch.FillRectangle(innerRectangle, _highlightColor);
				
			_font.DrawString(spriteBatch, RenderTransform.Identity, $"{_mx}, {_my}", 
				new Vector2(bounds.Left + 4, bounds.Top + 3), Color.Black);
		}
	}
}

public class WorldGraphicsScreen : InteropGraphicsScreen
{
	private static List<Keys> _selectorKeys = new List<Keys>()
	{
		Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8
	};

	private static List<Keys> _toolKeys = new List<Keys>()
	{
		Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8
	};

	protected static Color _selectionFill = Color.FromNonPremultiplied(255, 255, 0, 75);
	protected static Color _selectionBorder = Color.FromNonPremultiplied(255, 255, 0, 100);

	protected ApplicationPresenter _presenter;
	private Selection _selection;

	private WorldPresentationTarget _worldPresentationTarget;
	protected UIScreen _uiScreen;
	private Menu _contextMenu;
	protected MSDFontRenderer _font;
	private List<MenuItem> _pointContextItems = new List<MenuItem>();
	private List<MenuItem> _selectionContextItems = new List<MenuItem>();
	private List<MenuItem> _spawnerContextItems = new List<MenuItem>();
	private List<MenuItem> _teleporterSourceContextItems = new List<MenuItem>();
	private List<MenuItem> _teleporterDestinationContextItems = new List<MenuItem>();

	private RenderTarget2D _renderTarget;
	private bool _invalidateRender;

	private Vector2F _cameraLocation = Vector2F.Zero;
	private Vector2F _cameraDrag = Vector2F.Zero;

	protected float _zoomFactor = 1.0f;

	private bool _drawgrid = false;
	private Color _gridcolor = Color.FromNonPremultiplied(255, 255, 0, 75);

	private Texture2D _commentSprite = null;

	private bool _isMouseOver;
	private bool _isMouseDirectlyOver;

	public Vector2F CameraLocation
	{
		get => _cameraLocation;
		set
		{
			_cameraLocation = value;
			_invalidateRender = true;
		}
	}

	public Vector2F CameraDrag
	{
		get => _cameraDrag;
		set
		{
			_cameraDrag = value;
			_invalidateRender = true;
		}
	}

	public float ZoomFactor
	{
		get => _zoomFactor;
		set
		{
			_zoomFactor = value;
			if (_zoomFactor < 0.2f) { _zoomFactor = 0.2f; }
			_invalidateRender = true;
		}
	}

	public bool DrawGrid
	{
		get => _drawgrid;
		set
		{
			_drawgrid = value;
			_invalidateRender = true;
		}
	}
	public Color Gridcolor
	{
		get => _gridcolor;
		set
		{
			_gridcolor = value;
			_invalidateRender = true;
		}
	}

	public int Width => (int)_worldPresentationTarget.ActualWidth;
	public int Height => (int)_worldPresentationTarget.ActualHeight;

	public UIScreen UI => _uiScreen;

	public WorldGraphicsScreen(IGraphicsService graphicsService, WorldPresentationTarget worldPresentationTarget) : base(graphicsService, worldPresentationTarget)
	{
		_worldPresentationTarget = worldPresentationTarget;

		var services = ServiceLocator.Current;

		_presenter = services.GetInstance<ApplicationPresenter>();
		_selection = _presenter.Selection;
		_renderTarget = new RenderTarget2D(GraphicsService.GraphicsDevice, 640, 480);
		
		_contextMenu = new Menu();

		var createSpawnMenuItem = new MenuButton() { Content = new TextBlock() { Text = "Create Location Spawner.." } };
		var createRegionSpawnerMenuItem = new MenuButton() { Content = new TextBlock() { Text = "Create Region Spawner.." } };
		var createSubregionMenuItem = new MenuButton() { Content = new TextBlock() { Text = "Create Subregion.." } };
        var SubregionIncludeMenuItem = new MenuButton() { Content = new TextBlock() { Text = "Add selection to Subregion.." } };
        var createLocationMenuItem = new MenuButton() { Content = new TextBlock() { Text = "Create Named Location.." } };
		var regionSpawnerIncludeMenuItem = new MenuButton() { Content = new TextBlock() { Text = "Add selection to Inclusions.." } };
		var regionSpawnerExcludeMenuItem = new MenuButton() { Content = new TextBlock() { Text = "Add selection to Exclusions.." } };
		var configureTeleporterMenuItem = new MenuButton() { Content = new TextBlock() { Text = "Set as Teleporter Destination..", IsVisible = false } };
		var cancelConfigureTeleporterMenuItem = new MenuButton() { Content = new TextBlock() { Text = "Cancel", IsVisible = false } };
		var configureThisTeleporterMenuItem = new MenuButton() { Content = new TextBlock() { Text = "Choose a Destination..", IsVisible = false } };


		createSpawnMenuItem.Click += CreateLocationSpawner;
		createLocationMenuItem.Click += CreateLocation;
		createRegionSpawnerMenuItem.Click += CreateRegionSpawner;
		createSubregionMenuItem.Click += CreateSubregion;
        SubregionIncludeMenuItem.Click += SubregionInclude;
        regionSpawnerIncludeMenuItem.Click += RegionSpawnerInclude;
		regionSpawnerExcludeMenuItem.Click += RegionSpawnerExclude;
		configureTeleporterMenuItem.Click += ConfigureTeleporter;
		cancelConfigureTeleporterMenuItem.Click += (o, e) => { _presenter.ConfiguringTeleporter = null; };
		configureThisTeleporterMenuItem.Click += SetTeleporterAsConfiguring;

		_contextMenu.Items.Add(createSpawnMenuItem);
		_contextMenu.Items.Add(createLocationMenuItem);
		_contextMenu.Items.Add(createSubregionMenuItem);
        _contextMenu.Items.Add(SubregionIncludeMenuItem);
        _contextMenu.Items.Add(createRegionSpawnerMenuItem);
		_contextMenu.Items.Add(regionSpawnerIncludeMenuItem);
		_contextMenu.Items.Add(regionSpawnerExcludeMenuItem);
		_contextMenu.Items.Add(configureTeleporterMenuItem);
		_contextMenu.Items.Add(cancelConfigureTeleporterMenuItem);
		_contextMenu.Items.Add(configureThisTeleporterMenuItem);

		_pointContextItems.Add(createSpawnMenuItem);
		_pointContextItems.Add(createLocationMenuItem);
		_selectionContextItems.Add(createRegionSpawnerMenuItem);
		_selectionContextItems.Add(createSubregionMenuItem);
        _selectionContextItems.Add(SubregionIncludeMenuItem);
        _spawnerContextItems.Add(regionSpawnerIncludeMenuItem);
		_spawnerContextItems.Add(regionSpawnerExcludeMenuItem);
		_teleporterDestinationContextItems.Add(configureTeleporterMenuItem);
		_teleporterDestinationContextItems.Add(cancelConfigureTeleporterMenuItem);
		_teleporterSourceContextItems.Add(configureThisTeleporterMenuItem);
		
		var commentStream = System.Windows.Application.GetResourceStream(new Uri(@"pack://application:,,,/Kesmai.WorldForge;component/Resources/Comment-White.png")).Stream;
		
		_commentSprite = Texture2D.FromStream(GraphicsService.GraphicsDevice, commentStream);
	}

	public void Initialize()
	{
		var services = ServiceLocator.Current;
		var contentManager = services.GetInstance<ContentManager>();
		
		var theme = contentManager.Load<Theme>(@"UI\Theme");
		var renderer = new UIRenderer(GraphicsService.GraphicsDevice, theme);

		var uiManager = _worldPresentationTarget.UIManager;

		uiManager.Screens.Add(_uiScreen = new UIScreen($"{_worldPresentationTarget.GetHashCode()} GUI Screen", renderer)
		{
			Background = Color.Transparent,
			ZIndex = int.MaxValue,
		});
		
		_font = renderer.GetFontRenderer("Tahoma", 10);
		
		OnInitialize();
	}
	
	protected virtual void OnInitialize()
	{
	}

	private void CreateLocationSpawner(object sender, EventArgs args)
	{
		var region = _worldPresentationTarget.Region;
		var tile = ToWorldTile((int)Math.Floor(_contextMenu.ActualX), (int)Math.Floor(_contextMenu.ActualY));
		if (tile == null)
			return;
			
		var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
		var segment = segmentRequest.Response;
		if (segment == null)
			return;

		var newSpawner = new LocationSpawner()
		{
			Name = $"Location Spawn {tile.X}, {tile.Y} [{region.ID}]",
			X = tile.X,
			Y = tile.Y,
			Region = region.ID,

			MinimumDelay = TimeSpan.FromMinutes(15.0),
			MaximumDelay = TimeSpan.FromMinutes(15.0),
		};
		segment.Spawns.Location.Add(newSpawner);

		_selection.Select(new Rectangle(tile.X, tile.Y, 1, 1), region);
		_presenter.SwapDocument("Spawn");
	}

	private void CreateLocation(object sender, EventArgs args)
	{
		var region = _worldPresentationTarget.Region;
		var tile = ToWorldTile((int)Math.Floor(_contextMenu.ActualX), (int)Math.Floor(_contextMenu.ActualY));
		if (tile == null)
			return;

		var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
		var segment = segmentRequest.Response;
		if (segment == null)
			return;

		segment.Locations.Add(new SegmentLocation()
		{
			Name = $"Location {tile.X}, {tile.Y} [{region.ID}]",
			X = tile.X, Y = tile.Y, Region = region.ID,
		});

		_selection.Select(new Rectangle(tile.X, tile.Y, 1, 1), region);
		_presenter.SwapDocument("Location");

	}

	private void CreateRegionSpawner(object sender, EventArgs args)
	{
		var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
		var segment = segmentRequest.Response;
		if (segment is null)
			return;
		var newSpawner = new RegionSpawner()
		{
			Name = $"New {_worldPresentationTarget.Region.Name} Spawner",
			Region = _worldPresentationTarget.Region.ID,
			MinimumDelay = TimeSpan.FromMinutes(15.0),
			MaximumDelay = TimeSpan.FromMinutes(15.0),
		};
		newSpawner.Inclusions.Clear();
		foreach (Rectangle rect in _selection)
		{
			var bounds = new SegmentBounds((int)rect.Left, (int)rect.Top, (int)rect.Right-1, (int)rect.Bottom-1);
			newSpawner.Inclusions.Add(bounds);
		}
		segment.Spawns.Region.Add(newSpawner);

		Rectangle insideNewSpawner = new Rectangle(_selection.First().Left, _selection.First().Top, 1, 1);
		_selection.Select(insideNewSpawner, _worldPresentationTarget.Region);
		_presenter.SwapDocument("Spawn");
			
	}
	private void CreateSubregion(object sender, EventArgs args)
	{
		var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
		var segment = segmentRequest.Response;
		if (segment is null)
			return;

		var newSubRegion = new SegmentSubregion()
		{
			Name = $"New {_worldPresentationTarget.Region.Name} Subregion",
			Region = _worldPresentationTarget.Region.ID
		};
			
		foreach (Rectangle rect in _selection)
		{
			var bounds = new SegmentBounds((int)rect.Left, (int)rect.Top, (int)rect.Right - 1, (int)rect.Bottom - 1);
			newSubRegion.Rectangles.Add(bounds);
		}
		segment.Subregions.Add(newSubRegion);

		Rectangle insideNewSubregion = new Rectangle(_selection.First().Left, _selection.First().Top, 1, 1);
		_selection.Select(insideNewSubregion, _worldPresentationTarget.Region);
		_presenter.SwapDocument("Subregion");
	}
    
    private void SubregionInclude(object sender, EventArgs args)
    {
        var currentSubregion = (_presenter.ActiveDocument as UI.Documents.SubregionViewModel).SelectedSubregion;
        if (currentSubregion == null)
            return;
        foreach (Rectangle rect in _selection)
        {
            var bounds = new SegmentBounds((int)rect.Left, (int)rect.Top, (int)rect.Right - 1, (int)rect.Bottom - 1);
            currentSubregion.Rectangles.Add(bounds);
        }
        InvalidateRender();
    }
    private void RegionSpawnerInclude(object sender, EventArgs args)
	{
		var currentSpawner = (_presenter.ActiveDocument as UI.Documents.SpawnsViewModel).SelectedRegionSpawner;
		if (currentSpawner == null)
			return;
		foreach (Rectangle rect in _selection)
		{
			var bounds = new SegmentBounds((int)rect.Left, (int)rect.Top, (int)rect.Right - 1, (int)rect.Bottom - 1);
			currentSpawner.Inclusions.Add(bounds);
		}
		InvalidateRender();
	}
	private void RegionSpawnerExclude(object sender, EventArgs args)
	{
		var currentSpawner = (_presenter.ActiveDocument as UI.Documents.SpawnsViewModel).SelectedRegionSpawner;
		if (currentSpawner == null)
			return;
		foreach (Rectangle rect in _selection)
		{
			var bounds = new SegmentBounds((int)rect.Left, (int)rect.Top, (int)rect.Right - 1, (int)rect.Bottom - 1);
			currentSpawner.Exclusions.Add(bounds);
		}
		InvalidateRender();
	}
		
	private void ConfigureTeleporter(object sender, EventArgs args)
	{
		var region = _worldPresentationTarget.Region;
		var tile = ToWorldTile((int)Math.Floor(_contextMenu.ActualX), (int)Math.Floor(_contextMenu.ActualY));
		if (tile == null)
			return;

		_presenter.ConfiguringTeleporter.DestinationX = tile.X;
		_presenter.ConfiguringTeleporter.DestinationY = tile.Y;
		_presenter.ConfiguringTeleporter.DestinationRegion = _worldPresentationTarget.Region.ID;
		_presenter.ConfiguringTeleporter = null;
		InvalidateRender();
	}

	private void SetTeleporterAsConfiguring(object sender, EventArgs args)
	{
		var tile = ToWorldTile((int)Math.Floor(_contextMenu.ActualX), (int)Math.Floor(_contextMenu.ActualY));
		if (tile == null)
			return;
		var teleporter = tile.Components.FirstOrDefault(t => t is TeleportComponent);
		_presenter.ConfiguringTeleporter = teleporter as TeleportComponent;
	}
	
	protected override void OnUpdate(TimeSpan deltaTime)
	{
		base.OnUpdate(deltaTime);
		
		var region = _worldPresentationTarget.Region;
		
		// retrieve the current active tool and update the cursor.
		var selectedTool = _presenter.SelectedTool;

		if (selectedTool != null)
			_worldPresentationTarget.Cursor = selectedTool.Cursor;
		
		// process input here, input not processed similar to a <see cref="UIControl" />.
		// we access the input manager from the presenter, and follow the same pattern.
		var inputManager = PresentationTarget.InputManager;
		
		if (inputManager is null)
			return;
		
		// track if the mouse is over the control.
		_isMouseOver = _worldPresentationTarget.IsMouseOver;
		_isMouseDirectlyOver = _isMouseOver;

		// only process input if the mouse is within the control.
		if (!_isMouseOver)
			return;
		
		// next check if the mouse is over a UI control.
		if (_uiScreen != null && _uiScreen.ControlDirectlyUnderMouse != null)
		{
			var controlType = _uiScreen.ControlDirectlyUnderMouse.GetType();

			// if the control is not the UIScreen, then the mouse is over a UI control.
			if (controlType != typeof(UIScreen))
				_isMouseDirectlyOver = false;
		}

		// we do not want to process input in this case, as it is being handled by the control.
		if (!_isMouseDirectlyOver)
			return;
		
		// process mouse/touch input.
		if (!inputManager.IsMouseOrTouchHandled)
		{
			// give the selected tool the first chance to handle input.
			if (selectedTool != null)
				selectedTool.OnHandleInput(_worldPresentationTarget, inputManager);
		}

		// if the input is still not handled, we can check for context menu and zoom.
		if (!inputManager.IsMouseOrTouchHandled)
		{
			if (_contextMenu != null && (_contextMenu.IsEnabled && _contextMenu.Items.Count > 0))
			{
				// TODO: (Workspaces) Is there a better way to do this?
				if (inputManager.IsReleased(MouseButtons.Right))
				{
					var currentPosition = inputManager.MousePosition;
					var (cx, cy) = this.ToWorldCoordinates((int)currentPosition.X, (int)currentPosition.Y);
					bool isInSelection = _selection.IsSelected(cx, cy, region);
					bool isInRegionSpawner = false;
					bool configuringTeleporter = _presenter.ConfiguringTeleporter is not null;
					
					bool tileHasOneTeleporter = false;
					
					if (region.GetTile(cx, cy) is SegmentTile tile && tile.Components.Where(c => c is TeleportComponent) is IEnumerable<TerrainComponent> teleporter && teleporter.Count() == 1)
						tileHasOneTeleporter = true;
                   
					if (_presenter.ActiveDocument is UI.Documents.SpawnsViewModel)
					{
						var response = WeakReferenceMessenger.Default.Send<Kesmai.WorldForge.UI.Documents.SpawnsDocument.GetCurrentTypeSelection>();
						
						if (response.HasReceivedResponse)
							isInRegionSpawner = response.Response == 1;
					}
					
					foreach (MenuItem item in _pointContextItems) { item.IsVisible = !configuringTeleporter &&(!isInSelection || _selection.FirstOrDefault() is { Height:1, Width:1 }); }
					foreach (MenuItem item in _selectionContextItems) { item.IsVisible = !configuringTeleporter	&& isInSelection; }
					foreach (MenuItem item in _spawnerContextItems) { item.IsVisible = !configuringTeleporter && isInRegionSpawner; }
					foreach (MenuItem item in _teleporterDestinationContextItems) { item.IsVisible = configuringTeleporter; }
					foreach (MenuItem item in _teleporterSourceContextItems) { item.IsVisible = !configuringTeleporter && tileHasOneTeleporter; }
					
					inputManager.IsMouseOrTouchHandled = true;
					
					_contextMenu.Open(_uiScreen, inputManager.MousePosition);
				}
			}
			
			if (inputManager.MouseWheelDelta != 0)
			{
				ZoomFactor += 0.2f * Math.Sign(inputManager.MouseWheelDelta);
				
				inputManager.IsMouseOrTouchHandled = true;
			}
		}

		if (inputManager.IsKeyboardHandled)
			return;
		
		// process keyboard prior to mouse/touch.
		var multiplier = 3;

		if (inputManager.IsDown(Keys.LeftShift) || inputManager.IsDown(Keys.RightShift))
			multiplier = 7;

		void shiftMap(int dx, int dy)
		{
			CameraLocation += new Vector2F(dx * multiplier, dy * multiplier);
			inputManager.IsKeyboardHandled = true;
		}
		
		if (inputManager.IsPressed(Keys.W, true))
		{
			shiftMap(0, -1);
		}
		else if (inputManager.IsPressed(Keys.S, true))
		{
			shiftMap(0, 1);
		}
		else if (inputManager.IsPressed(Keys.A, true))
		{
			shiftMap(-1, 0);
		}
		else if (inputManager.IsPressed(Keys.D, true))
		{
			shiftMap(1, 0);
		}
		else if (inputManager.IsPressed(Keys.Home, false))
		{
			CenterCameraOn(0, 0);

			if (_selection != null)
				_selection.Select(new Rectangle(0, 0, 1, 1), region);
		}
		else if (inputManager.IsPressed(Keys.Back, false))
		{
			_presenter.JumpPrevious();
		}
		else if (inputManager.IsPressed(Keys.Add, false))
		{
			ZoomFactor += 0.2f;
		}
		else if (inputManager.IsPressed(Keys.Subtract, false))
		{
			ZoomFactor -= 0.2f;
		}
		else if (inputManager.IsReleased(Keys.Delete))
		{
			if (region != null)
			{
				foreach (var area in _selection)
				{
					for (var x = area.Left; x < area.Right; x++)
					for (var y = area.Top; y < area.Bottom; y++)
					{
						var currentFilter = _presenter.SelectedFilter;
						var tile = region.GetTile(x, y);
						if (tile is null)
							continue;
						var validComponents = tile.Components.Where(c => currentFilter.IsValid(c)).ToArray();
						foreach (var component in validComponents)
						{
							tile.RemoveComponent(component);
						}
					}

				}

				_invalidateRender = true;
				inputManager.IsKeyboardHandled = true;
			}
		}
		else if (inputManager.IsPressed(Keys.Multiply, false))
		{
			_drawgrid = !_drawgrid;
			_invalidateRender = true;
		}
		else
		{
			if (!inputManager.IsKeyboardHandled)
			{
				foreach (var selectorKey in _selectorKeys)
				{
					if (!inputManager.IsReleased(selectorKey))
						continue;

					var index = _selectorKeys.IndexOf(selectorKey);
					var filters = _presenter.Filters;

					if (index >= 0 && index < filters.Count)
						_presenter.SelectFilter(filters[index]);

					inputManager.IsKeyboardHandled = true;
				}
			}

			if (!inputManager.IsKeyboardHandled)
			{
				foreach (var toolKey in _toolKeys)
				{
					if (!inputManager.IsReleased(toolKey))
						continue;

					var index = _toolKeys.IndexOf(toolKey);
					var tools = _presenter.Tools;

					if (index >= 0 && index < tools.Count)
						_presenter.SelectTool(tools[index]);

					inputManager.IsKeyboardHandled = true;
				}
			}
		}
	}

	public void InvalidateRender()
	{
		_invalidateRender = true;
    }

	public Rectangle GetRenderRectangle(Rectangle viewRectangle, Rectangle inlay, int offset) {
		var ox = inlay.Left - viewRectangle.Left;
		var oy = inlay.Top - viewRectangle.Top;

		var x = (int)Math.Floor(ox * _presenter.UnitSize * _zoomFactor) + (offset * 2) + 1;
		var y = (int)Math.Floor(oy * _presenter.UnitSize * _zoomFactor) + (offset * 2) + 1;

		var width = (int)Math.Floor(inlay.Width * _presenter.UnitSize * _zoomFactor) - (offset * 4) - 2;
		var height = (int)Math.Floor(inlay.Height * _presenter.UnitSize * _zoomFactor) - (offset * 4) - 2;

		var bounds = new Rectangle(x, y, width, height);
		return bounds;
	}
	
	public Rectangle GetRenderRectangle(Rectangle viewRectangle, Rectangle inlay) {
		var ox = inlay.Left - viewRectangle.Left;
		var oy = inlay.Top - viewRectangle.Top;

		var x = (int)Math.Floor(ox * _presenter.UnitSize * _zoomFactor) + 1;
		var y = (int)Math.Floor(oy * _presenter.UnitSize * _zoomFactor) + 1;

		var width = (int)Math.Floor(inlay.Width * _presenter.UnitSize * _zoomFactor) - 2;
		var height = (int)Math.Floor(inlay.Height * _presenter.UnitSize * _zoomFactor) - 2;

		var bounds = new Rectangle(x, y, width, height);
		return bounds;
	}
	public Rectangle GetRenderRectangle(Rectangle viewRectangle, int x, int y)
	{
		var rx = (x - viewRectangle.Left) * (int)Math.Floor(_presenter.UnitSize * _zoomFactor) + 1;
		var ry = (y - viewRectangle.Top) * (int)Math.Floor(_presenter.UnitSize * _zoomFactor) + 1;
		var bounds = new Rectangle(rx, ry,
			(int)Math.Floor(_presenter.UnitSize * _zoomFactor) - 2, (int)Math.Floor(_presenter.UnitSize * _zoomFactor) - 2);
		return bounds;
	}

	protected override void OnRender(RenderContext context)
    {
        var graphicsService = context.GraphicsService;
		var graphicsDevice = graphicsService.GraphicsDevice;
		var spritebatch = graphicsService.GetSpriteBatch();
		var viewRectangle = GetViewRectangle();
		var selection = _selection;

		graphicsDevice.Clear(Color.Black);
			
		spritebatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);
			
		var presentation = _worldPresentationTarget;
		var region = presentation.Region;
			
		if (region != default(SegmentRegion))
		{
			var oldTargets = graphicsService.GraphicsDevice.GetRenderTargets();

			if (oldTargets.Length > 0 && oldTargets[0].RenderTarget is RenderTarget2D oldTarget)
			{
				if (oldTarget.Width != _renderTarget.Width || oldTarget.Height != _renderTarget.Height)
					OnSizeChanged(oldTarget.Width, oldTarget.Height);
			}
				
			viewRectangle = GetViewRectangle();

			if (_invalidateRender)
			{
				graphicsService.GraphicsDevice.SetRenderTarget(_renderTarget);

				OnBeforeRender(spritebatch);
					
				for (var vx = viewRectangle.Left; vx <= viewRectangle.Right; vx++)
				for (var vy = viewRectangle.Top; vy <= viewRectangle.Bottom; vy++)
				{
					var rx = (vx - viewRectangle.Left) * (int)Math.Floor(_presenter.UnitSize * _zoomFactor);
					var ry = (vy - viewRectangle.Top) * (int)Math.Floor(_presenter.UnitSize * _zoomFactor);
						
					var segmentTile = region.GetTile(vx, vy);

					if (segmentTile != default(SegmentTile))
					{
						var tileBounds = new Rectangle(rx, ry,
							(int)Math.Floor(_presenter.UnitSize * _zoomFactor), (int)Math.Floor(_presenter.UnitSize * _zoomFactor));
						var originalBounds = new Rectangle((int)Math.Floor(tileBounds.X - (45*_zoomFactor)), (int)Math.Floor(tileBounds.Y - (45*_zoomFactor)),
							(int)Math.Floor(100 * _zoomFactor), (int)Math.Floor(100 * _zoomFactor));

						var renders = segmentTile.Renders;
							
						foreach (var render in renders)
						{
							var sprite = render.Layer.Sprite;

							if (sprite != null)
							{
								var spriteBounds = originalBounds;

								if (sprite.Offset != Vector2F.Zero)
									spriteBounds.Offset((int)Math.Floor(sprite.Offset.X * _zoomFactor), (int)Math.Floor(sprite.Offset.Y * _zoomFactor));

								spritebatch.Draw(sprite.Texture, spriteBounds.Location.ToVector2(), null, render.Color, 0, Vector2.Zero, _zoomFactor / sprite.Resolution, SpriteEffects.None, 0f);
							}
						}


						OnRenderTile(spritebatch, segmentTile, tileBounds);
					}
				}

				OnAfterRender(spritebatch);

				var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
				var segment = segmentRequest.Response;
				if (_presenter.Visibility.ShowComments ||_presenter.Visibility.ShowTeleporters||(_presenter.Visibility.ShowSpawns && _presenter.ActiveDocument is not WorldForge.UI.Documents.SpawnsViewModel))
				{
					//dim the screen
					var viewportrectangle = GetRenderRectangle(viewRectangle, viewRectangle);
					spritebatch.FillRectangle(viewportrectangle, Color.FromNonPremultiplied(0, 0, 0, 128));
				}
				var _drawStrings = new List<Tuple<String, Vector2, Color>>();
				if (_presenter.Visibility.ShowTeleporters) // Destination highlights for teleporters //todo: make this a toggle with a "tool" like button
				{
					var _teleportDestinationHighlight = Color.FromNonPremultiplied(80, 255, 80, 200);
					var _teleportSourceHighlight = Color.FromNonPremultiplied(160, 255, 20, 200);
					var _teleportSelectedHighlight = Color.FromNonPremultiplied(255, 255, 80, 255);
					var _destinationCounter = new System.Collections.Hashtable();
					int porterCount;
					foreach (SegmentRegion searchRegion in segment.Regions) //for each region in the segment
					{
						foreach (SegmentTile tile in searchRegion.GetTiles((t) => { return t.GetComponents<TeleportComponent>().Any(); })) // for each tile in the region that is a teleporter:
						{
							var highlight = selection.IsSelected(tile.X, tile.Y, searchRegion) ? _teleportSelectedHighlight : _teleportDestinationHighlight; // use a 'destination' color, but override to the selected color if this tile is in a selection

							foreach (TeleportComponent component in tile.GetComponents<TeleportComponent>(t => t is TeleportComponent && t.DestinationRegion == region.ID && (t.DestinationX!= 0 || t.DestinationY!=0))) // for each Teleportcomponent on that tile where the destination is this region
							{
								var _mx = component.DestinationX;
								var _my = component.DestinationY;
								if (viewRectangle.Contains(_mx, _my))
								{
									if (!_destinationCounter.ContainsKey($"{_mx}{_my}"))
									{
										_destinationCounter.Add($"{_mx}{_my}", 0);
										porterCount = 0;
									} 
									else
									{
										_destinationCounter[$"{_mx}{_my}"] = (int)_destinationCounter[$"{_mx}{_my}"] + 1;
										porterCount = (int)_destinationCounter[$"{_mx}{_my}"];
									}

									var bounds = GetRenderRectangle(viewRectangle, _mx, _my);
									var innerRectangle = new Rectangle(bounds.Left, bounds.Top + 16*porterCount, (int)Math.Floor(55 *_zoomFactor), 16);

									spritebatch.DrawRectangle(bounds, highlight);
									spritebatch.FillRectangle(innerRectangle, highlight);
										
									_drawStrings.Add(Tuple.Create($"{searchRegion.Name}", new Vector2(bounds.Left + 2, bounds.Top + 2 + 16 * porterCount), Color.Black));
								}
							}
						}
					}
					foreach (SegmentTile tile in region.GetTiles(t => { return t.GetComponents<TeleportComponent>().Any(); })) // for each tile in this region that has a teleportcomponent
					{

						foreach (TeleportComponent component in tile.GetComponents<TeleportComponent>(t=> t.DestinationX !=0 || t.DestinationY!=0)) // for each of those Teleportcomponents
						{
							var _mx = tile.X;
							var _my = tile.Y;
							var targetRegionName = "Invalid";
							var targetregion = segment.GetRegion(component.DestinationRegion);
							if (targetregion is not null) { targetRegionName = targetregion.Name; }

							var highlight = selection.IsSelected(component.DestinationX, component.DestinationY, targetregion) ? _teleportSelectedHighlight : _teleportSourceHighlight;

							if (viewRectangle.Contains(_mx,_my))
							{

								var bounds = GetRenderRectangle(viewRectangle, _mx, _my);
								var innerRectangle = new Rectangle(bounds.Left, bounds.Bottom - 16, (int)Math.Floor(55 * _zoomFactor), 16);

								spritebatch.FillRectangle(innerRectangle, highlight);
	
								_drawStrings.Add(Tuple.Create($"{targetRegionName}", new Vector2(bounds.Left + 2, bounds.Bottom - 14), Color.Black));
							}
						}

					}
				}
				if (_presenter.Visibility.ShowSpawns && _presenter.ActiveDocument is not WorldForge.UI.Documents.SpawnsViewModel)
				{
					var _inclusionBorder = Color.FromNonPremultiplied(200, 255, 50, 200);
					var _inclusionFill = Color.FromNonPremultiplied(200, 255, 50, 50);
					var _exclusionBorder = Color.FromNonPremultiplied(255, 0, 0, 200);
					var _exclusionFill = Color.FromNonPremultiplied(50, 50, 50, 200);
					var _locationBorder = Color.FromNonPremultiplied(0, 255, 255, 200);
					var _textAtLocation = new Dictionary<(int, int), int>();

					//doing exclusions first for visibility. They are very opaque so obscure anything below
					foreach (RegionSpawner r in segment.Spawns.Region.Where<RegionSpawner>(r => r.Region == region.ID))
					{
						foreach (SegmentBounds exc in r.Exclusions)
						{
							if (exc is { Left: 0, Right: 0, Top: 0, Bottom: 0 } || !viewRectangle.Intersects(exc.ToRectangle())) { continue; }

							var bounds = GetRenderRectangle(viewRectangle, exc.ToRectangle());

							spritebatch.FillRectangle(bounds, _exclusionFill);
							spritebatch.DrawRectangle(bounds, _exclusionBorder);

							if (_textAtLocation.ContainsKey((exc.Left, exc.Top)))
							{
								_textAtLocation[(exc.Left, exc.Top)] += 1;
							}
							else
							{
								_textAtLocation.Add((exc.Left, exc.Top), 0);
							}

							_drawStrings.Add(Tuple.Create($"Exclusion {r.Name}", new Vector2(bounds.Left + 2, bounds.Top + 2 + (_textAtLocation[(exc.Left, exc.Top)] * 15)), Color.Red));
						}
					}
					foreach (LocationSpawner l in segment.Spawns.Location.Where<LocationSpawner>(l => l.Region == region.ID))
					{
						var _mx = l.X;
						var _my = l.Y;
						if (viewRectangle.Contains(_mx,_my))
						{
							if (_textAtLocation.ContainsKey((_mx, _my)))
							{
								_textAtLocation[(_mx, _my)] += 1;
							}
							else
							{
								_textAtLocation.Add((_mx, _my), 0);
							}

							var bounds = GetRenderRectangle(viewRectangle, _mx, _my);
							var innerRectangle = new Rectangle(bounds.Left, bounds.Top, (int)Math.Floor(55 * _zoomFactor) - (_textAtLocation[(_mx, _my)] * 2) - 2, 16);

							spritebatch.DrawRectangle(bounds, _locationBorder);
							spritebatch.FillRectangle(innerRectangle, _locationBorder);

							_drawStrings.Add(Tuple.Create($"{l.Name}", new Vector2(bounds.Left + 2, bounds.Top + 2 + (_textAtLocation[(_mx, _my)] * 15)), Color.Black));

						}
					}
					foreach (RegionSpawner r in segment.Spawns.Region.Where<RegionSpawner>(r => r.Region == region.ID))
					{
						foreach (SegmentBounds inc in r.Inclusions)
						{
							if (!viewRectangle.Intersects(inc.ToRectangle())) { continue; }

							if (_textAtLocation.ContainsKey((inc.Left, inc.Top)))
							{
								_textAtLocation[(inc.Left, inc.Top)] += 1;
							}
							else
							{
								_textAtLocation.Add((inc.Left, inc.Top), 0);
							}
							var offset = _textAtLocation[(inc.Left, inc.Top)];

							var bounds = GetRenderRectangle(viewRectangle, inc.ToRectangle(), offset);

							spritebatch.DrawRectangle(bounds, _inclusionBorder);
							spritebatch.FillRectangle(bounds, _inclusionFill);
						}
					}
				}
				//draw all strings on top..
				foreach (Tuple<String, Vector2, Color> _drawString in _drawStrings)
				{
					_font.DrawString(spritebatch, RenderTransform.Identity, _drawString.Item1, _drawString.Item2, _drawString.Item3);
				}
				if (_presenter.Visibility.ShowComments)
				{
					var visibleComments = region.GetTiles().Where(t => t.Components.Any(c => !string.IsNullOrWhiteSpace(c.Comment)) && viewRectangle.Contains(t.X, t.Y));

					var spriteWidth = 32 * _zoomFactor;

					foreach (var tile in visibleComments) { //todo: this looks hideous. Find something better.
						var bounds = GetRenderRectangle(viewRectangle, tile.X, tile.Y);
						bounds.X = (int)Math.Floor(bounds.Right - spriteWidth);
						bounds.Width = (int)Math.Floor(spriteWidth);
						bounds.Height = (int)Math.Floor(spriteWidth);
						spritebatch.Draw(_commentSprite, bounds, Color.Aqua);

					}
				}
			}
				
			graphicsService.GraphicsDevice.SetRenderTargets(oldTargets);
				
			spritebatch.Draw(_renderTarget, Vector2.Zero, Color.White);
		}

		if (selection.Region == region)
		{
			foreach (var rectangle in selection)
			{
				if (!viewRectangle.Intersects(rectangle))
					continue;

				// if (rectangle.X == 3 && rectangle.Y == 3) { var breakpoint = true; } removed because breakpoint not used

				var bounds = GetRenderRectangle(viewRectangle, rectangle);

				spritebatch.FillRectangle(bounds, _selectionFill);
				spritebatch.DrawRectangle(bounds, _selectionBorder);
			}
		}

		if (_isMouseOver)
		{
			if (_presenter.SelectedTool != null)
				_presenter.SelectedTool.OnRender(context);
		}

		if (_uiScreen != null)
			_uiScreen.Draw(context.DeltaTime);
				
		_invalidateRender = false;
			
		spritebatch.End();
	}

	public virtual void OnRenderTile(SpriteBatch spritebatch, SegmentTile tile, Rectangle bounds)
	{
	}

	protected virtual void OnBeforeRender(SpriteBatch spriteBatch)
	{
	}

	protected virtual void OnAfterRender(SpriteBatch spriteBatch)
	{
		var viewRectangle = GetViewRectangle();

		if (_drawgrid) {
			for (var vx = viewRectangle.Left; vx <= viewRectangle.Right; vx++)
			for (var vy = viewRectangle.Top; vy <= viewRectangle.Bottom; vy++)
			{
				var dx = vx % 10;
				var dy = vy % 10;

				if (dx != 0 || dy != 0)
					continue;

				var bounds = GetRenderRectangle(viewRectangle, vx, vy);
				var innerRectangle = new Rectangle(bounds.Left, bounds.Top, 55, 20);

				spriteBatch.DrawRectangle(bounds, _selectionBorder);
				spriteBatch.FillRectangle(innerRectangle, _selectionBorder);

				_font.DrawString(spriteBatch, RenderTransform.Identity, $"{vx}, {vy}",
					new Vector2(bounds.Left + 4, bounds.Top + 3), Color.Black);
			}
		}
	}

	public void OnSizeChanged(int width, int height)
	{
		var services = ServiceLocator.Current;
		var graphicsService = services.GetInstance<IGraphicsService>();

		width = Math.Max(1, Math.Min(width, 4096));
		height = Math.Max(1, Math.Min(height, 16384));
			
		if (_uiScreen != null)
		{
			_uiScreen.Width = width;
			_uiScreen.Height = height;
		}
			
		_renderTarget = new RenderTarget2D(graphicsService.GraphicsDevice, width, height);
		_invalidateRender = true;
	}

	public Rectangle GetViewRectangle()
	{
		var actualX = _cameraLocation.X;
		var actualY = _cameraLocation.Y;
			
		if (_cameraDrag != Vector2F.Zero)
		{
			actualX -= (_cameraDrag.X / (_presenter.UnitSize * _zoomFactor));
			actualY -= (_cameraDrag.Y / (_presenter.UnitSize * _zoomFactor));
		}
			
		return new Rectangle((int)actualX, (int)actualY,
			(int)Math.Floor(_renderTarget.Width / (_presenter.UnitSize * _zoomFactor)) + 1,
			(int)Math.Floor(_renderTarget.Height / (_presenter.UnitSize * _zoomFactor)) + 1);
	}

	public (int wx, int wy) ToWorldCoordinates(int mx, int my)
	{
		return ((int)_cameraLocation.X + (int)Math.Floor((mx - _cameraDrag.X) / (_presenter.UnitSize * _zoomFactor)), 
			(int)_cameraLocation.Y + (int)Math.Floor((my - _cameraDrag.Y) / (_presenter.UnitSize * _zoomFactor)));
	}

	public SegmentTile ToWorldTile(int mx, int my)
	{
		var (wx, wy) = ToWorldCoordinates(mx, my);
		var region = _worldPresentationTarget.Region;

		if (region != null)
			return region.GetTile(wx, wy);

		return default(SegmentTile);
	}

	public Rectangle ToScreenBounds(int wx, int wy)
	{
		return new Rectangle(
			(wx - (int)_cameraLocation.X) * ((int)Math.Floor(_presenter.UnitSize * _zoomFactor)), 
			(wy - (int)_cameraLocation.Y) * ((int)Math.Floor(_presenter.UnitSize * _zoomFactor)), 
			_presenter.UnitSize, 
			_presenter.UnitSize);
	}

	public void CenterCameraOn(int mx, int my)
	{
		// get the display width and height
		var displayWidth = _renderTarget.Width;
		var displayHeight = _renderTarget.Height;
		
		// calculate the unit width and height
		var unitWidth = _presenter.UnitSize * _zoomFactor;
		var unitHeight = _presenter.UnitSize * _zoomFactor;
		
		var mapWidth = (displayWidth / unitWidth);
		var mapHeight = (displayHeight / unitHeight);
		
		// calculate the offset
		var ox = (int)Math.Floor(mapWidth / 2);
		var oy = (int)Math.Floor(mapHeight / 2);
		
		CameraLocation = new Vector2F(mx, my) - new Vector2F(ox, oy);
			
		InvalidateRender();
	}
}