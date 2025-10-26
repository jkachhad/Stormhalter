using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.UI.Windows;

namespace Kesmai.WorldForge.UI.Documents;

public partial class SegmentTemplateDocument : UserControl
{
    private SegmentTemplate _segmentTemplate;

    public SegmentTemplateDocument()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<ActiveContentChanged>(this, (_, message) =>
        {
            _segmentTemplate = message.Value as SegmentTemplate;
        });
    }

    private void OnAddComponentClick(object sender, RoutedEventArgs e)
    {
        if (_segmentTemplate is null)
            return;

        var picker = new ComponentsWindow
        {
            Owner = Window.GetWindow(this)
        };

        var result = picker.ShowDialog();

        if (!result.HasValue || result != true || picker.SelectedComponent is null)
            return;

        _segmentTemplate.Providers.Add(picker.SelectedComponent);

        _providersList.SelectedItem = picker.SelectedComponent;
        _providersList.ScrollIntoView(picker.SelectedComponent);
    }

    private void OnRemoveComponentClick(object sender, RoutedEventArgs e)
    {
        if (_segmentTemplate is null)
            return;

        if (_providersList.SelectedItem is not IComponentProvider provider)
            return;

        _segmentTemplate.Providers.Remove(provider);
    }
}

public class SegmentTemplateViewModel : ObservableRecipient
{
    public string Name => "(Template)";
}
