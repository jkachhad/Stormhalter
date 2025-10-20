using System;
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.UI;

public class TerrainComponentImage : Image
{
    public static readonly DependencyProperty ProviderProperty = DependencyProperty.Register(
        nameof(Provider), typeof(IComponentProvider), typeof(TerrainComponentImage),
        new PropertyMetadata(default(TerrainComponent), OnComponentChanged));

    public IComponentProvider Provider
    {
        get => (TerrainComponent)GetValue(ProviderProperty);
        set => SetValue(ProviderProperty, value);
    }

    private static void OnComponentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        if (sender is TerrainComponentImage componentImage && args.NewValue is IComponentProvider provider)
            componentImage.UpdateComponent(provider);
    }

    static TerrainComponentImage()
    {
        WidthProperty.OverrideMetadata(typeof(TerrainComponentImage), new FrameworkPropertyMetadata((double)100));
        HeightProperty.OverrideMetadata(typeof(TerrainComponentImage), new FrameworkPropertyMetadata((double)100));
    }

    public TerrainComponentImage()
    {
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
    }

    internal void UpdateComponent(IComponentProvider provider)
    {
        var componentImageCache = ServiceLocator.Current.GetInstance<ComponentImageCache>();

        if (componentImageCache != null)
            Source = componentImageCache.Get(provider.Component);
    }
}