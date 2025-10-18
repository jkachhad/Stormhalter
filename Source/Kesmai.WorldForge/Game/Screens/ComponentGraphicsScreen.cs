using DigitalRune.Game.Interop;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge;

public class ComponentGraphicsScreen : InteropGraphicsScreen
{
	public ComponentGraphicsScreen(IGraphicsService graphicsService, ComponentPresentationTarget presentationTarget) : base(graphicsService, presentationTarget)
	{
	}

	public void Initialize()
	{
	}

	protected override void OnRender(RenderContext context)
	{
		base.OnRender(context);

		var graphicsService = context.GraphicsService;
		var graphicsDevice = graphicsService.GraphicsDevice;
		var spriteBatch = graphicsService.GetSpriteBatch();
		
		graphicsDevice.Clear(Color.Black);

		spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);
		
		// render the component.
		
		spriteBatch.End();
	}
}
