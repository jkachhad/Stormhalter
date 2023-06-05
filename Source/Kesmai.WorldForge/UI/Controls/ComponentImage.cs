using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.UI;

public class ComponentImage : Image
{
	public static readonly DependencyProperty ComponentProperty = DependencyProperty.Register(
		nameof(Component), typeof(TerrainComponent), typeof(ComponentImage), 
		new PropertyMetadata(default(TerrainComponent), OnTextureChanged));

	public TerrainComponent Component
	{
		get => (TerrainComponent)GetValue(ComponentProperty);
		set => SetValue(ComponentProperty, value);
	}
		
	private static void OnTextureChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
	{
		if (sender is ComponentImage componentImage && args.NewValue is TerrainComponent component)
			componentImage.UpdateComponent(component);
	}

	static ComponentImage()
	{
		WidthProperty.OverrideMetadata(typeof(ComponentImage), new FrameworkPropertyMetadata((double)100));
		HeightProperty.OverrideMetadata(typeof(ComponentImage), new FrameworkPropertyMetadata((double)100));
	}
		
	public ComponentImage()
	{
		HorizontalAlignment = HorizontalAlignment.Left;
		VerticalAlignment = VerticalAlignment.Top;
	}

	internal void UpdateComponent(TerrainComponent component)
	{
		var maxWidth = 100;
		var maxHeight = 100;

		foreach (var terrain in component.GetTerrain())
		{
			foreach (var layer in terrain.Terrain)
			{
				var sprite = layer.Sprite;

				if (sprite != null)
				{
					maxWidth = Math.Max(sprite.Texture.Width, maxWidth);
					maxHeight = Math.Max(sprite.Texture.Height, maxHeight);
				}
			}
		}

		var writeableBitmap = BitmapFactory.New(maxWidth, maxHeight);

		writeableBitmap.Lock();
		
		foreach (var terrain in component.GetTerrain())
		{
			foreach (var layer in terrain.Terrain)
			{
				var sprite = layer.Sprite;

				if (sprite != null && sprite.Bitmap != null)
				{
					var spriteBounds = new Vector2F(0, 0);

					if (sprite.Offset != Vector2F.Zero)
						spriteBounds += new Vector2F(sprite.Offset.X, sprite.Offset.Y);

					writeableBitmap.Blit(
						new Rect(spriteBounds.X, spriteBounds.Y, maxWidth, maxHeight), sprite.Bitmap,
						new Rect(0, 0, sprite.Texture.Width, sprite.Texture.Height),
						WriteableBitmapExtensions.BlendMode.Additive);
				}
			}
		}

		writeableBitmap.Unlock();
		writeableBitmap.Freeze();
			
		Source = writeableBitmap;
	}
}