using System;
using System.Linq;
using DigitalRune.Game.Interop;
using DigitalRune.Game.UI;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Editor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge;

public class BrushGraphicsScreen : InteropGraphicsScreen
{
	private readonly BrushPresentationTarget _presentationTarget;

	public BrushGraphicsScreen(IGraphicsService graphicsService, BrushPresentationTarget presentationTarget) : base(graphicsService, presentationTarget)
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

		var brush = _presentationTarget.Brush;

		if (brush is null)
			return;

		var entries = brush.Entries
			.Where(entry => entry.Component is not null && entry.Weight > 0)
			.ToArray();

		if (entries.Length == 0)
			return;

		var totalWeight = entries.Sum(entry => entry.Weight);

		if (totalWeight <= 0)
			return;

		var width = Math.Max(1, (int)_presentationTarget.ActualWidth);
		var height = Math.Max(1, (int)_presentationTarget.ActualHeight);

		var columns = Math.Max(1, width / 55.0f);
		var rows = Math.Max(1, height / 55.0f);
		
		var random = new Random(CreateSeed(brush));

		spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);

		for (var my = 0; my < rows; my++)
		for (var mx = 0; mx < columns; mx++)
		{
			var entry = SelectEntry(entries, totalWeight, random);
			
			if (entry is null || entry.Component is null)
				return;

			var renders = entry.Component.GetRenders();
			
			var bounds = new RectangleF(
				(mx * 55.0f), (my * 55.0f), 55.0f, 55.0f);
			var terrainBounds = new Rectangle(
				(int)Math.Floor(bounds.X - 45), (int)Math.Floor(bounds.Y - 45), 100, 100);
			
			foreach (var render in renders)
			{
				foreach (var layer in render.Terrain)
				{
					var sprite = layer.Sprite;

					if (sprite is null)
						continue;

					var spriteBounds = terrainBounds;

					if (sprite.Offset != Vector2F.Zero)
						spriteBounds.Offset((int)Math.Floor(sprite.Offset.X), (int)Math.Floor(sprite.Offset.Y));

					spriteBatch.Draw(sprite.Texture, spriteBounds.Location.ToVector2(), null, render.Color, 0, 
						Vector2.Zero, 1.0f / sprite.Resolution, SpriteEffects.None, 0f);
				}
			}
		}

		spriteBatch.End();
	}

	private static SegmentBrushEntry SelectEntry(SegmentBrushEntry[] entries, int totalWeight, Random random)
	{
		if (entries.Length == 0 || totalWeight <= 0)
			return null;

		var roll = random.Next(totalWeight);
		var cumulative = 0;

		foreach (var entry in entries)
		{
			cumulative += entry.Weight;

			if (roll < cumulative)
				return entry;
		}

		return entries[^1];
	}

	private static int CreateSeed(SegmentBrush brush)
	{
		if (brush is null)
			return 0;

		var hash = HashCode.Combine(brush.Name ?? string.Empty, brush.TotalWeight, brush.Entries.Count);

		foreach (var entry in brush.Entries)
		{
			hash = HashCode.Combine(hash, entry.Weight, entry.Component?.Name?.GetHashCode() ?? 0);
		}

		return hash;
	}
	
}
