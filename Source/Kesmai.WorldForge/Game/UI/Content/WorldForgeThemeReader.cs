using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge.Game.UI.Content;

internal sealed class WorldForgeThemeReader : ContentTypeReader<Theme>
{
	public const string DigitalRuneThemeReaderType =
		"DigitalRune.Game.UI.Content.ThemeReader, DigitalRune.Game.UI, Version=1.8.0.0, Culture=neutral, PublicKeyToken=null";

	protected override Theme Read(ContentReader input, Theme existingInstance)
	{
		var theme = existingInstance ?? new Theme();

		theme.Cursors.Clear();
		theme.Fonts.Clear();
		theme.Styles.Clear();
		theme.Textures.Clear();
		theme.Content = input.ContentManager;

		ReadCursors(input, theme);
		ReadFonts(input, theme);
		ReadTextures(input, theme);
		ReadStyles(input, theme);

		return theme;
	}

	private static void ReadCursors(ContentReader input, Theme theme)
	{
		var cursorCount = input.ReadInt32();
		for (var i = 0; i < cursorCount; i++)
		{
			var cursor = new ThemeCursor
			{
				Name = input.ReadString(),
				IsDefault = input.ReadBoolean(),
			};

			// WorldForge does not use themed hardware cursors, but the string is still
			// present in the asset and must be consumed to keep the stream aligned.
			input.ReadString();

			theme.Cursors.Add(cursor);
		}
	}

	private static void ReadFonts(ContentReader input, Theme theme)
	{
		var fontCount = input.ReadInt32();
		for (var i = 0; i < fontCount; i++)
		{
			theme.Fonts.Add(new ThemeFont
			{
				Name = input.ReadString(),
				IsDefault = input.ReadBoolean(),
				Font = input.ReadExternalReference<MSDFont>(),
			});
		}
	}

	private static void ReadTextures(ContentReader input, Theme theme)
	{
		var textureCount = input.ReadInt32();
		for (var i = 0; i < textureCount; i++)
		{
			theme.Textures.Add(new ThemeTexture
			{
				Name = input.ReadString(),
				IsDefault = input.ReadBoolean(),
				Texture = input.ReadExternalReference<Texture2D>(),
			});
		}
	}

	private static void ReadStyles(ContentReader input, Theme theme)
	{
		var styleCount = input.ReadInt32();
		var inheritedStyleNames = new (ThemeStyle Style, string Inherits)[styleCount];

		for (var i = 0; i < styleCount; i++)
		{
			var style = new ThemeStyle
			{
				Name = input.ReadString(),
			};
			inheritedStyleNames[i] = (style, input.ReadString());

			ReadAttributes(input, style);
			ReadStates(input, theme, style);

			theme.Styles.Add(style);
		}

		foreach (var (style, inherits) in inheritedStyleNames)
		{
			if (!string.IsNullOrEmpty(inherits) && theme.Styles.TryGet(inherits, out var inheritedStyle))
				style.Inherits = inheritedStyle;
		}
	}

	private static void ReadAttributes(ContentReader input, ThemeStyle style)
	{
		var attributeCount = input.ReadInt32();
		for (var i = 0; i < attributeCount; i++)
		{
			style.Attributes.Add(new ThemeAttribute
			{
				Name = input.ReadString(),
				Value = input.ReadString(),
			});
		}
	}

	private static void ReadStates(ContentReader input, Theme theme, ThemeStyle style)
	{
		var stateCount = input.ReadInt32();
		for (var i = 0; i < stateCount; i++)
		{
			var state = new ThemeState
			{
				Name = input.ReadString(),
				IsInherited = input.ReadBoolean(),
			};

			ReadImages(input, theme, state);

			if (input.ReadBoolean())
				state.Background = input.ReadColor();
			if (input.ReadBoolean())
				state.Foreground = input.ReadColor();
			if (input.ReadBoolean())
				state.Opacity = input.ReadSingle();

			style.States.Add(state);
		}
	}

	private static void ReadImages(ContentReader input, Theme theme, ThemeState state)
	{
		var imageCount = input.ReadInt32();
		for (var i = 0; i < imageCount; i++)
		{
			var image = new ThemeImage
			{
				Name = input.ReadString(),
			};

			var textureName = input.ReadString();
			if (!string.IsNullOrEmpty(textureName))
			{
				if (theme.Textures.TryGet(textureName, out var texture))
					image.Texture = texture;

				var sourceRectangle = ReadRectangle(input);
				image.SourceRectangle = sourceRectangle;
				image.Size = new Vector2F(sourceRectangle.Width, sourceRectangle.Height);
				image.Margin = ReadVector4F(input);
				image.HorizontalAlignment = (HorizontalAlignment)input.ReadInt32();
				image.VerticalAlignment = (VerticalAlignment)input.ReadInt32();
				image.TileMode = (TileMode)input.ReadInt32();
				image.Border = ReadVector4F(input);
				image.IsOverlay = input.ReadBoolean();
				image.Color = input.ReadColor();
			}

			state.Images.Add(image);
		}
	}

	private static Rectangle ReadRectangle(ContentReader input)
	{
		return new Rectangle(input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
	}

	private static Vector4F ReadVector4F(ContentReader input)
	{
		return new Vector4F(input.ReadSingle(), input.ReadSingle(), input.ReadSingle(), input.ReadSingle());
	}
}
