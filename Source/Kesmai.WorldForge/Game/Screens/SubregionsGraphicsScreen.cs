using System.Linq;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using Kesmai.WorldForge.Editor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge;

public class SubregionsGraphicsScreen : WorldGraphicsScreen
{
	private SegmentSubregion _subregion;
		
	public SubregionsGraphicsScreen(IGraphicsService graphicsService, WorldPresentationTarget worldPresentationTarget) : base(graphicsService, worldPresentationTarget)
	{
	}

	public void SetSubregion(SegmentSubregion subregion)
	{
		_subregion = subregion;

		var first = _subregion.Rectangles.FirstOrDefault();

		if (first != null)
			CenterCameraOn(first.Left, first.Top);
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

				spriteBatch.FillRectangle(bounds, _subregion.Color);
				spriteBatch.DrawRectangle(bounds, _subregion.Border);

				_font.DrawString(spriteBatch, RenderTransform.Identity, _subregion.Name,
					new Vector2(bounds.X + 5, bounds.Y + 5), Color.White);
			}
		}
	}
}