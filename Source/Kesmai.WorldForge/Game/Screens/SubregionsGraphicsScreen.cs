using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Graphics;
using Kesmai.WorldForge.Editor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge;

public class SubregionsGraphicsScreen : WorldGraphicsScreen
{
	private SegmentSubregion _subregion;
	private SegmentBounds _selectedBounds;
	
	
	private readonly ArrowTool _arrowTool;
		
	public SubregionsGraphicsScreen(IGraphicsService graphicsService, WorldPresentationTarget worldPresentationTarget) : base(graphicsService, worldPresentationTarget)
	{
		_arrowTool = new ArrowTool();
	}
	
	protected override IEnumerable<MenuItem> GetContextMenuItems(int mx, int my)
	{
		yield return _contextMenu.Create("Add selection to Subregion..", selectionAppendInclusions);
		void selectionAppendInclusions(object sender, EventArgs args)
		{
			foreach (var rectangle in _selection)
				_subregion.Rectangles.Add(new SegmentBounds(rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1));

			InvalidateRender();
		}
	}

	public void SetSubregion(SegmentSubregion subregion)
	{
		_subregion = subregion;
	}

	public void SetBounds(SegmentBounds bounds)
	{
		_selectedBounds = bounds;
		
		var centerX = (bounds.Left + bounds.Right) / 2;
		var centerY = (bounds.Top + bounds.Bottom) / 2;
		
		CenterCameraOn(centerX, centerY);
	}

	protected override void OnHandleInput(TimeSpan deltaTime)
	{
		base.OnHandleInput(deltaTime);

		if (_subregion is null)
			return;

		if (_arrowTool != null)
			_worldPresentationTarget.Cursor = _arrowTool.Cursor;

		var inputManager = PresentationTarget.InputManager;

		if (inputManager is null)
			return;

		if (!inputManager.IsMouseOrTouchHandled && _arrowTool != null)
			_arrowTool.OnHandleInput(_worldPresentationTarget, inputManager);
	}

	protected override void OnRender(RenderContext context)
	{
		base.OnRender(context);

		if (_subregion is null)
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

		if (_subregion != null)
		{
			var viewRectangle = GetViewRectangle();
			var rectangles = _subregion.Rectangles;

			foreach (var rectangle in rectangles)
			{
				if (!viewRectangle.Intersects(rectangle.ToRectangle()))
					continue;

				var bounds = GetRenderRectangle(viewRectangle, rectangle.ToRectangle());

				var opacity = 0.6f;
				
				if (Equals(rectangle, _selectedBounds))
					opacity = 1.2f;

				spriteBatch.FillRectangle(bounds, _subregion.Color * opacity);
				spriteBatch.DrawRectangle(bounds, _subregion.Border);
			}
		}
	}
}
