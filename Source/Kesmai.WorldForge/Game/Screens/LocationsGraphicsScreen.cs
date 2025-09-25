using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge;

public class LocationsGraphicsScreen : WorldGraphicsScreen
{
	private static Color _highlightColor = Color.FromNonPremultiplied(0, 255, 255, 200);
		
	private int _mx;
	private int _my;
		
	public LocationsGraphicsScreen(IGraphicsService graphicsService, WorldPresentationTarget worldPresentationTarget) : base(graphicsService, worldPresentationTarget)
	{
		DrawGrid = true;
		Gridcolor = Color.FromNonPremultiplied(154, 205, 50, 200);
	}

	public void SetLocation(int mx, int my)
	{
		_mx = mx;
		_my = my;
			
		CenterCameraOn(_mx, _my);
	}

	protected override void OnAfterRender(SpriteBatch spriteBatch)
	{
		base.OnAfterRender(spriteBatch);

		var viewRectangle = GetViewRectangle();

		if (viewRectangle.Contains(_mx, _my))
		{
			var bounds = GetRenderRectangle(viewRectangle, _mx, _my);
			var innerRectangle = new Rectangle(bounds.Left, bounds.Top, 55, 20);
				
			spriteBatch.DrawRectangle(bounds, _highlightColor);
			spriteBatch.FillRectangle(innerRectangle, _highlightColor);
				
			_font.DrawString(spriteBatch, RenderTransform.Identity, $"{_mx}, {_my}", 
				new Vector2(bounds.Left + 4, bounds.Top + 3), Color.Black);
		}
	}
}