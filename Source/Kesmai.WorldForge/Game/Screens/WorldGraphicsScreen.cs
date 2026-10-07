using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CommonServiceLocator;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;
using CommunityToolkit.Mvvm.Messaging;
using DigitalRune.Game.Interop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kesmai.WorldForge;

public class WorldGraphicsScreen : UIGraphicsScreen
{
	/// <summary>
	/// Deepest nesting of segment components and templates followed when looking inside a tile's providers.
	/// </summary>
	protected const int MaxProviderNesting = 8;

	protected static Color _selectionFill = Color.FromNonPremultiplied(255, 255, 0, 75);
	protected static Color _selectionBorder = Color.FromNonPremultiplied(255, 255, 0, 100);

	protected ApplicationPresenter _presenter;
	
	protected Selection _selection;

	protected WorldPresentationTarget _worldPresentationTarget;
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

	private Texture2D _commentSprite;

	/// <summary>
	/// Represents the current camera location in world coordinates. (Top-left corner of the view)
	/// </summary>
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

	/// <summary>
	/// Gets a value indicating whether the selection should be drawn.
	/// </summary>
	public virtual bool DrawSelection => true;

	/// <summary>
	/// Gets the on-screen size of one tile, in pixels, at the current zoom.
	/// </summary>
	protected int TileSize => (int)Math.Floor(_presenter.UnitSize * _zoomFactor);

	public int Width => (int)_worldPresentationTarget.ActualWidth;
	public int Height => (int)_worldPresentationTarget.ActualHeight;

	public virtual bool DisplayComments => true;

	public WorldGraphicsScreen(IGraphicsService graphicsService, WorldPresentationTarget worldPresentationTarget) : base(graphicsService, worldPresentationTarget)
	{
		_worldPresentationTarget = worldPresentationTarget;

		var services = ServiceLocator.Current;
		
		_presenter = services.GetInstance<ApplicationPresenter>();
		
		_selection = _presenter.Selection;
		_renderTarget = new RenderTarget2D(GraphicsService.GraphicsDevice, 640, 480);
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		
		_font = UIRenderer.GetFontRenderer("Tahoma", 10);

		var commentIcon = Application.GetResourceStream(
			new Uri(@"pack://application:,,,/Kesmai.WorldForge;component/Resources/Comment-White.png"));

		if (commentIcon != null)
			_commentSprite = Texture2D.FromStream(GraphicsService.GraphicsDevice, commentIcon.Stream);
	}
	
	protected virtual IEnumerable<MenuItem> GetContextMenuItems(int mx, int my)
	{
		yield break;
	}

