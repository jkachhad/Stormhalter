using System;
using System.Linq;
using System.Windows.Input;
using CommonServiceLocator;
using DigitalRune.Game.Input;
using DigitalRune.Graphics;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;
using Microsoft.Xna.Framework.Input;
using DigitalRune.Game.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Kesmai.WorldForge
{
	public class PaintTool : Tool
	{
		private bool _isShiftDown;
		private bool _isAltDown;

		public PaintTool() : base("Paint", "Editor-Icon-Paint")
		{
		}

		public override void OnHandleInput(PresentationTarget target, IInputService inputService)
		{
			base.OnHandleInput(target, inputService);
			
			if (inputService.IsMouseOrTouchHandled)
				return;
			
			var services = ServiceLocator.Current;
			var presenter = services.GetInstance<ApplicationPresenter>();

			var graphicsScreen = target.WorldScreen;
			var region = target.Region;
			var selection = presenter.Selection;

			if (!inputService.IsKeyboardHandled)
			{

				_isShiftDown = inputService.IsDown(Keys.LeftShift) || inputService.IsDown(Keys.RightShift);
				_isAltDown = inputService.IsDown(Keys.LeftAlt) || inputService.IsDown(Keys.RightAlt);

				if (inputService.IsReleased(Keys.Escape))
				{
					presenter.SelectTool(default(Tool));
					inputService.IsKeyboardHandled = true;
				}
			}
			
			var (cx, cy) = graphicsScreen.ToWorldCoordinates((int)_position.X, (int)_position.Y);

			if (!selection.Any())
				return;

			if (inputService.IsReleased(MouseButtons.Left) && selection.IsSelected(cx, cy, region))
			{
				var component = presenter.SelectedComponent;

				if (component != null)
				{
					var componentType = component.GetType();
					var floorTypes = new List<String>() {
							"FloorComponent",
							"WaterComponent",
							"IceComponent",
							"SkyComponent"
						};
					IEnumerable<TerrainComponent> similar = Enumerable.Empty<TerrainComponent>();

					foreach (var area in selection)
					{
						for (var x = area.Left; x < area.Right; x++)
						for (var y = area.Top; y < area.Bottom; y++)
						{
							var selectedTile = region.GetTile(x, y);
							
							if (selectedTile == null)
								region.SetTile(x, y, selectedTile = new SegmentTile(x, y));

							// Shift = Append; Alt = Replace
							// if not shift and not alt, then clobber like components.
							// if Shift, then there are no like components to clobber
							// if Alt, then all components get clobbered.
							// Alt wins in the case of shift+alt

							if (!_isShiftDown && !_isAltDown) //not appending and not replacing.
							{
									if (floorTypes.Contains(componentType.Name))
									{
										similar = selectedTile.GetComponents<TerrainComponent>(c => c is FloorComponent || c is WaterComponent || c is IceComponent || c is SkyComponent);
									}
									else
									{
										similar = selectedTile.GetComponents<TerrainComponent>(c => c.GetType().IsAssignableFrom(componentType));
									}
							}
							if (_isAltDown) // replacing
                            {
									similar = selectedTile.GetComponents<TerrainComponent>();
							}

							foreach (var similarComponent in similar)
								selectedTile.RemoveComponent(similarComponent);

							selectedTile.Components.Add(component.Clone());
							selectedTile.UpdateTerrain();
						}
					}
					
					graphicsScreen.InvalidateRender();
				}
			}
		}

		public override void OnActivate()
		{
			base.OnActivate();
			_cursor = Cursors.Arrow;
		}
		
		public override void OnDeactivate()
		{
			base.OnDeactivate();
		}

		public override void OnRender(RenderContext context)
		{
			base.OnRender(context);

			var graphicsService = context.GraphicsService;
			var spriteBatch = graphicsService.GetSpriteBatch();

			var presentationTarget = context.GetPresentationTarget();

			var worldScreen = presentationTarget.WorldScreen;
			var uiScreen = worldScreen.UI;
			var renderer = uiScreen.Renderer;
			var spriteFont = renderer.GetFont("Tahoma14Bold");

			var text = String.Empty;

			if (_isShiftDown)
				text = "Append";
			if (_isAltDown)
				text = "Replace";

			var position = (Vector2)_position + new Vector2(10.0f, -10.0f);

			spriteBatch.DrawString(spriteFont, text, position + new Vector2(1f, 1f),
				Color.Black);
			spriteBatch.DrawString(spriteFont, text, position,
				Color.Yellow);
		}
	}
}