using System;
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
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
        var componentImageCache = ServiceLocator.Current.GetInstance<ComponentImageCache>();

        if (componentImageCache != null)
            Source = componentImageCache.Get(component);
    }
}