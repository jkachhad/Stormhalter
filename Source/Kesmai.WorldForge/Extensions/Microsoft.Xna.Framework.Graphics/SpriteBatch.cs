using System;
using System.Collections.Generic;
using DigitalRune.Game.UI;

namespace Microsoft.Xna.Framework.Graphics;

public static class SpriteBatchExtensions
{
	#region Static

	private static Texture2D _pixel;
	private static readonly Dictionary<String, List<Vector2>> _circleCache = new Dictionary<string, List<Vector2>>();

	private static void CreatePixel(SpriteBatch spriteBatch)
	{
		_pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
		_pixel.SetData(new[] { Color.White });
	}

	/// <summary>
	/// Draws a list of connecting points.
	/// </summary>
	private static void DrawPoints(SpriteBatch spriteBatch, Vector2 position, List<Vector2> points, Color color, float thickness)
	{
		if (points.Count < 2)
			return;

		for (int i = 1; i < points.Count; i++)
			DrawLine(spriteBatch, points[i - 1] + position, points[i] + position, color, thickness);
	}

	/// <summary>
	/// Creates a list of vectors that represents a circle.
	/// </summary>
	private static List<Vector2> CreateCircle(double radius, int sides)
	{
		var circleKey = String.Format("{0}x{0}", radius);

		if (_circleCache.ContainsKey(circleKey))
			return _circleCache[circleKey];

		var vectors = new List<Vector2>();

		var max = 2.0 * Math.PI;
		var step = max / sides;

		for (double theta = 0.0; theta < max; theta += step)
			vectors.Add(new Vector2((float)(radius * Math.Cos(theta)), (float)(radius * Math.Sin(theta))));

		vectors.Add(new Vector2((float)(radius * Math.Cos(0)), (float)(radius * Math.Sin(0))));

		_circleCache.Add(circleKey, vectors);

		return vectors;
	}

	/// <summary>
	/// Creates a list of vectors that represents an arc.
	/// </summary>
	private static List<Vector2> CreateArc(float radius, int sides, float startingAngle, float radians)
	{
		var points = new List<Vector2>();

		points.AddRange(CreateCircle(radius, sides));
		points.RemoveAt(points.Count - 1);

		var curAngle = 0.0;
		var anglePerSide = MathHelper.TwoPi / sides;

		while ((curAngle + (anglePerSide / 2.0)) < startingAngle)
		{
			curAngle += anglePerSide;

			points.Add(points[0]);
			points.RemoveAt(0);
		}

		points.Add(points[0]);

		var sidesInArc = (int)((radians / anglePerSide) + 0.5);

		points.RemoveRange(sidesInArc + 1, points.Count - sidesInArc - 1);

		return points;
	}

