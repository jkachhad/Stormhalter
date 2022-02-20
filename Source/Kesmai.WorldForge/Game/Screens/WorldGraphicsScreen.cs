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
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kesmai.WorldForge
{
	public class SubregionsGraphicsScreen : WorldGraphicsScreen
	{
		private SegmentSubregion _subregion;
		
		public SubregionsGraphicsScreen(PresentationTarget presentationTarget, IUIService uiService, IGraphicsService graphicsService) : base(presentationTarget, uiService, graphicsService)
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

					spriteBatch.DrawString(_font, _subregion.Name,
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

		public SpawnsGraphicsScreen(PresentationTarget presentationTarget, IUIService uiService, IGraphicsService graphicsService) : base(presentationTarget, uiService, graphicsService)
		{
		}

		public void SetSpawner(Spawner spawner)
		{
			_spawner = spawner;
			if (_spawner is LocationSpawner ls)
            {
				CenterCameraOn(ls.X, ls.Y);
            }
			if (_spawner is RegionSpawner rs)
			{
				var inclusion = rs.Inclusions.FirstOrDefault();
				if (inclusion != null)
					
					CenterCameraOn((int)(inclusion.Left+inclusion.Width/2), (int)(inclusion.Top+inclusion.Height/2));
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

					spriteBatch.DrawString(_font, _spawner.Name,
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

					spriteBatch.DrawString(_font, "Exclusion",
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

					spriteBatch.DrawString(_font, $"{_mx}, {_my}",
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
		
		public LocationsGraphicsScreen(PresentationTarget presentationTarget, IUIService uiService, IGraphicsService graphicsService) : base(presentationTarget, uiService, graphicsService)
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
				
				spriteBatch.DrawString(_font, $"{_mx}, {_my}", 
					new Vector2(bounds.Left + 4, bounds.Top + 3), Color.Black);
			}
		}
	}

	public class WorldGraphicsScreen : GraphicsScreen
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

		private PresentationTarget _presentationTarget;
		protected UIScreen _uiScreen;
		private ContextMenu _contextMenu;
		protected BitmapFont _font;
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

		public int Width => (int)_presentationTarget.ActualWidth;
		public int Height => (int)_presentationTarget.ActualHeight;

		public UIScreen UI => _uiScreen;

		public WorldGraphicsScreen(PresentationTarget presentationTarget, 
			IUIService uiService, IGraphicsService graphicsService) : base(graphicsService)
		{
			_presentationTarget = presentationTarget;

			var services = (ServiceContainer)ServiceLocator.Current;

			_presenter = services.GetInstance<ApplicationPresenter>();
			_selection = _presenter.Selection;

			var contentManager = services.GetInstance<ContentManager>();
			var graphicsDevice = GraphicsService.GraphicsDevice;

			_renderTarget = new RenderTarget2D(graphicsDevice, 640, 480);

			var theme = contentManager.Load<Theme>(@"UI\Theme");
			var renderer = new UIRenderer(graphicsDevice, theme);

			_uiScreen = new UIScreen($"{presentationTarget.GetHashCode()} GUI Screen", renderer)
			{
				Background = Color.Transparent,
				ZIndex = int.MaxValue,
			};
			_uiScreen.InputProcessed += HandleInput;

			_contextMenu = new ContextMenu();

			var createSpawnMenuItem = new MenuItem() { Title = "Create Location Spawner.." };
			var createRegionSpawnerMenuItem = new MenuItem() { Title = "Create Region Spawner.." };
			var createSubregionMenuItem = new MenuItem() { Title = "Create Subregion.." };
			var createLocationMenuItem = new MenuItem() { Title = "Create Named Location.." };
			var RegionSpawnerIncludeMenuItem = new MenuItem() { Title = "Add selection to Inclusions.." };
			var RegionSpawnerExcludeMenuItem = new MenuItem() { Title = "Add selection to Exclusions.." };
			var configureTeleporterMenuItem = new MenuItem() { Title = "Set as Teleporter Destination..", IsVisible = false };
			var cancelConfigureTeleporterMenuItem = new MenuItem() { Title = "Cancel", IsVisible = false };
			var configureThisTeleporterMenuItem = new MenuItem() { Title = "Choose a Destination..", IsVisible = false };


			createSpawnMenuItem.Click += CreateLocationSpawner;
			createLocationMenuItem.Click += CreateLocation;
			createRegionSpawnerMenuItem.Click += CreateRegionSpawner;
			createSubregionMenuItem.Click += CreateSubregion;
			RegionSpawnerIncludeMenuItem.Click += RegionSpawnerInclude;
			RegionSpawnerExcludeMenuItem.Click += RegionSpawnerExclude;
			configureTeleporterMenuItem.Click += ConfigureTeleporter;
			cancelConfigureTeleporterMenuItem.Click += (o, e) => { _presenter.ConfiguringTeleporter = null; };
			configureThisTeleporterMenuItem.Click += SetTeleporterAsConfiguring;

			_contextMenu.Items.Add(createSpawnMenuItem);
			_contextMenu.Items.Add(createLocationMenuItem);
			_contextMenu.Items.Add(createSubregionMenuItem);
			_contextMenu.Items.Add(createRegionSpawnerMenuItem);
			_contextMenu.Items.Add(RegionSpawnerIncludeMenuItem);
			_contextMenu.Items.Add(RegionSpawnerExcludeMenuItem);
			_contextMenu.Items.Add(configureTeleporterMenuItem);
			_contextMenu.Items.Add(cancelConfigureTeleporterMenuItem);
			_contextMenu.Items.Add(configureThisTeleporterMenuItem);

			_pointContextItems.Add(createSpawnMenuItem);
			_pointContextItems.Add(createLocationMenuItem);
			_selectionContextItems.Add(createRegionSpawnerMenuItem);
			_selectionContextItems.Add(createSubregionMenuItem);
			_spawnerContextItems.Add(RegionSpawnerIncludeMenuItem);
			_spawnerContextItems.Add(RegionSpawnerExcludeMenuItem);
			_teleporterDestinationContextItems.Add(configureTeleporterMenuItem);
			_teleporterDestinationContextItems.Add(cancelConfigureTeleporterMenuItem);
			_teleporterSourceContextItems.Add(configureThisTeleporterMenuItem);

			uiService.Screens.Add(_uiScreen);

			_font = renderer.GetFont("Tahoma14Bold");

			var commentStream = System.Windows.Application.GetResourceStream(new Uri(@"pack://application:,,,/Kesmai.WorldForge;component/Resources/Comment-White.png")).Stream;
			_commentSprite = Texture2D.FromStream(graphicsDevice, commentStream);
		}

		public virtual void Initialize()
		{
		}

		private void CreateLocationSpawner(object sender, EventArgs args)
		{
			var region = _presentationTarget.Region;
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
			var region = _presentationTarget.Region;
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
				Name = $"New {_presentationTarget.Region.Name} Spawner",
				Region = _presentationTarget.Region.ID,
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
			_selection.Select(insideNewSpawner, _presentationTarget.Region);
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
				Name = $"New {_presentationTarget.Region.Name} Subregion",
				Region = _presentationTarget.Region.ID
			};
			
			foreach (Rectangle rect in _selection)
			{
				var bounds = new SegmentBounds((int)rect.Left, (int)rect.Top, (int)rect.Right - 1, (int)rect.Bottom - 1);
				newSubRegion.Rectangles.Add(bounds);
			}
			segment.Subregions.Add(newSubRegion);

			Rectangle insideNewSubregion = new Rectangle(_selection.First().Left, _selection.First().Top, 1, 1);
			_selection.Select(insideNewSubregion, _presentationTarget.Region);
			_presenter.SwapDocument("Subregion");
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
			var region = _presentationTarget.Region;
			var tile = ToWorldTile((int)Math.Floor(_contextMenu.ActualX), (int)Math.Floor(_contextMenu.ActualY));
			if (tile == null)
				return;

			_presenter.ConfiguringTeleporter.DestinationX = tile.X;
			_presenter.ConfiguringTeleporter.DestinationY = tile.Y;
			_presenter.ConfiguringTeleporter.DestinationRegion = _presentationTarget.Region.ID;
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
		protected virtual void OnHandleInput(object sender, InputEventArgs args)
		{
		}

		private void HandleInput(object sender, InputEventArgs args)
		{
			_isMouseOver = false;

			var inputService = _uiScreen.InputService;
			var region = _presentationTarget.Region;

			if (inputService == null || inputService.IsMouseOrTouchHandled || region is null)
				return;

			var selectedTool = _presenter.SelectedTool;

			if (selectedTool != null)
				_presentationTarget.Cursor = selectedTool.Cursor;

			_isMouseOver = _presentationTarget.IsMouseOver;
			_isMouseDirectlyOver = _isMouseOver;

			if (!_isMouseOver)
				return;

			if (_uiScreen != null && _uiScreen.ControlUnderMouse != null)
			{
				var controlType = _uiScreen.ControlUnderMouse.GetType();

				if (controlType != typeof(UIScreen))
					_isMouseDirectlyOver = false;
			}

			if (!_isMouseDirectlyOver)
				return;

			OnHandleInput(sender, args);

			if (!inputService.IsMouseOrTouchHandled)
			{
				if (selectedTool != null)
					selectedTool.OnHandleInput(_presentationTarget, inputService);
			}

			if (!inputService.IsMouseOrTouchHandled)
			{
				if (_contextMenu != null && (_contextMenu.IsEnabled && _contextMenu.Items.Count > 0))
				{
					if (inputService.IsReleased(MouseButtons.Right))
					{

						var currentPosition = inputService.MousePosition;
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
						inputService.IsMouseOrTouchHandled = true;
						_contextMenu.Open(_uiScreen, args.Context.MousePosition);
					}
				}
				if (inputService.MouseWheelDelta != 0)
				{
					ZoomFactor += 0.2f * Math.Sign(inputService.MouseWheelDelta);
					inputService.IsMouseOrTouchHandled = true;
				}
			}

			/* Map Shift */
			var multiplier = 3;

			if (inputService.IsDown(Keys.LeftShift) || inputService.IsDown(Keys.RightShift))
				multiplier = 7;

			void shiftMap(int dx, int dy)
			{
				CameraLocation += new Vector2F(dx * multiplier, dy * multiplier);
				inputService.IsKeyboardHandled = true;
			}

			if (!(inputService.IsDown(Keys.LeftControl) || inputService.IsDown(Keys.RightControl)))
			{
				if (inputService.IsPressed(Keys.W, true))
				{
					shiftMap(0, -1);
				}
				else if (inputService.IsPressed(Keys.S, true))
				{
					shiftMap(0, 1);
				}
				else if (inputService.IsPressed(Keys.A, true))
				{
					shiftMap(-1, 0);
				}
				else if (inputService.IsPressed(Keys.D, true))
				{
					shiftMap(1, 0);
				}
				else if (inputService.IsPressed(Keys.Home, false))
				{
					CenterCameraOn(0, 0);

					if (_selection != null)
						_selection.Select(new Rectangle(0, 0, 1, 1), region);
				}
				else if (inputService.IsPressed(Keys.Back, false))
				{
					_presenter.JumpPrevious();
				}
				else if (inputService.IsPressed(Keys.Add, false))
				{
					ZoomFactor += 0.2f;
				}
				else if (inputService.IsPressed(Keys.Subtract, false))
				{
					ZoomFactor -= 0.2f;
				}
				else if (inputService.IsReleased(Keys.Delete))
				{
					if (region != null)
					{
						foreach (var area in _selection)
						{
							for (var x = area.Left; x < area.Right; x++)
								for (var y = area.Top; y < area.Bottom; y++)
								{
									var currentFilter = _presenter.SelectedFilter;
									var tile = region.GetTile(x,y);
									if (tile is null)
										continue;
									var validComponents = tile.Components.Where(c => currentFilter.IsValid(c)).ToArray();
									foreach (var component in validComponents){
										tile.RemoveComponent(component);
									}
								}

						}
						_invalidateRender = true;
						inputService.IsKeyboardHandled = true;
					}
				}
				else if (inputService.IsPressed(Keys.Multiply, false))
				{
					_drawgrid = !_drawgrid;
					_invalidateRender = true;
				}
				else
				{
					if (!inputService.IsKeyboardHandled)
					{
						foreach (var selectorKey in _selectorKeys)
						{
							if (!inputService.IsReleased(selectorKey))
								continue;

							var index = _selectorKeys.IndexOf(selectorKey);
							var filters = _presenter.Filters;

							if (index >= 0 && index < filters.Count)
								_presenter.SelectFilter(filters[index]);

							inputService.IsKeyboardHandled = true;
						}
					}

					if (!inputService.IsKeyboardHandled)
					{
						foreach (var toolKey in _toolKeys)
						{
							if (!inputService.IsReleased(toolKey))
								continue;

							var index = _toolKeys.IndexOf(toolKey);
							var tools = _presenter.Tools;

							if (index >= 0 && index < tools.Count)
								_presenter.SelectTool(tools[index]);

							inputService.IsKeyboardHandled = true;
						}
					}
				}
			}
		}

		protected override void OnUpdate(TimeSpan deltaTime)
		{
		}

		public void InvalidateRender()
		{
			_invalidateRender = true;
		}

		public Rectangle GetRenderRectangle(Rectangle viewRectangle, Rectangle inlay) {
			var ox = inlay.Left - viewRectangle.Left;
			var oy = inlay.Top - viewRectangle.Top;

			var x = (int)Math.Floor(ox * _presenter.UnitSize * _zoomFactor);
			var y = (int)Math.Floor(oy * _presenter.UnitSize * _zoomFactor);

			var width = (int)Math.Floor(inlay.Width * _presenter.UnitSize * _zoomFactor);
			var height = (int)Math.Floor(inlay.Height * _presenter.UnitSize * _zoomFactor);

			var bounds = new Rectangle(x, y, width, height);
			return bounds;
		}
		public Rectangle GetRenderRectangle(Rectangle viewRectangle, int x, int y)
		{
				var rx = (x - viewRectangle.Left) * (int)Math.Floor(_presenter.UnitSize * _zoomFactor);
				var ry = (y - viewRectangle.Top) * (int)Math.Floor(_presenter.UnitSize * _zoomFactor);
				var bounds = new Rectangle(rx, ry,
					(int)Math.Floor(_presenter.UnitSize * _zoomFactor), (int)Math.Floor(_presenter.UnitSize * _zoomFactor));
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
			
			var presentation = context.GetPresentationTarget();
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

									spritebatch.Draw(sprite.Texture, spriteBounds.Location.ToVector2(),null,  render.Color, 0, Vector2.Zero, _zoomFactor, SpriteEffects.None, 0f);
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
					if (_presenter.Visibility.ShowTeleporters) // Destination highlights for teleporters //todo: make this a toggle with a "tool" like button
					{
						var _teleportDestinationHighlight = Color.FromNonPremultiplied(80, 255, 80, 200);
						var _teleportSourceHighlight = Color.FromNonPremultiplied(160, 255, 20, 200);
						var _teleportSelectedHighlight = Color.FromNonPremultiplied(255, 255, 80, 255);
						var destinationCounter = new System.Collections.Hashtable();
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
										if (!destinationCounter.ContainsKey($"{_mx}{_my}"))
                                        {
											destinationCounter.Add($"{_mx}{_my}", 0);
											porterCount = 0;
                                        } 
										else
                                        {
											destinationCounter[$"{_mx}{_my}"] = (int)destinationCounter[$"{_mx}{_my}"] + 1;
											porterCount = (int)destinationCounter[$"{_mx}{_my}"];
										}

										var bounds = GetRenderRectangle(viewRectangle, _mx, _my);
										var innerRectangle = new Rectangle(bounds.Left, bounds.Top + 16*porterCount, (int)Math.Floor(55 *_zoomFactor), 16);

										spritebatch.DrawRectangle(bounds, highlight);
										spritebatch.FillRectangle(innerRectangle, highlight);

										spritebatch.DrawString(_font, $"{searchRegion.Name}",
											new Vector2(bounds.Left + 2, bounds.Top + 2 + 16 * porterCount), Color.Black);
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
	
									spritebatch.DrawString(_font, $"{targetRegionName}",
										new Vector2(bounds.Left + 2, bounds.Bottom - 14), Color.Black);
								}
							}

						}
					}
					if (_presenter.Visibility.ShowSpawns && _presenter.ActiveDocument is not WorldForge.UI.Documents.SpawnsViewModel)
					{
						var _inclusionBorder = Color.FromNonPremultiplied(200, 255, 50, 255);
						var _inclusionFill = Color.FromNonPremultiplied(200, 255, 50, 50);
						var _exclusionBorder = Color.FromNonPremultiplied(0, 0, 0, 255);
						var _exclusionFill = Color.FromNonPremultiplied(50, 50, 50, 200);
						var _locationBorder = Color.FromNonPremultiplied(0, 255, 255, 200);
						foreach (LocationSpawner l in segment.Spawns.Location.Where<LocationSpawner>(l => l.Region == region.ID))
						{
							var _mx = l.X;
							var _my = l.Y;
							if (viewRectangle.Contains(_mx,_my))
							{
								var bounds = GetRenderRectangle(viewRectangle, _mx, _my);
								var innerRectangle = new Rectangle(bounds.Left, bounds.Top, (int)Math.Floor(55 * _zoomFactor), 16);

								spritebatch.DrawRectangle(bounds, _locationBorder);
								spritebatch.FillRectangle(innerRectangle, _locationBorder);

								spritebatch.DrawString(_font, $"{l.Name}",
									new Vector2(bounds.Left + 2, bounds.Top + 2), Color.Black);

							}
						}
						foreach (RegionSpawner r in segment.Spawns.Region.Where<RegionSpawner>(r => r.Region == region.ID))
						{
							foreach (SegmentBounds inc in r.Inclusions)
							{
								if (!viewRectangle.Intersects(inc.ToRectangle())) { continue; }

								var bounds = GetRenderRectangle(viewRectangle, inc.ToRectangle());

								spritebatch.DrawRectangle(bounds, _inclusionBorder);
								spritebatch.FillRectangle(bounds, _inclusionFill);

								spritebatch.DrawString(_font, $"{r.Name}",
									new Vector2(bounds.Left + 2, bounds.Top + 2), Color.Black);
							}
							foreach (SegmentBounds exc in r.Exclusions)
							{
								if (exc is {Left:0, Right:0, Top:0, Bottom:0 } ||!viewRectangle.Intersects(exc.ToRectangle())) { continue; }

								var bounds = GetRenderRectangle(viewRectangle, exc.ToRectangle());

								spritebatch.DrawRectangle(bounds, _exclusionBorder);
								spritebatch.FillRectangle(bounds, _exclusionFill);

								spritebatch.DrawString(_font, "Exclusion",
									new Vector2(bounds.Left + 2, bounds.Top + 2), Color.Black);
							}
						}
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

					if (rectangle.X == 3 && rectangle.Y == 3) { var breakpoint = true; }

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

					spriteBatch.DrawString(_font, $"{vx}, {vy}",
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
			var region = _presentationTarget.Region;

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
			var offset = new Vector2F(
				(int)Math.Floor((_renderTarget.Width / 2) / (_presenter.UnitSize * _zoomFactor)),
				(int)Math.Floor((_renderTarget.Height / 2) / (_presenter.UnitSize * _zoomFactor)));
			
			CameraLocation = new Vector2F(mx, my) - offset;
			
			InvalidateRender();
		}
	}

}


