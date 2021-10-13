using System.Linq;
using System.Windows.Input;
using CommonServiceLocator;
using DigitalRune.Game.Input;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;
using Microsoft.Xna.Framework.Input;

namespace Kesmai.WorldForge
{
	public class PaintTool : Tool
	{
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
				if (inputService.IsReleased(Keys.Escape))
				{
					presenter.SelectTool(default(Tool));
					inputService.IsKeyboardHandled = true;
				}
			}
			
			var (cx, cy) = graphicsScreen.ToWorldCoordinates((int)_position.X, (int)_position.Y);

			if (!selection.Any())
				return;

			var append = (inputService.IsDown(Keys.RightShift) || inputService.IsDown(Keys.LeftShift));
			
			if (inputService.IsReleased(MouseButtons.Left) && selection.IsSelected(cx, cy, region))
			{
				var component = presenter.SelectedComponent;

				if (component != null)
				{
					var componentType = component.GetType();

					foreach (var area in selection)
					{
						for (var x = area.Left; x < area.Right; x++)
						for (var y = area.Top; y < area.Bottom; y++)
						{
							var selectedTile = region.GetTile(x, y);
							
							if (selectedTile == null)
								region.SetTile(x, y, selectedTile = new SegmentTile(x, y));

							if (!append)
							{
								var similar = selectedTile.GetComponents<TerrainComponent>(c => c.GetType().IsAssignableFrom(componentType));

								foreach (var similarComponent in similar)
									selectedTile.RemoveComponent(similarComponent);
							}

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
	}
}