	/// <summary>
	/// Draws a filled rectangle.
	/// </summary>
	public static void FillRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color color)
	{
		if (_pixel == null)
			CreatePixel(spriteBatch);

		spriteBatch.Draw(_pixel, rect, color);
	}

	/// <summary>
	/// Draws a filled rectangle.
	/// </summary>
	public static void FillRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color color, float angle)
	{
		if (_pixel == null)
			CreatePixel(spriteBatch);

		spriteBatch.Draw(_pixel, rect, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
	}

	/// <summary>
	/// Draws a filled rectangle.
	/// </summary>
	public static void FillRectangle(this SpriteBatch spriteBatch, Vector2 location, Vector2 size, Color color)
	{
		FillRectangle(spriteBatch, location, size, color, 0.0f);
	}

	/// <summary>
	/// Draws a filled rectangle.
	/// </summary>
	public static void FillRectangle(this SpriteBatch spriteBatch, Vector2 location, Vector2 size, Color color, float angle)
	{
		if (_pixel == null)
			CreatePixel(spriteBatch);

		spriteBatch.Draw(_pixel, location, null, color, angle, Vector2.Zero, size, SpriteEffects.None, 0);
	}

	/// <summary>
	/// Draws a filled rectangle.
	/// </summary>
	public static void FillRectangle(this SpriteBatch spriteBatch, float x, float y, float w, float h, Color color)
	{
		FillRectangle(spriteBatch, new Vector2(x, y), new Vector2(w, h), color, 0.0f);
	}

	/// <summary>
	/// Draws a filled rectangle.
	/// </summary>
	public static void FillRectangle(this SpriteBatch spriteBatch, float x, float y, float w, float h, Color color, float angle)
	{
		FillRectangle(spriteBatch, new Vector2(x, y), new Vector2(w, h), color, angle);
	}

	/// <summary>
	/// Draws a rectangle with the thickness provided.
	/// </summary>
	public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color color)
	{
		DrawRectangle(spriteBatch, rect, color, 1.0f);
	}

	/// <summary>
	/// Draws a rectangle with the thickness provided.
	/// </summary>
	public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color color, float thickness)
	{
		DrawLine(spriteBatch, new Vector2(rect.X, rect.Y), new Vector2(rect.Right, rect.Y), color, thickness); // top
		DrawLine(spriteBatch, new Vector2(rect.X + 1f, rect.Y), new Vector2(rect.X + 1f, rect.Bottom + thickness), color, thickness); // left
		DrawLine(spriteBatch, new Vector2(rect.X, rect.Bottom), new Vector2(rect.Right, rect.Bottom), color, thickness); // bottom
		DrawLine(spriteBatch, new Vector2(rect.Right + 1f, rect.Y), new Vector2(rect.Right + 1f, rect.Bottom + thickness), color, thickness); // right
	}

	/// <summary>
	/// Draws a rectangle with the thickness provided.
	/// </summary>
	public static void DrawRectangle(this SpriteBatch spriteBatch, Vector2 location, Vector2 size, Color color)
	{
		DrawRectangle(spriteBatch, new Rectangle((int)location.X, (int)location.Y, (int)size.X, (int)size.Y), color, 1.0f);
	}

	/// <summary>
	/// Draws a rectangle with the thickness provided.
	/// </summary>
	public static void DrawRectangle(this SpriteBatch spriteBatch, Vector2 location, Vector2 size, Color color, float thickness)
	{
		DrawRectangle(spriteBatch, new Rectangle((int)location.X, (int)location.Y, (int)size.X, (int)size.Y), color, thickness);
	}

	/// <summary>
	/// Draws a line from point1 to point2 with an offset.
	/// </summary>
	public static void DrawLine(this SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color)
	{
		DrawLine(spriteBatch, new Vector2(x1, y1), new Vector2(x2, y2), color, 1.0f);
	}

	/// <summary>
	/// Draws a line from point1 to point2 with an offset.
	/// </summary>
	public static void DrawLine(this SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color, float thickness)
	{
		DrawLine(spriteBatch, new Vector2(x1, y1), new Vector2(x2, y2), color, thickness);
	}

	/// <summary>
	/// Draws a line from point1 to point2 with an offset.
	/// </summary>
	public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color)
	{
		DrawLine(spriteBatch, point1, point2, color, 1.0f);
	}

	/// <summary>
	/// Draws a line from point1 to point2 with an offset.
	/// </summary>
	public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness)
	{
		var distance = Vector2.Distance(point1, point2);
		var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

		DrawLine(spriteBatch, point1, distance, angle, color, thickness);
	}

	/// <summary>
	/// Draws a line from point1 to point2 with an offset.
	/// </summary>
	public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color)
	{
		DrawLine(spriteBatch, point, length, angle, color, 1.0f);
	}

	/// <summary>
	/// Draws a line from point1 to point2 with an offset.
	/// </summary>
	public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness)
	{
		if (_pixel == null)
			CreatePixel(spriteBatch);

		spriteBatch.Draw(_pixel, point, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0);
	}

	/// <summary>
	/// Draws a pixel.
	/// </summary>
	public static void DrawPixel(this SpriteBatch spriteBatch, float x, float y, Color color)
	{
		DrawPixel(spriteBatch, new Vector2(x, y), color);
	}

	/// <summary>
	/// Draws a pixel.
	/// </summary>
	public static void DrawPixel(this SpriteBatch spriteBatch, Vector2 position, Color color)
	{
		if (_pixel == null)
			CreatePixel(spriteBatch);

		spriteBatch.Draw(_pixel, position, color);
	}

	/// <summary>
	/// Draw a circle.
	/// </summary>
	public static void DrawCircle(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, Color color)
	{
		DrawPoints(spriteBatch, center, CreateCircle(radius, sides), color, 1.0f);
	}

	/// <summary>
	/// Draw a circle.
	/// </summary>
	public static void DrawCircle(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, Color color, float thickness)
	{
		DrawPoints(spriteBatch, center, CreateCircle(radius, sides), color, thickness);
	}

	/// <summary>
	/// Draw a circle.
	/// </summary>
	public static void DrawCircle(this SpriteBatch spriteBatch, float x, float y, float radius, int sides, Color color)
	{
		DrawPoints(spriteBatch, new Vector2(x, y), CreateCircle(radius, sides), color, 1.0f);
	}

	/// <summary>
	/// Draw an circle.
	/// </summary>
	public static void DrawCircle(this SpriteBatch spriteBatch, float x, float y, float radius, int sides, Color color, float thickness)
	{
		DrawPoints(spriteBatch, new Vector2(x, y), CreateCircle(radius, sides), color, thickness);
	}

	/// <summary>
	/// Draw an arc.
	/// </summary>
	public static void DrawArc(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, float startingAngle, float radians, Color color)
	{
		DrawArc(spriteBatch, center, radius, sides, startingAngle, radians, color, 1.0f);
	}

	/// <summary>
	/// Draw an arc.
	/// </summary>
	public static void DrawArc(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, float startingAngle, float radians, Color color, float thickness)
	{
		DrawPoints(spriteBatch, center, CreateArc(radius, sides, startingAngle, radians), color, thickness);
	}


	/// <summary>
	/// Draws the string with an outline.
	/// </summary>
#if BITMAPFONT
		public static void DrawStringOutlined(this SpriteBatch spriteBatch, BitmapFont spriteFont, string text, Vector2 position, Color textColor, Color outlineColor, int borderWidth = 1)
#else
	public static void DrawStringOutlined(this SpriteBatch spriteBatch, SpriteFont spriteFont, string text, Vector2 position, Color textColor, Color outlineColor, int borderWidth = 1)
#endif
	{
		for (var i = (-1 * borderWidth); i <= (1 * borderWidth); i++)
		for (var j = (-1 * borderWidth); j <= (1 * borderWidth); j++)
			spriteBatch.DrawString(spriteFont, text, new Vector2(position.X + i, position.Y + j), outlineColor);

		spriteBatch.DrawString(spriteFont, text, position, textColor);
	}


	#endregion
}