using System;
using DigitalRune.Game.Interop;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge;

public class TemplateGraphicsScreen : InteropGraphicsScreen
{
	private readonly TemplatePresentationTarget _presentationTarget;

	public TemplateGraphicsScreen(IGraphicsService graphicsService, TemplatePresentationTarget presentationTarget) : base(graphicsService, presentationTarget)
	{
		_presentationTarget = presentationTarget ?? throw new ArgumentNullException(nameof(presentationTarget));
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

		var template = _presentationTarget.Template;

		if (template is null)
			return;

		spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);

		var width = (int)Math.Max(1, PresentationTarget.ActualWidth);
		var height = (int)Math.Max(1, PresentationTarget.ActualHeight);

		var renderSize = 160;
		var bounds = new Rectangle( (width - renderSize) / 2, (height - renderSize) / 2, renderSize, renderSize);

		foreach (var render in template.GetRenders())
		{
			foreach (var layer in render.Terrain)
			{
				var sprite = layer.Sprite;

				if (sprite is null)
					continue;

				var spriteBounds = bounds;

				if (sprite.Offset != Vector2F.Zero)
				{
					spriteBounds.Offset(
						(int)Math.Floor(sprite.Offset.X),
						(int)Math.Floor(sprite.Offset.Y));
				}

				spriteBatch.Draw( sprite.Texture, spriteBounds, null, render.Color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			}
		}

		spriteBatch.End();
	}
}