	protected override void OnHandleInput(TimeSpan deltaTime)
	{
		base.OnHandleInput(deltaTime);
		
		var inputManager = PresentationTarget.InputManager;
		
		if (inputManager is null)
			return;
		
		var coordinateText = "(-, -)";

		if (_isMouseDirectlyOver)
		{
			var mousePosition = inputManager.MousePosition;
			var (wx, wy) = ToWorldCoordinates((int)mousePosition.X, (int)mousePosition.Y);
			
			coordinateText = $"({wx}, {wy})";
		}

		if (_presenter.TileCoordinateDisplay != coordinateText)
			_presenter.TileCoordinateDisplay = coordinateText;
		
		// if the input is still not handled, we can check for context menu and zoom.
		if (_isMouseDirectlyOver && !inputManager.IsMouseOrTouchHandled)
		{
			if (_contextMenu != null && _contextMenu.IsEnabled)
			{
				if (inputManager.IsReleased(MouseButtons.Right) && _cameraDrag.Magnitude < 2.0f)
				{
					_contextMenu.Items.Clear();
					
					var currentPosition = inputManager.MousePosition;
					var (mx, my) = ToWorldCoordinates((int)currentPosition.X, (int)currentPosition.Y);

					inputManager.IsMouseOrTouchHandled = true;
					
					_contextMenu.Items.AddRange(GetContextMenuItems(mx, my));
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
		if (inputManager.IsPressed(Keys.Add, false))
		{
			ZoomFactor += 0.2f;
		}
		else if (inputManager.IsPressed(Keys.Subtract, false))
		{
			ZoomFactor -= 0.2f;
		}
		else if (inputManager.IsPressed(Keys.Multiply, false))
		{
			_drawgrid = !_drawgrid;
			_invalidateRender = true;
		}
	}

	public void InvalidateRender()
	{
		_invalidateRender = true;
    }
	
	public Rectangle GetRenderRectangle(Rectangle viewRectangle, Rectangle inlay)
	{
		var tileSize = TileSize;
		
		var x = (inlay.Left - viewRectangle.Left) * tileSize;
		var y = (inlay.Top - viewRectangle.Top) * tileSize;
		
		var width = inlay.Width * tileSize;
		var height = inlay.Height * tileSize;

		return new Rectangle(x, y, width, height);
	}

	public Rectangle GetRenderRectangle(Rectangle viewRectangle, int x, int y)
	{
		var tileSize = TileSize;
		
		return new Rectangle((x - viewRectangle.Left) * tileSize, (y - viewRectangle.Top) * tileSize,
			tileSize, tileSize);
	}

	protected override void OnRender(RenderContext context)
    {
	    base.OnRender(context);
	    
        var graphicsService = context.GraphicsService;
		var graphicsDevice = graphicsService.GraphicsDevice;
		var spriteBatch = graphicsService.GetSpriteBatch();
		var viewRectangle = GetViewRectangle();
		var selection = _selection;

		graphicsDevice.Clear(Color.Black);
			
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

				// the offscreen pass uses its own deferred batch so consecutive draws sharing a texture
				// are submitted together, instead of one draw call per sprite in immediate mode.
				spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);

				OnBeforeRender(spriteBatch);
				
				var commentIcons = new List<(int X, int Y)>();
				var displayComments = DisplayComments;
				var tileSize = TileSize;
					
				// render terrain
				for (var vx = viewRectangle.Left; vx <= viewRectangle.Right; vx++)
				for (var vy = viewRectangle.Top; vy <= viewRectangle.Bottom; vy++)
				{
					var segmentTile = region.GetTile(vx, vy);

					if (segmentTile != null)
					{
						var tileBounds = new Rectangle((vx - viewRectangle.Left) * tileSize,
							(vy - viewRectangle.Top) * tileSize, tileSize, tileSize);
						
						OnRenderTile(spriteBatch, segmentTile, tileBounds);

						if (displayComments && HasComment(segmentTile.Providers))
							commentIcons.Add((vx, vy));
					}
				}

				OnAfterRender(spriteBatch);
				
				// render comment icons
				if (DisplayComments)
				{
					foreach (var (vx, vy) in commentIcons)
					{
						var bounds = GetRenderRectangle(viewRectangle, vx, vy);
						var iconWidth = (int)(24 * _zoomFactor);
						var iconHeight = (int)(24 * _zoomFactor);
						
						var iconBounds = new Rectangle(
							bounds.Left + (bounds.Width - iconWidth) / 2,
							bounds.Top + (bounds.Height - iconHeight) / 2,
							iconWidth, iconHeight);

						if (_commentSprite != null)
							spriteBatch.Draw(_commentSprite, iconBounds, Color.White);
					}
				}

				spriteBatch.End();

				graphicsService.GraphicsDevice.SetRenderTargets(oldTargets);
			}
		}

		// the screen pass stays immediate; the UI renderer draws with its own sprite batch,
		// so selection must be flushed before the UI is drawn over it.
		spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);

		if (region != default(SegmentRegion))
			spriteBatch.Draw(_renderTarget, Vector2.Zero, Color.White);

		if (DrawSelection)
		{
			if (selection.Region == region)
			{
				foreach (var rectangle in selection)
				{
					if (!viewRectangle.Intersects(rectangle))
						continue;

					// if (rectangle.X == 3 && rectangle.Y == 3) { var breakpoint = true; } removed because breakpoint not used

					var bounds = GetRenderRectangle(viewRectangle, rectangle);

					spriteBatch.FillRectangle(bounds, _selectionFill);
					spriteBatch.DrawRectangle(bounds, _selectionBorder);
				}
			}
		}

		RenderUI(context);
				
		_invalidateRender = false;
			
