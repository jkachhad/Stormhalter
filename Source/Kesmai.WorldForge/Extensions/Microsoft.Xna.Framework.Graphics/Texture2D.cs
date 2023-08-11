using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Xna.Framework.Graphics;

public static class Texture2DExtensions
{
	#region Static

	/// <summary>
	/// Gets a copy of Texture2D data.
	/// </summary>
	public static T[] GetData<T>(this Texture2D texture) where T : struct
	{
		var data = new T[texture.Width * texture.Height];

		texture.GetData<T>(data);

		return data;
	}


	/// <summary>
	/// Determines whether the specified pixel is transparent.
	/// </summary>
	/// <param name="fullTransparency">If set to <c>true</c>, pixes are 
	/// considered transparent only if the alpha is 0. If set to 
	/// <c>false</c>, alpha values greather than zero are not considered 
	/// transparent.</param>
	public static bool IsTransparent(this Texture2D texture, int x, int y, bool fullTransparency = false)
	{
		if (x < 0 || y < 0 || x >= texture.Width || y >= texture.Height)
			return true;

		var color = texture.GetPixel(x, y);

		if (color.A > 0)
		{
			if (color.A < 255 && !fullTransparency)
				return true;

			return false;
		}

		return true;
	}

	/// <summary>
	/// Gets the average color of the pixels.
	/// </summary>
	public static Color GetPixel(this Texture2D texture, int x, int y)
	{
		var data = new Color[1];

		if (x < 0 || y < 0 || x >= texture.Width || y >= texture.Height)
			return Color.Black;

		texture.GetData<Color>(0, new Rectangle(x, y, 1, 1), data, 0, 1);

		return data[0];
	}

	/// <summary>
	/// Gets the average color.
	/// </summary>
	public static Color GetAverageColor(this Texture2D texture)
	{
		var data = texture.GetData<Color>();

		if (data.Length != 0)
		{
			var r = data.Sum(t => t.R) / data.Length;
			var g = data.Sum(t => t.G) / data.Length;
			var b = data.Sum(t => t.B) / data.Length;
			var a = data.Sum(t => t.A) / data.Length;

			return new Color(r, g, b, a);
		}

		return Color.Transparent;
	}

	public static WriteableBitmap ToBitmapSource(this Texture2D texture)
	{
		var bmp = new WriteableBitmap(texture.Width, texture.Height, 96, 96, PixelFormats.Bgra32, null);
		var pixelData = new int[texture.Width * texture.Height];
			
		texture.GetData(pixelData);

		bmp.Lock();
			
		unsafe
		{
			var pixels = (int*)bmp.BackBuffer;
				
			for (var i = 0; i < pixelData.Length; i++)
				pixels[i] = ColorToWindows(pixelData[i]);
		}
			
		bmp.AddDirtyRect(new Int32Rect(0, 0, texture.Width, texture.Height));
		bmp.Unlock();
			
		return bmp;
	}
		
	private static int ColorToWindows(int color)
	{
		var a = (byte)(color >> 24);
		var b = (byte)(color >> 16);
		var g = (byte)(color >> 8);
		var r = (byte)(color >> 0);

		return (a << 24)
		       | ((byte)((r * a) >> 8) << 16)
		       | ((byte)((g * a) >> 8) << 8)
		       | ((byte)((b * a) >> 8));
	}

	#endregion
}