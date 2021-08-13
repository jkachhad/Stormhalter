using System.Linq;
using CommonServiceLocator;
using DigitalRune.Game.Input;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Kesmai.WorldForge
{
	public abstract class ComponentTool : Tool
	{
		protected SegmentTile _tileUnderMouse;
		protected TerrainComponent _componentUnderMouse;

		protected virtual Color SelectionColor => Color.Yellow;

		protected ComponentTool(string name, string icon) : base(name, icon)
		{
		}
		
		public override void OnHandleInput(PresentationTarget target, IInputService inputService)
		{
			base.OnHandleInput(target, inputService);
			
			if (inputService.IsMouseOrTouchHandled)
				return;
			
			var services = ServiceLocator.Current;
			var presenter = services.GetInstance<ApplicationPresenter>();
			var filter = presenter.SelectedFilter;
			
			var worldScreen = target.WorldScreen;
			var region = target.Region;
			
			var (mx, my) = worldScreen.ToWorldCoordinates((int)_position.X, (int)_position.Y);
			var tile = region.GetTile(mx, my);

			if (tile != null)
			{
				var viewRectangle = worldScreen.GetViewRectangle();

				var rx = (mx - viewRectangle.Left) * (presenter.UnitSize);
				var ry = (my - viewRectangle.Top) * (presenter.UnitSize);

				var dx = _position.X - (rx - 45);
				var dy = _position.Y - (ry - 45);
				
				foreach (var component in tile.Components)
				{
					if ((filter != null && !filter.IsValid(component)) || !IsValid(component))
						continue;
					
					var renderList = component.GetTerrain().ToList();

					for (var i = renderList.Count - 1; i >= 0; i--)
					{
						var render = renderList[i];

						foreach (var layer in render.Terrain)
						{
							var sprite = layer.Sprite;

							if (sprite != null && sprite.HitTest((int)dx, (int)dy))
							{
								_tileUnderMouse = tile;
								_componentUnderMouse = component;
							}
						}
					}
				}
			}
			
			if (!inputService.IsKeyboardHandled)
			{
				if (inputService.IsReleased(Keys.Escape))
				{
					presenter.SelectTool(default(Tool));
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
			
			var services = ServiceLocator.Current;
			var presenter = services.GetInstance<ApplicationPresenter>();
			
			var graphicsService = context.GraphicsService;
			var spriteBatch = graphicsService.GetSpriteBatch();
			
			var presentationTarget = context.GetPresentationTarget();
			var worldScreen = presentationTarget.WorldScreen;
			
			if (_componentUnderMouse != null)
			{
				var viewRectangle = worldScreen.GetViewRectangle();
				var (mx, my) = worldScreen.ToWorldCoordinates((int)_position.X, (int)_position.Y);
			
				var rx = (mx - viewRectangle.Left) * (presenter.UnitSize);
				var ry = (my - viewRectangle.Top) * (presenter.UnitSize);
			
				var tileBounds = new Rectangle(rx, ry, presenter.UnitSize, presenter.UnitSize);
				var originalBounds = new Rectangle(tileBounds.X - 45, tileBounds.Y - 45, 100, 100);

				var component = _componentUnderMouse;
				var terrains = component.GetTerrain();
				
				foreach (var render in terrains)
				{
					foreach (var layer in render.Terrain)
					{
						var sprite = layer.Sprite;

						if (sprite != null)
						{
							var spriteBounds = originalBounds;

							if (sprite.Offset != Vector2F.Zero)
								spriteBounds.Offset(sprite.Offset.X, sprite.Offset.Y);

							spriteBatch.Draw(sprite.Texture, spriteBounds.Location.ToVector2(), SelectionColor);
						}
					}
				}
			}
		}
	}
}