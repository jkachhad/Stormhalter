using System;
using System.Linq;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.UI.Documents;

public partial class RegionSpawnDocument : UserControl
{
    public RegionSpawnDocument()
    {
        InitializeComponent();

        var messenger = WeakReferenceMessenger.Default;
        
        messenger.Register<ActiveContentChanged>(this, (_, message) =>
        {
            if (message.Value is not RegionSpawner regionSpawn)
                return;
            
            var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
            var segment = segmentRequest.Response;
            
            _presenter.Region = segment.GetRegion(regionSpawn.Region);
            _presenter.SetSpawner(regionSpawn);
            
            _scriptsTabControl.SelectedItem = regionSpawn.Scripts.FirstOrDefault(s => s.IsEnabled);
        });
    }
}

public class RegionSpawnViewModel : ObservableRecipient
{
    public string Name => "(Spawn)";
}