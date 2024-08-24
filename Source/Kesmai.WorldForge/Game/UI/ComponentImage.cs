using System;
using System.Collections.Generic;
using DigitalRune.Game;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge;

public class ComponentImage : UIControl
{
	private List<ComponentRender> _renders;
		
	public static readonly int ComponentPropertyId = CreateProperty(
		typeof(ComponentImage), "Component", GamePropertyCategories.Default, null, default(TerrainComponent),
		UIPropertyOptions.AffectsRender);

	public TerrainComponent Component
	{
		get => GetValue<TerrainComponent>(ComponentPropertyId);
		set => SetValue(ComponentPropertyId, value);
	}

	public ComponentImage()
	{
		_renders = new List<ComponentRender>();
			
		Width = 100;
		Height = 100;

		Properties.Get<TerrainComponent>(ComponentPropertyId).Changed += (sender, args) =>
		{
			_renders.Clear();
				
			var newValue = args.NewValue;

			if (newValue != null)
				_renders.AddRange(newValue.GetTerrain());
		};
	}
		
	protected override Vector2F OnMeasure(Vector2F availableSize)
	{
		var padding = Padding;
			
		var width = Math.Min(100, Width);
		var height = Math.Min(100, Height);

		if (width > availableSize.X)
			width = availableSize.X;

		if (height > availableSize.Y)
			height = availableSize.Y;

		width += (padding.X + padding.Z);
		height += (padding.Y + padding.W);
			
		return new Vector2F(width, height);
	}
		
	protected override void OnRender(UIRenderContext context)
	{
		base.OnRender(context);

		if (_renders.Count > 0)
		{
			var screen = Screen ?? context.Screen;
			var renderer = screen.Renderer;
			var spriteBatch = renderer.SpriteBatch;

			var originalBounds = ActualBounds.ToRectangle(true);
				
			foreach (var render in _renders)
			{
				foreach (var layer in render.Terrain)
				{
					var sprite = layer.Sprite;

					if (sprite != null)
					{
						var spriteBounds = originalBounds;

						if (sprite.Offset != Vector2F.Zero)
							spriteBounds.Offset(sprite.Offset.X, sprite.Offset.Y);
							
						spriteBatch.Draw(sprite.Texture, originalBounds,  render.Color);
					}
				}
			}
		}
	}
}