		spriteBatch.End();
	}

	/// <summary>
	/// Gets the screen position where a sprite of the tile at (x, y) is drawn, matching OnRenderTile and
	/// OnRenderTerrain. The sprite is drawn from there scaled by ZoomFactor / Resolution.
	/// </summary>
	public Vector2 GetSpritePosition(Rectangle viewRectangle, int x, int y, GameSprite sprite)
	{
		var tileBounds = GetRenderRectangle(viewRectangle, x, y);

		var sx = (int)Math.Floor(tileBounds.X - (45 * _zoomFactor));
		var sy = (int)Math.Floor(tileBounds.Y - (45 * _zoomFactor));

		if (sprite.Offset != Vector2F.Zero)
		{
			sx += (int)Math.Floor(sprite.Offset.X * _zoomFactor);
			sy += (int)Math.Floor(sprite.Offset.Y * _zoomFactor);
		}

		return new Vector2(sx, sy);
	}

	protected virtual void OnRenderTile(SpriteBatch spritebatch, SegmentTile segmentTile, Rectangle bounds)
	{
		var terrainBounds = new Rectangle((int)Math.Floor(bounds.X - (45*_zoomFactor)), (int)Math.Floor(bounds.Y - (45*_zoomFactor)),
			(int)Math.Floor(100 * _zoomFactor), (int)Math.Floor(100 * _zoomFactor));
		
		foreach (var render in segmentTile.Renders)
			OnRenderTerrain(spritebatch, segmentTile, render, terrainBounds);
	}

	/// <summary>
	/// Determines whether any component has a comment, including those wrapped by segment components and templates.
	/// </summary>
	private static bool HasComment(IList<IComponentProvider> providers)
	{
		// indexed loop; this runs for every visible tile on each redraw, so avoid enumerator allocations.
		for (var index = 0; index < providers.Count; index++)
		{
			if (HasComment(providers[index]))
				return true;
		}

		return false;
	}

	private static bool HasComment(IComponentProvider provider, int depth = 0)
	{
		if (provider is TerrainComponent terrainComponent)
			return !String.IsNullOrEmpty(terrainComponent.Comment);

		// wrappers only nest a few levels; the limit guards against a template that contains itself.
		if (depth >= MaxProviderNesting)
			return false;

		// only descend into wrappers. brushes, like terrain components, yield themselves from GetComponents.
		foreach (var component in provider.GetComponents())
		{
			if (ReferenceEquals(component, provider))
				continue;

			if (HasComment(component, depth + 1))
				return true;
		}

		return false;
	}

	protected virtual void OnRenderTerrain(SpriteBatch spriteBatch, SegmentTile segmentTile, TerrainRender render, Rectangle bounds)
	{
		var sprite = render.Layer.Sprite;

		if (sprite != null)
		{
			var spriteBounds = bounds;

			if (sprite.Offset != Vector2F.Zero)
				spriteBounds.Offset((int)Math.Floor(sprite.Offset.X * _zoomFactor), (int)Math.Floor(sprite.Offset.Y * _zoomFactor));
			
			spriteBatch.Draw(sprite.Texture, spriteBounds.Location.ToVector2(), null, render.Color, 0, Vector2.Zero, _zoomFactor / sprite.Resolution, SpriteEffects.None, 0f);
		}
	}

	protected virtual void OnBeforeRender(SpriteBatch spriteBatch)
	{
	}

	protected virtual void OnAfterRender(SpriteBatch spriteBatch)
	{
		var viewRectangle = GetViewRectangle();

		if (_drawgrid) {
			// step straight to the multiples of 10 in view, rather than testing every tile.
			for (var vx = FirstMultipleOf10(viewRectangle.Left); vx <= viewRectangle.Right; vx += 10)
			for (var vy = FirstMultipleOf10(viewRectangle.Top); vy <= viewRectangle.Bottom; vy += 10)
			{
				var bounds = GetRenderRectangle(viewRectangle, vx, vy);
				var innerRectangle = new Rectangle(bounds.Left, bounds.Top, 55, 20);

				spriteBatch.DrawRectangle(bounds, _selectionBorder);
				spriteBatch.FillRectangle(innerRectangle, _selectionBorder);

				_font.DrawString(spriteBatch, RenderTransform.Identity, $"{vx}, {vy}",
					new Vector2(bounds.Left + 4, bounds.Top + 3), Color.Black);
			}
		}
	}

	/// <summary>
	/// Gets the smallest multiple of 10 at or after the value, including for negative values.
	/// </summary>
	private static int FirstMultipleOf10(int value)
	{
		// the remainder keeps the value's sign in C#, so -13 % 10 is -3 and this yields -10.
		return value + ((10 - (value % 10)) % 10);
	}

	public void OnSizeChanged(int width, int height)
	{
		var services = ServiceLocator.Current;
		var graphicsService = services.GetInstance<IGraphicsService>();

		width = Math.Max(1, Math.Min(width, 4096));
		height = Math.Max(1, Math.Min(height, 16384));

		// a clamped size can never match the screen, so avoid recreating an identical target every frame.
		if (_renderTarget != null && _renderTarget.Width == width && _renderTarget.Height == height)
			return;

		// release the previous target's video memory; it is never bound here, since rendering
		// switches to it only after this check.
		_renderTarget?.Dispose();

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
