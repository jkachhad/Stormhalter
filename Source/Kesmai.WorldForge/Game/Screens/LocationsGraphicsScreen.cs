using System;
using CommonServiceLocator;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using Kesmai.WorldForge.Editor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kesmai.WorldForge;

public class LocationsGraphicsScreen : WorldGraphicsScreen
{
	private static readonly string[] _arrowPaths =
	{
		@"UI\Textures\Pathing\Arrows\SE",
		@"UI\Textures\Pathing\Arrows\S",
		@"UI\Textures\Pathing\Arrows\SW",
		@"UI\Textures\Pathing\Arrows\E",
		String.Empty,
		@"UI\Textures\Pathing\Arrows\W",
		@"UI\Textures\Pathing\Arrows\NE",
		@"UI\Textures\Pathing\Arrows\N",
		@"UI\Textures\Pathing\Arrows\NW"
	};
	
	private static Color _highlightColor = Color.FromNonPremultiplied(0, 255, 255, 200);

	private LocationsPresentationTarget _locationsPresentationTarget;
	
	private int _cameraMx;
	private int _cameraMy;

	private bool _locked;
	
	private Texture2D[] _arrowTextures;

	public override bool DrawSelection => false;

	public LocationsGraphicsScreen(IGraphicsService graphicsService, LocationsPresentationTarget locationPresentationTarget) : base(graphicsService, locationPresentationTarget)
	{
		_locationsPresentationTarget = locationPresentationTarget;
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		
		_arrowTextures ??= new  Texture2D[_arrowPaths.Length];

		var services = ServiceLocator.Current;
		var contentManager = services.GetInstance<ContentManager>();
		
		for (int i = 0; i < _arrowPaths.Length; i++)
		{
			var path = _arrowPaths[i];
			
			if (!String.IsNullOrEmpty(path))
				_arrowTextures[i] = contentManager.Load<Texture2D>(path);
		}

		_locked = true;
	}

	public void SetCameraLocation(int mx, int my)
	{
		_cameraMx = mx;
		_cameraMy = my;
			
		CenterCameraOn(_cameraMx, _cameraMy);
	}

	protected override void OnHandleInput(TimeSpan deltaTime)
	{
		base.OnHandleInput(deltaTime);
		
		var inputManager = PresentationTarget.InputManager;
		
		if (inputManager is null)
			return;
		
		// process mouse/touch input.

		// process keyboard
		if (inputManager.IsKeyboardHandled || !PresentationTarget.IsFocused)
			return;
		
		if (inputManager.IsPressed(Keys.Left, true) || inputManager.IsPressed(Keys.A, true))
		{
			SetCameraLocation(_cameraMx - 1, _cameraMy);

			_locked = false;
			inputManager.IsKeyboardHandled = true;
		}
		else if (inputManager.IsPressed(Keys.Right, true) || inputManager.IsPressed(Keys.D, true))
		{
			SetCameraLocation(_cameraMx + 1, _cameraMy);
			
			_locked = false;
			inputManager.IsKeyboardHandled = true;
		}
		else if (inputManager.IsPressed(Keys.Up, true) || inputManager.IsPressed(Keys.W, true))
		{
			SetCameraLocation(_cameraMx, _cameraMy - 1);
			
			_locked = false;
			inputManager.IsKeyboardHandled = true;
		}
		else if (inputManager.IsPressed(Keys.Down, true) || inputManager.IsPressed(Keys.S, true))
		{
			SetCameraLocation(_cameraMx, _cameraMy + 1);
			
			_locked = false;
			inputManager.IsKeyboardHandled = true;
		}
		else if (inputManager.IsPressed(Keys.Enter, true))
		{
			_locationsPresentationTarget.Location.X = _cameraMx;
			_locationsPresentationTarget.Location.Y = _cameraMy;
			
			_locked = true;
			
			InvalidateRender();
			
			inputManager.IsKeyboardHandled = true;
		}
	}

	protected override void OnAfterRender(SpriteBatch spriteBatch)
	{
		base.OnAfterRender(spriteBatch);

		var viewRectangle = GetViewRectangle();

		if (viewRectangle.Contains(_cameraMx, _cameraMy))
		{
			// render arrows around the location
			for (int i = 0; i < _arrowTextures.Length; i++)
			{
				int dx = (i % 3) - 1;
				int dy = (i / 3) - 1;

				if (dx is 0 && dy is 0)
					continue;
				
				var texture = _arrowTextures[i];
				var arrowBounds = GetRenderRectangle(viewRectangle, _cameraMx + dx, _cameraMy + dy);
				
				// calculate the destination rectangle to be adjusted by the texture size
				arrowBounds.X += (arrowBounds.Width - texture.Width) / 2;
				arrowBounds.Y += (arrowBounds.Height - texture.Height) / 2;
				arrowBounds.Width = texture.Width;
				arrowBounds.Height = texture.Height;
				
				spriteBatch.Draw(texture, arrowBounds, (_locked ? Color.Lime : Color.Red));
			}
			
			var bounds = GetRenderRectangle(viewRectangle, _cameraMx, _cameraMy);
			
			spriteBatch.DrawRectangle(bounds, _highlightColor);
			spriteBatch.FillRectangle(bounds, _highlightColor * 0.2f);
		}
	}
}