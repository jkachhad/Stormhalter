using System;
using CommonServiceLocator;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kesmai.WorldForge;

public class LocationSpawnGraphicsScreen : WorldGraphicsScreen
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

	private static readonly Color _highlightColor = Color.FromNonPremultiplied(0, 255, 255, 200);

	private LocationSegmentSpawner _segmentSpawner;

	private int _targetMx;
	private int _targetMy;

	private bool _locked;

	private Texture2D[] _arrowTextures;

	public LocationSpawnGraphicsScreen(IGraphicsService graphicsService, WorldPresentationTarget presentationTarget) : base(graphicsService, presentationTarget)
	{
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();

		_arrowTextures ??= new Texture2D[_arrowPaths.Length];

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

	public void SetSpawner(LocationSegmentSpawner segmentSpawner)
	{
		_segmentSpawner = segmentSpawner;

		if (_segmentSpawner == null)
			return;

		SetCameraLocation(_segmentSpawner.X, _segmentSpawner.Y);

		_locked = true;
	}

	private void SetCameraLocation(int mx, int my)
	{
		_targetMx = mx;
		_targetMy = my;

		CenterCameraOn(_targetMx, _targetMy);
	}

	protected override void OnHandleInput(TimeSpan deltaTime)
	{
		base.OnHandleInput(deltaTime);

		if (_segmentSpawner == null)
			return;

		var inputManager = PresentationTarget.InputManager;

		if (inputManager is null)
			return;

		if (inputManager.IsKeyboardHandled || !PresentationTarget.IsFocused)
			return;

		if (inputManager.IsPressed(Keys.Left, true) || inputManager.IsPressed(Keys.A, true))
		{
			SetCameraLocation(_targetMx - 1, _targetMy);

			_locked = false;
			inputManager.IsKeyboardHandled = true;
		}
		else if (inputManager.IsPressed(Keys.Right, true) || inputManager.IsPressed(Keys.D, true))
		{
			SetCameraLocation(_targetMx + 1, _targetMy);

			_locked = false;
			inputManager.IsKeyboardHandled = true;
		}
		else if (inputManager.IsPressed(Keys.Up, true) || inputManager.IsPressed(Keys.W, true))
		{
			SetCameraLocation(_targetMx, _targetMy - 1);

			_locked = false;
			inputManager.IsKeyboardHandled = true;
		}
		else if (inputManager.IsPressed(Keys.Down, true) || inputManager.IsPressed(Keys.S, true))
		{
			SetCameraLocation(_targetMx, _targetMy + 1);

			_locked = false;
			inputManager.IsKeyboardHandled = true;
		}
		else if (inputManager.IsPressed(Keys.Enter, true))
		{
			_segmentSpawner.X = _targetMx;
			_segmentSpawner.Y = _targetMy;

			_locked = true;

			InvalidateRender();

			inputManager.IsKeyboardHandled = true;
		}
	}

	protected override void OnAfterRender(SpriteBatch spriteBatch)
	{
		base.OnAfterRender(spriteBatch);

		if (_segmentSpawner is null)
			return;

		var viewRectangle = GetViewRectangle();

		if (!viewRectangle.Contains(_targetMx, _targetMy))
			return;

		for (int i = 0; i < _arrowTextures.Length; i++)
		{
			int dx = (i % 3) - 1;
			int dy = (i / 3) - 1;

			if (dx is 0 && dy is 0)
				continue;

			var texture = _arrowTextures[i];
			var arrowBounds = GetRenderRectangle(viewRectangle, _targetMx + dx, _targetMy + dy);

			arrowBounds.X += (arrowBounds.Width - texture.Width) / 2;
			arrowBounds.Y += (arrowBounds.Height - texture.Height) / 2;
			arrowBounds.Width = texture.Width;
			arrowBounds.Height = texture.Height;

			spriteBatch.Draw(texture, arrowBounds, (_locked ? Color.Lime : Color.Red));
		}

		var bounds = GetRenderRectangle(viewRectangle, _targetMx, _targetMy);

		spriteBatch.DrawRectangle(bounds, _highlightColor);
		spriteBatch.FillRectangle(bounds, _highlightColor * 0.2f);
	}
}
