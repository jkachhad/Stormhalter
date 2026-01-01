using System;
using DigitalRune.Game.Interop;
using DigitalRune.Game.UI;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge;

public class ComponentGraphicsScreen : InteropGraphicsScreen
{
	private ComponentPresentationTarget _target;
	
	public ComponentGraphicsScreen(IGraphicsService graphicsService, ComponentPresentationTarget presentationTarget) : base(graphicsService, presentationTarget)
	{
		_target = presentationTarget;
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
		
		// render the component at the center.
		var viewport = graphicsDevice.Viewport;
		var cx = (viewport.Width - 100) / 2;
		var cy = (viewport.Height - 100) / 2;
		
		// calculate bounds
		var bounds = new Rectangle(cx - 100, cy - 100, 200, 200);
		
		var provider = _target.Provider;

		if (_target.Provider is not null)
		{
			var renders = provider.GetRenders();

			foreach (var render in renders)
			{
				foreach (var layer in render.Terrain)
				{
					var sprite = layer.Sprite;

					if (sprite is null)
						continue;

					var spriteBounds = bounds;

					if (sprite.Offset != Vector2F.Zero)
						spriteBounds.Offset((int)Math.Floor(sprite.Offset.X), (int)Math.Floor(sprite.Offset.Y));

					spriteBatch.Draw(sprite.Texture, spriteBounds, render.Color);
				}
			}
		}

		spriteBatch.End();
	}
}
