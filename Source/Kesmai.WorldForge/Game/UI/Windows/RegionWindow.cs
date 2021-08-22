using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using Kesmai.WorldForge.Editor;
using Microsoft.Xna.Framework.Input;

namespace Kesmai.WorldForge.Windows
{
	public class RegionWindow : Window
	{
		private SegmentRegion _region;
		
		public RegionWindow(SegmentRegion region)
		{
			_region = region;

			Title = $"Editing region {_region.Name} ({_region.ID})";
			
			VerticalAlignment = VerticalAlignment.Center;
			HorizontalAlignment = HorizontalAlignment.Center;
		}

		protected override void OnLoad()
		{
			base.OnLoad();
			
			var propertyGrid = new PropertyGrid()
			{
				VerticalAlignment = VerticalAlignment.Stretch,
				HorizontalAlignment = HorizontalAlignment.Stretch,
			};
			
			Content = propertyGrid;
			
			propertyGrid.Item = _region;
		}
		
		protected override void OnHandleInput(InputContext context)
		{
			if (!IsVisible)
				return;
			
			base.OnHandleInput(context);

			var inputService = InputService;

			if (inputService == null)
				return;

			if (!inputService.IsKeyboardHandled)
			{
				if (inputService.IsReleased(Keys.Escape))
				{
					Close();
				}
			}
			
			if (IsActive)
				inputService.IsKeyboardHandled = true;
		}
	}
}