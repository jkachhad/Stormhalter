using System;
using System.Collections.Generic;
using System.Linq;
using CommonServiceLocator;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using Kesmai.WorldForge.Editor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge;

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