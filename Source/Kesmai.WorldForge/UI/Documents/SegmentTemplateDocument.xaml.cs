using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;

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
}

public class SegmentTemplateViewModel : ObservableRecipient
{
    public string Name => "(Template)";
}
