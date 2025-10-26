using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using CommonServiceLocator;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.UI;

public class TerrainComponentImage : Image
{
    public static readonly DependencyProperty ProviderProperty = DependencyProperty.Register(
        nameof(Provider), typeof(IComponentProvider), typeof(TerrainComponentImage),
        new PropertyMetadata(default(TerrainComponent), OnComponentChanged));

    public IComponentProvider Provider
    {
        get => (IComponentProvider)GetValue(ProviderProperty);
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

        WeakReferenceMessenger.Default.Register<TerrainComponentImage, SegmentTemplateChanged>(this, static (recipient, message) =>
        {
            recipient.OnSegmentTemplateChanged(message.Value);
        });
    }

    private void OnSegmentTemplateChanged(SegmentTemplate template)
    {
        if (ReferenceEquals(Provider, template))
            UpdateComponent(template, forceRefresh: true);
    }

    internal void UpdateComponent(IComponentProvider provider)
    {
        UpdateComponent(provider, forceRefresh: false);
    }

    private void UpdateComponent(IComponentProvider provider, bool forceRefresh)
    {
        if (provider is null)
            return;

        var componentImageCache = ServiceLocator.Current.GetInstance<ComponentImageCache>();

        if (componentImageCache is null)
            return;

        if (forceRefresh)
            Source = componentImageCache.Update(provider, true);
        else
            Source = componentImageCache.Get(provider);
    }
}
