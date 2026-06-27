using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Kesmai.WorldForge.Game.UI.Content;

internal static class WorldForgeContentReaders
{
	public static void Register()
	{
		RegisterReader<WorldForgeThemeReader>(WorldForgeThemeReader.DigitalRuneThemeReaderType);
		RegisterRectangleReader();
		RegisterReader<Vector2FReader>(
			"DigitalRune.Mathematics.Content.Vector2FReader, DigitalRune.Mathematics, Version=1.14.0.0, Culture=neutral, PublicKeyToken=null");
		RegisterReader<Vector4FReader>(
			"DigitalRune.Mathematics.Content.Vector4FReader, DigitalRune.Mathematics, Version=1.14.0.0, Culture=neutral, PublicKeyToken=null");
	}

	private static void RegisterRectangleReader()
	{
		RegisterReader<XnaRectangleReader>(
			"Microsoft.Xna.Framework.Content.RectangleReader, Microsoft.Xna.Framework");
		RegisterReader<XnaRectangleReader>(
			"Microsoft.Xna.Framework.Content.RectangleReader, Microsoft.Xna.Framework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553");
		RegisterReader<XnaRectangleReader>(
			"Microsoft.Xna.Framework.Content.RectangleReader, MonoGame.Framework");
		RegisterReader<XnaRectangleReader>(
			"Microsoft.Xna.Framework.Content.RectangleReader, MonoGame.Framework, Version=3.8.2.1105, Culture=neutral, PublicKeyToken=null");
	}

	private static void RegisterReader<TReader>(string readerTypeName)
		where TReader : ContentTypeReader, new()
	{
		ContentTypeReaderManager.AddTypeCreator(readerTypeName, () => new TReader());
	}

	private sealed class XnaRectangleReader : ContentTypeReader<Rectangle>
	{
		protected override Rectangle Read(ContentReader input, Rectangle existingInstance)
		{
			return new Rectangle(input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
		}
	}
}
