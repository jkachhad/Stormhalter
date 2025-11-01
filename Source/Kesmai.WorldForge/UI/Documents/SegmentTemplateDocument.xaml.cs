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
        
        var messenger = WeakReferenceMessenger.Default;
		
        messenger.Register<ActiveContentChanged>(this, (_, message) =>
        {
            if (message.Value is not SegmentTemplate segmentTemplate)
                return;
            
            _presenter.Template = _segmentTemplate = segmentTemplate;
            _presenter.Focus();
            
            _presenter.Invalidate();
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
        
        _presenter.Invalidate();
    }

    private void OnRemoveComponentClick(object sender, RoutedEventArgs e)
    {
        if (_segmentTemplate is null)
            return;

        if (_providersList.SelectedItem is not IComponentProvider provider)
            return;

        _segmentTemplate.Providers.Remove(provider);
        
        _presenter.Invalidate();
    }
}

public class SegmentTemplateViewModel : ObservableRecipient
{
    public string Name => "(Template)";
}
