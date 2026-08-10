using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Content;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Content;
using DigitalRune.Storages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge.Game.UI.Content;

/// <summary>
/// Registers compatibility readers required by legacy XNA-built DigitalRune
/// themes when they are loaded by MonoGame.
/// </summary>
internal static class WorldForgeContentReaders
{
    internal const string DigitalRuneThemeReaderType =
        "DigitalRune.Game.UI.Content.ThemeReader, DigitalRune.Game.UI, Version=1.8.0.0, Culture=neutral, PublicKeyToken=null";

    public static void Register()
    {
        RegisterReader<WorldForgeThemeReader>(DigitalRuneThemeReaderType);
        RegisterReader<XnaRectangleReader>("Microsoft.Xna.Framework.Content.RectangleReader, Microsoft.Xna.Framework");
        RegisterReader<XnaRectangleReader>(
            "Microsoft.Xna.Framework.Content.RectangleReader, Microsoft.Xna.Framework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553");
        RegisterReader<XnaRectangleReader>("Microsoft.Xna.Framework.Content.RectangleReader, MonoGame.Framework");
        RegisterReader<XnaRectangleReader>(
            "Microsoft.Xna.Framework.Content.RectangleReader, MonoGame.Framework, Version=3.8.2.1105, Culture=neutral, PublicKeyToken=null");
        RegisterReader<Vector2FReader>(
            "DigitalRune.Mathematics.Content.Vector2FReader, DigitalRune.Mathematics, Version=1.14.0.0, Culture=neutral, PublicKeyToken=null");
        RegisterReader<Vector4FReader>(
            "DigitalRune.Mathematics.Content.Vector4FReader, DigitalRune.Mathematics, Version=1.14.0.0, Culture=neutral, PublicKeyToken=null");
    }

    private static void RegisterReader<TReader>(string readerTypeName)
        where TReader : ContentTypeReader, new()
    {
        ContentTypeReaderManager.AddTypeCreator(readerTypeName, () => new TReader());
    }
}

internal sealed class WorldForgeThemeReader : ContentTypeReader<Theme>
{
    private static readonly Type PlatformHelperType =
        typeof(DigitalRune.Storages.Path).Assembly.GetType("DigitalRune.PlatformHelper", true);

    private static readonly MethodInfo CreateCursorFromFileMethod =
        PlatformHelperType.GetMethod("CreateCursor", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null, new[] { typeof(string) }, null);

    private static readonly MethodInfo CreateCursorFromStreamMethod =
        PlatformHelperType.GetMethod("CreateCursor", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null, new[] { typeof(Stream) }, null);

    protected override Theme Read(ContentReader input, Theme existingInstance)
    {
        var theme = existingInstance ?? new Theme();
        theme.Content = input.ContentManager;

        var cursorCount = input.ReadInt32();
        for (var i = 0; i < cursorCount; i++)
        {
            theme.Cursors.Add(new ThemeCursor
            {
                Name = input.ReadString(),
                IsDefault = input.ReadBoolean(),
                Cursor = LoadCursor(input),
            });
        }

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

        var inheritedStyles = new Dictionary<ThemeStyle, string>();
        var styleCount = input.ReadInt32();
        for (var i = 0; i < styleCount; i++)
        {
            var style = new ThemeStyle { Name = input.ReadString() };
            inheritedStyles.Add(style, input.ReadString());

            var attributeCount = input.ReadInt32();
            for (var j = 0; j < attributeCount; j++)
            {
                style.Attributes.Add(new ThemeAttribute
                {
                    Name = input.ReadString(),
                    Value = input.ReadString(),
                });
            }

            var stateCount = input.ReadInt32();
            for (var j = 0; j < stateCount; j++)
            {
                var state = new ThemeState
                {
                    Name = input.ReadString(),
                    IsInherited = input.ReadBoolean(),
                };

                var imageCount = input.ReadInt32();
                for (var k = 0; k < imageCount; k++)
                {
                    var image = new ThemeImage { Name = input.ReadString() };
                    var textureName = input.ReadString();
                    if (!String.IsNullOrEmpty(textureName) &&
                        theme.Textures.TryGet(textureName, out var texture))
                        image.Texture = texture;

                    image.SourceRectangle = new Rectangle(
                        input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    image.Size = new Vector2F(input.ReadSingle(), input.ReadSingle());
                    image.Margin = new Vector4F(
                        input.ReadSingle(), input.ReadSingle(), input.ReadSingle(), input.ReadSingle());
                    image.HorizontalAlignment = (HorizontalAlignment)input.ReadInt32();
                    image.VerticalAlignment = (VerticalAlignment)input.ReadInt32();
                    image.TileMode = (TileMode)input.ReadInt32();
                    image.Border = new Vector4F(
                        input.ReadSingle(), input.ReadSingle(), input.ReadSingle(), input.ReadSingle());
                    image.IsOverlay = input.ReadBoolean();
                    image.Color = input.ReadColor();
                    state.Images.Add(image);
                }

                if (input.ReadBoolean())
                    state.Background = input.ReadColor();
                if (input.ReadBoolean())
                    state.Foreground = input.ReadColor();
                if (input.ReadBoolean())
                    state.Opacity = input.ReadSingle();

                style.States.Add(state);
            }

            theme.Styles.Add(style);
        }

        foreach (var inheritedStyle in inheritedStyles)
        {
            if (!String.IsNullOrEmpty(inheritedStyle.Value) &&
                theme.Styles.TryGet(inheritedStyle.Value, out var baseStyle))
                inheritedStyle.Key.Inherits = baseStyle;
        }

        return theme;
    }

    private static object LoadCursor(ContentReader input)
    {
        var cursorFile = input.ReadString();
        var assetDirectory = DigitalRune.Storages.Path.GetDirectoryName(input.AssetName) ?? String.Empty;

        try
        {
            var fileName = DigitalRune.Storages.Path.Combine(
                input.ContentManager.RootDirectory, assetDirectory, cursorFile);
            var cursor = CreateCursorFromFileMethod.Invoke(null, new object[] { fileName });
            if (cursor is not null)
                return cursor;

            if (input.ContentManager is IStorageProvider storageProvider)
            {
                fileName = DigitalRune.Storages.Path.Combine(assetDirectory, cursorFile);
                using var stream = storageProvider.Storage.OpenFile(fileName);
                return CreateCursorFromStreamMethod.Invoke(null, new object[] { stream });
            }
        }
        catch (ArgumentException)
        {
            return null;
        }

        return null;
    }
}

internal sealed class XnaRectangleReader : ContentTypeReader<Rectangle>
{
    protected override Rectangle Read(ContentReader input, Rectangle existingInstance)
    {
        return new Rectangle(input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
    }
}
