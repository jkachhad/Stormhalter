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

					var ox = rectangle.Left - viewRectangle.Left;
					var oy = rectangle.Top - viewRectangle.Top;

					var x = (int)(ox * _presenter.UnitSize * _zoomFactor);
					var y = (int)(oy * _presenter.UnitSize * _zoomFactor);

					var width = (int)(rectangle.Width * _presenter.UnitSize * _zoomFactor);
					var height = (int)(rectangle.Height * _presenter.UnitSize * _zoomFactor);

					var bounds = new Rectangle(x, y, width, height);

					spriteBatch.FillRectangle(bounds, _subregion.Color);
					spriteBatch.DrawRectangle(bounds, _subregion.Border);

					spriteBatch.DrawString(_font, _subregion.Name,
						new Vector2(bounds.X + 5, bounds.Y + 5), Color.White);
				}
			}
		}
	}
	
	public class LocationsGraphicsScreen : WorldGraphicsScreen
	{
		private static Color _majorColor = Color.FromNonPremultiplied(154, 205, 50, 200);
		private static Color _highlightColor = Color.FromNonPremultiplied(0, 255, 255, 200);
		
		private int _mx;
		private int _my;
		
		public LocationsGraphicsScreen(PresentationTarget presentationTarget, IUIService uiService, IGraphicsService graphicsService) : base(presentationTarget, uiService, graphicsService)
		{
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
			
			for (var vx = viewRectangle.Left; vx <= viewRectangle.Right; vx++)
			for (var vy = viewRectangle.Top; vy <= viewRectangle.Bottom; vy++)
			{
				var dx = vx % 10;
				var dy = vy % 10;

				if ((dx != 0 || dy != 0) || (_mx == vx && _my == vy))
					continue;
				
				var rx = (vx - viewRectangle.Left) * (int)(_presenter.UnitSize * _zoomFactor);
				var ry = (vy - viewRectangle.Top) * (int)(_presenter.UnitSize * _zoomFactor);
				
				var bounds = new Rectangle(rx, ry,
					(int) (_presenter.UnitSize * _zoomFactor), (int) (_presenter.UnitSize * _zoomFactor));
				var innerRectangle = new Rectangle(bounds.Left, bounds.Top, 55, 20);
				
				spriteBatch.DrawRectangle(bounds, _majorColor);
				spriteBatch.FillRectangle(innerRectangle, _majorColor);
			
				spriteBatch.DrawString(_font, $"{vx}, {vy}", 
					new Vector2(bounds.Left + 4, bounds.Top + 3), Color.Black);
			}

			if (viewRectangle.Contains(_mx, _my))
			{
				var rx = (_mx - viewRectangle.Left) * (int)(_presenter.UnitSize * _zoomFactor);
				var ry = (_my - viewRectangle.Top) * (int)(_presenter.UnitSize * _zoomFactor);
				
				var bounds = new Rectangle(rx, ry,
					(int) (_presenter.UnitSize * _zoomFactor), (int) (_presenter.UnitSize * _zoomFactor));
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
		
		private RenderTarget2D _renderTarget;
		private bool _invalidateRender;
		
		private Vector2F _cameraLocation = Vector2F.Zero;
		private Vector2F _cameraDrag = Vector2F.Zero;

		protected float _zoomFactor = 1.0f;

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
			
			var createSpawnMenuItem = new MenuItem() { Title = "Create Spawn..", };
			var createLocationMenuItem = new MenuItem() { Title = "Create Location..", };

			createSpawnMenuItem.Click += CreateSpawn;
			createLocationMenuItem.Click += CreateLocation;
			
			_contextMenu.Items.Add(createSpawnMenuItem);
			_contextMenu.Items.Add(createLocationMenuItem);

			uiService.Screens.Add(_uiScreen);

			_font = renderer.GetFont("Tahoma14Bold");
		}

		public virtual void Initialize()
		{
		}

		private void CreateSpawn(object sender, EventArgs args)
		{
			var inputService = _uiScreen.InputService;
			var position = inputService.MousePosition;
			var region = _presentationTarget.Region;
			var tile = ToWorldTile((int)position.X, (int)position.Y);

			if (tile != null)
			{
				var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
				var segment = segmentRequest.Response;

				if (segment != null)
				{
					segment.Spawns.Location.Add(new LocationSpawner()
					{
						Name = $"Location Spawn {tile.X}, {tile.Y} [{region.ID}]",
						X = tile.X, Y = tile.Y, Region = region.ID,
						
						MinimumDelay = TimeSpan.FromMinutes(15.0),
						MaximumDelay = TimeSpan.FromMinutes(15.0),
					});
				}
			}
		}
		
		private void CreateLocation(object sender, EventArgs args)
		{
			var inputService = _uiScreen.InputService;
			var position = inputService.MousePosition;
			var region = _presentationTarget.Region;
			var tile = ToWorldTile((int)position.X, (int)position.Y);

			if (tile != null)
			{
				var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
				var segment = segmentRequest.Response;
				
				if (segment != null)
				{
					segment.Locations.Add(new SegmentLocation()
					{
						Name = $"Location {tile.X}, {tile.Y} [{region.ID}]",
						X = tile.X, Y = tile.Y, Region = region.ID,
					});
				}
			}
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
						inputService.IsMouseOrTouchHandled = true;
						_contextMenu.Open(_uiScreen, args.Context.MousePosition);
					}
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
			else if (inputService.IsPressed(Keys.Add, false))
			{
				ZoomFactor += 0.1f;
			}
			else if (inputService.IsPressed(Keys.Subtract, false))
			{
				ZoomFactor -= 0.1f;
			}
			else if (inputService.IsReleased(Keys.Delete))
			{
				if (region != null)
				{
					foreach (var area in _selection)
					{
						for (var x = area.Left; x < area.Right; x++)
						for (var y = area.Top; y < area.Bottom; y++)
							region.DeleteTile(x, y);
					}

					inputService.IsKeyboardHandled = true;
				}
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

		protected override void OnUpdate(TimeSpan deltaTime)
		{
		}

		public void InvalidateRender()
		{
			_invalidateRender = true;
		}

		protected override void OnRender(RenderContext context)
		{
			var graphicsService = context.GraphicsService;
			var graphicsDevice = graphicsService.GraphicsDevice;
			var spritebatch = graphicsService.GetSpriteBatch();
			var viewRectangle = GetViewRectangle();
			
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
						var rx = (vx - viewRectangle.Left) * (int) (_presenter.UnitSize * _zoomFactor);
						var ry = (vy - viewRectangle.Top) * (int) (_presenter.UnitSize * _zoomFactor);
						
						var segmentTile = region.GetTile(vx, vy);

						if (segmentTile != default(SegmentTile))
						{
							var tileBounds = new Rectangle(rx, ry,
								(int) (_presenter.UnitSize * _zoomFactor), (int) (_presenter.UnitSize * _zoomFactor));
							var originalBounds = new Rectangle(tileBounds.X - 45, tileBounds.Y - 45,
								(int) (100 * _zoomFactor), (int) (100 * _zoomFactor));

							var renders = segmentTile.Renders;
							
							foreach (var render in renders)
							{
								var sprite = render.Layer.Sprite;

								if (sprite != null)
								{
									var spriteBounds = originalBounds;

									if (sprite.Offset != Vector2F.Zero)
										spriteBounds.Offset(sprite.Offset.X, sprite.Offset.Y);

									spritebatch.Draw(sprite.Texture, spriteBounds.Location.ToVector2(), render.Color);
								}
							}


							OnRenderTile(spritebatch, segmentTile, tileBounds);
						}
					}

					OnAfterRender(spritebatch);
				}
				
				graphicsService.GraphicsDevice.SetRenderTargets(oldTargets);
				
				spritebatch.Draw(_renderTarget, Vector2.Zero, Color.White);
			}
			
			var selection = _selection;

			if (selection.Region == region)
			{
				foreach (var rectangle in selection)
				{
					if (!viewRectangle.Intersects(rectangle))
						continue;

					var ox = rectangle.Left - viewRectangle.Left;
					var oy = rectangle.Top - viewRectangle.Top;

					var x = (int)(ox * _presenter.UnitSize);
					var y = (int)(oy * _presenter.UnitSize);

					var width = (int)(rectangle.Width * _presenter.UnitSize);
					var height = (int)(rectangle.Height * _presenter.UnitSize);

					var bounds = new Rectangle(x, y, width, height);

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
				(int)(_renderTarget.Width / (_presenter.UnitSize * _zoomFactor)) + 1,
				(int)(_renderTarget.Height / (_presenter.UnitSize * _zoomFactor)) + 1);
		}

		public (int wx, int wy) ToWorldCoordinates(int mx, int my)
		{
			return ((int)_cameraLocation.X + (int)((mx - _cameraDrag.X) / (_presenter.UnitSize * _zoomFactor)), 
					(int)_cameraLocation.Y + (int)((my - _cameraDrag.Y) / (_presenter.UnitSize * _zoomFactor)));
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
				(wx - (int)_cameraLocation.X) * ((int)(_presenter.UnitSize * _zoomFactor)), 
				(wy - (int)_cameraLocation.Y) * ((int)(_presenter.UnitSize * _zoomFactor)), 
				_presenter.UnitSize, 
				_presenter.UnitSize);
		}

		public void CenterCameraOn(int mx, int my)
		{
			var offset = new Vector2F(
				(int)((_renderTarget.Width / 2) / (_presenter.UnitSize * _zoomFactor)), 
				(int)((_renderTarget.Height / 2) / (_presenter.UnitSize * _zoomFactor)));
			
			CameraLocation = new Vector2F(mx, my) - offset;
			
			InvalidateRender();
		}
	}
}
