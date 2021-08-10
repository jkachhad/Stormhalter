using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge.UI
{
	public class TextureImage : Image
	{
		public static readonly DependencyProperty TextureProperty = DependencyProperty.Register(
			nameof(Texture), typeof(Texture2D), typeof(TextureImage), 
			new PropertyMetadata(default(Texture2D), OnTextureChanged));

		public Texture2D Texture
		{
			get => (Texture2D)GetValue(TextureProperty);
			set => SetValue(TextureProperty, value);
		}
		
		private static void OnTextureChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			if (sender is TextureImage textureImage && args.NewValue is Texture2D texture)
				textureImage.UpdateTexture(texture);
		}

		internal void UpdateTexture(Texture2D texture)
		{
			Source = texture.ToBitmapSource();
		}
	}
}