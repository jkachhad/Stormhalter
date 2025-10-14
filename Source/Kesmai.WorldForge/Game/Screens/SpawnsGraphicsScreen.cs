using System;
using System.Collections.Generic;
using System.Linq;
using CommonServiceLocator;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using Kesmai.WorldForge.Editor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kesmai.WorldForge;

public class SpawnsGraphicsScreen : WorldGraphicsScreen
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

	
	private Spawner _spawner;
	
	private Color _inclusionBorder = Color.FromNonPremultiplied(200, 255, 50, 255);
	private Color _inclusionFill = Color.FromNonPremultiplied(200, 255, 50, 50);
	private Color _exclusionBorder = Color.FromNonPremultiplied(0, 0, 0, 255);
	private Color _exclusionFill = Color.FromNonPremultiplied(50, 50, 50, 200);
	private Color _locationBorder = Color.FromNonPremultiplied(0, 255, 255, 200);
	
	private int _targetMx;
	private int _targetMy;
	
	private bool _locked;
	
	private Texture2D[] _arrowTextures;

	private SpawnsPresentationTarget _spawnsPresentationTarget;

	public SpawnsGraphicsScreen(IGraphicsService graphicsService, SpawnsPresentationTarget spawnsPresentationTarget) : base(graphicsService, spawnsPresentationTarget)
	{
		_spawnsPresentationTarget = spawnsPresentationTarget;
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
		_targetMx = mx;
		_targetMy = my;
			
		CenterCameraOn(_targetMx, _targetMy);
	}
	
	public void SetSpawner(Spawner spawner)
	{
		_spawner = spawner;
		
		switch (_spawner)
		{
			case LocationSpawner ls:
			{
				SetCameraLocation(ls.X, ls.Y);
				break;
			}
			case RegionSpawner rs:
			{
				var inclusion = rs.Inclusions.FirstOrDefault();

				if (inclusion != null)
					SetCameraLocation((int)(inclusion.Left + inclusion.Width / 2),
						(int)(inclusion.Top + inclusion.Height / 2));
				
				break;
			}
		}

		_locked = true;
	}

	protected override void OnUpdate(TimeSpan deltaTime)
	{
		base.OnUpdate(deltaTime);
		
		var inputManager = PresentationTarget.InputManager;
		
		if (inputManager is null)
			return;
		
		if (inputManager.IsKeyboardHandled || !PresentationTarget.IsFocused)
			return;

		if (_spawner is LocationSpawner locationSpawner)
		{

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
				locationSpawner.X = _targetMx;
				locationSpawner.Y = _targetMy;

				_locked = true;

				InvalidateRender();

				inputManager.IsKeyboardHandled = true;
			}
		}
	}
	
	protected override IEnumerable<MenuItem> GetContextMenuItems(int mx, int my)
	{
		if (_spawner is RegionSpawner)
		{
			yield return _contextMenu.Create("Add selection to Inclusions..", selectionAppendInclusions);
			yield return _contextMenu.Create("Add selection to Exclusions..", selectionAppendExclusions);
		}
		
		void selectionAppendInclusions(object sender, EventArgs args)
		{
			if (_spawner is not RegionSpawner regionSpawner)
				return;
			
			foreach (var rectangle in _selection)
				regionSpawner.Inclusions.Add(new SegmentBounds(rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1));

			InvalidateRender();
		}
		
		void selectionAppendExclusions(object sender, EventArgs args)
		{
			if (_spawner is not RegionSpawner regionSpawner)
				return;
			
			foreach (var rectangle in _selection)
				regionSpawner.Exclusions.Add(new SegmentBounds(rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1));

			InvalidateRender();
		}
	}

	protected override void OnAfterRender(SpriteBatch spriteBatch)
	{
		base.OnAfterRender(spriteBatch);
		
		var viewRectangle = GetViewRectangle();

		if (_spawner is RegionSpawner regionSpawner)
		{
			var inclusions = regionSpawner.Inclusions;
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

			var exclusions = regionSpawner.Exclusions;
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
		else if (_spawner is LocationSpawner locationSpawner)
		{
			var mx = _targetMx;
			var my = _targetMy;
			
			if (viewRectangle.Contains(mx, my))
			{
				// render arrows around the location
				for (int i = 0; i < _arrowTextures.Length; i++)
				{
					int dx = (i % 3) - 1;
					int dy = (i / 3) - 1;

					if (dx is 0 && dy is 0)
						continue;
				
					var texture = _arrowTextures[i];
					var arrowBounds = GetRenderRectangle(viewRectangle, mx + dx, my + dy);
				
					// calculate the destination rectangle to be adjusted by the texture size
					arrowBounds.X += (arrowBounds.Width - texture.Width) / 2;
					arrowBounds.Y += (arrowBounds.Height - texture.Height) / 2;
					arrowBounds.Width = texture.Width;
					arrowBounds.Height = texture.Height;
				
					spriteBatch.Draw(texture, arrowBounds, (_locked ? Color.Lime : Color.Red));
				}
			
				var bounds = GetRenderRectangle(viewRectangle, mx, my);
			
				spriteBatch.DrawRectangle(bounds, _highlightColor);
				spriteBatch.FillRectangle(bounds, _highlightColor * 0.2f);
			}
		}
	}
}