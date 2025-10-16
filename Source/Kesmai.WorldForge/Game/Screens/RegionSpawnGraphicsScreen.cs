using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using Kesmai.WorldForge.Editor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge;

public class RegionSpawnGraphicsScreen : WorldGraphicsScreen
{
	private readonly Color _inclusionBorder = Color.FromNonPremultiplied(255, 0, 255, 255);
	private readonly Color _inclusionFill = Color.FromNonPremultiplied(255, 150, 255, 10);
	private readonly Color _exclusionBorder = Color.FromNonPremultiplied(255, 0, 0, 255);
	private readonly Color _exclusionFill = Color.FromNonPremultiplied(100, 50, 50, 150);

	private RegionSpawner _spawner;

	private readonly ArrowTool _arrowTool;

	public RegionSpawnGraphicsScreen(IGraphicsService graphicsService, WorldPresentationTarget presentationTarget) : base(graphicsService, presentationTarget)
	{
		_arrowTool = new ArrowTool();
	}

	public void SetSpawner(RegionSpawner spawner)
	{
		_spawner = spawner;

		if (_spawner is null)
			return;

		var inclusion = _spawner.Inclusions.FirstOrDefault();

		if (inclusion != null)
		{
			CenterCameraOn((int)(inclusion.Left + inclusion.Width / 2),
				(int)(inclusion.Top + inclusion.Height / 2));
		}
	}
	
	protected override void OnHandleInput(TimeSpan deltaTime)
	{
		base.OnHandleInput(deltaTime);

		if (_spawner is null)
			return;

		if (_arrowTool != null)
			_worldPresentationTarget.Cursor = _arrowTool.Cursor;

		var inputManager = PresentationTarget.InputManager;

		if (inputManager is null)
			return;

		if (!inputManager.IsMouseOrTouchHandled && _arrowTool != null)
			_arrowTool.OnHandleInput(_worldPresentationTarget, inputManager);
	}

	protected override IEnumerable<MenuItem> GetContextMenuItems(int mx, int my)
	{
		if (_spawner is null)
			yield break;

		if (_selection.IsSelected(mx, my, _worldPresentationTarget.Region))
		{
			yield return _contextMenu.Create("Add selection to Inclusions..", selectionAppendInclusions);
			yield return _contextMenu.Create("Add selection to Exclusions..", selectionAppendExclusions);
		}

		var inclusion = _spawner.Inclusions.OrderBy(r => r.Area).FirstOrDefault(r => r.Contains(mx, my));
		var exclusion = _spawner.Exclusions.OrderBy(r => r.Area).FirstOrDefault(r => r.Contains(mx, my));

		if (inclusion != null)
		{
			yield return _contextMenu.Create("Remove Inclusion", (s, a) =>
			{
				_spawner.Inclusions.Remove(inclusion);
				InvalidateRender();
			});
		}

		if (exclusion != null)
		{
			yield return _contextMenu.Create("Remove Exclusion", (s, a) =>
			{
				_spawner.Exclusions.Remove(exclusion);
				InvalidateRender();
			});
		}

		void selectionAppendInclusions(object sender, EventArgs args)
		{
			foreach (var rectangle in _selection)
				_spawner.Inclusions.Add(new SegmentBounds(rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1));

			InvalidateRender();
		}

		void selectionAppendExclusions(object sender, EventArgs args)
		{
			foreach (var rectangle in _selection)
				_spawner.Exclusions.Add(new SegmentBounds(rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1));

			InvalidateRender();
		}
	}

	protected override void OnRender(RenderContext context)
	{
		base.OnRender(context);

		if (_spawner is null)
			return;

		var graphicsService = context.GraphicsService;
		var spriteBatch = graphicsService.GetSpriteBatch();

		spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);

		if (_isMouseOver && _arrowTool != null)
			_arrowTool.OnRender(context);

		spriteBatch.End();
	}

	protected override void OnAfterRender(SpriteBatch spriteBatch)
	{
		base.OnAfterRender(spriteBatch);

		if (_spawner is null)
			return;

		var viewRectangle = GetViewRectangle();

		foreach (var rectangle in _spawner.Inclusions)
		{
			if (!viewRectangle.Intersects(rectangle.ToRectangle()))
				continue;

			var bounds = GetRenderRectangle(viewRectangle, rectangle.ToRectangle());

			spriteBatch.FillRectangle(bounds, _inclusionFill);
			spriteBatch.DrawRectangle(bounds, _inclusionBorder);

			_font.DrawString(spriteBatch, RenderTransform.Identity, _spawner.Name,
				new Vector2(bounds.X + 5, bounds.Y + 5), Color.White);
		}

		foreach (var rectangle in _spawner.Exclusions)
		{
			if (rectangle is { Left: 0, Top: 0, Right: 0, Bottom: 0 } || !viewRectangle.Intersects(rectangle.ToRectangle()))
				continue;

			var bounds = GetRenderRectangle(viewRectangle, rectangle.ToRectangle());

			spriteBatch.FillRectangle(bounds, _exclusionFill);
			spriteBatch.DrawRectangle(bounds, _exclusionBorder);

			_font.DrawString(spriteBatch, RenderTransform.Identity, "Exclusion",
				new Vector2(bounds.X + 5, bounds.Y + 5), Color.White);
		}
	}
}
