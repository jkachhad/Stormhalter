using System;
using System.Linq;
using CommonServiceLocator;
using DigitalRune.Game.Input;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kesmai.WorldForge;

public abstract class ComponentTool : Tool
{
	protected SegmentTile _tileUnderMouse;
	protected TerrainComponent _componentUnderMouse;

	protected virtual Color SelectionColor => Color.Yellow;

	protected ComponentTool(string name, string icon) : base(name, icon)
	{
	}
		
	public override void OnHandleInput(WorldPresentationTarget target, IInputService inputService)
	{
		base.OnHandleInput(target, inputService);
			
		if (inputService.IsMouseOrTouchHandled)
			return;
			
		var services = ServiceLocator.Current;
		var presenter = services.GetInstance<ApplicationPresenter>();
		var regionToolbar = services.GetInstance<RegionToolbar>();
		var regionFilters = services.GetInstance<RegionFilters>();
		
		var filter = regionFilters.SelectedFilter;
			
		var worldScreen = target.WorldScreen;
		var zoomFactor = worldScreen.ZoomFactor;
		var region = target.Region;

		// get the origin tile coordinates under the mouse. Apply a hit test to find the actual component.
		bool hitTest(int wx, int wy, out SegmentTile localTileUnderMouse, out TerrainComponent localComponentUnderMouse)
		{
			localTileUnderMouse = null;
			localComponentUnderMouse = null;

			var tile = region.GetTile(wx, wy);

			if (tile is null) 
				return false;
			
			// Get the view rectangle to calculate the screen position of the tile and the offset within the tile.
			var viewRectangle = worldScreen.GetViewRectangle();

			var rx = (int)Math.Floor((wx - viewRectangle.Left) * (presenter.UnitSize * zoomFactor));
			var ry = (int)Math.Floor((wy - viewRectangle.Top) * (presenter.UnitSize * zoomFactor));

			var dx = _position.X - (rx - 45);
			var dy = _position.Y - (ry - 45);

			if (!tile.Components.Any())
				return false;

			// Check all components of the tile (in reverse order, so top-most components are checked first).
			foreach (var component in tile.Components.Reverse())
			{
				// Skip components that do not match the filter or are not valid for this tool.
				if ((filter != null && !filter.IsValid(component)) || !IsValid(component))
					continue;
				
				// Check all terrain layers of the component (in order of their drawing order).
				foreach (var layer in component.GetTerrain().SelectMany(r => r.Terrain))
				{
					var sprite = layer.Sprite;

					if (sprite is null || !sprite.HitTest((int)dx, (int)dy)) 
						continue;
					
					localTileUnderMouse = tile;
					localComponentUnderMouse = component;
					return true;
				}
			}

			return false;
		}
		
		var (mx, my) = worldScreen.ToWorldCoordinates((int)_position.X, (int)_position.Y);

		if (hitTest(mx + 1, my + 1, out var tileUnderMouse, out var componentUnderMouse) ||
		    hitTest(mx, my + 1, out tileUnderMouse, out componentUnderMouse) ||
		    hitTest(mx + 1, my, out tileUnderMouse, out componentUnderMouse) ||
		    hitTest(mx, my, out tileUnderMouse, out componentUnderMouse))
		{
			if (_tileUnderMouse != tileUnderMouse || _componentUnderMouse != componentUnderMouse)
				worldScreen.InvalidateRender();
			
			_tileUnderMouse = tileUnderMouse;
			_componentUnderMouse = componentUnderMouse;
		}
		else
		{
			_tileUnderMouse = null;
			_componentUnderMouse = null;
		}
		
		if (!inputService.IsKeyboardHandled)
		{
			if (inputService.IsReleased(Keys.Escape))
			{
				regionToolbar.SelectTool(null);
				inputService.IsKeyboardHandled = true;
			}
		}

		if (inputService.IsReleased(MouseButtons.Left))
		{
			if (_componentUnderMouse != null)
				OnClick();

			if (_tileUnderMouse != null)
				_tileUnderMouse.UpdateTerrain();

			worldScreen.InvalidateRender();
				
			inputService.IsMouseOrTouchHandled = true;
		}
	}

	protected virtual bool IsValid(TerrainComponent component)
	{
		return true;
	}

	protected virtual void OnClick()
	{
	}
		
	public override void OnRender(RenderContext context)
	{
		base.OnRender(context);
	}
}