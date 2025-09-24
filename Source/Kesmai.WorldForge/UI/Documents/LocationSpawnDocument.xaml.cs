using System;
using System.Linq;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.UI.Documents;

public partial class LocationSpawnDocument : UserControl
{
    public LocationSpawnDocument()
    {
        InitializeComponent();
        
        var messenger = WeakReferenceMessenger.Default;
		
        messenger.Register<ActiveContentChanged>(this, (_, message) =>
        {
            if (message.Value is not LocationSpawner locationSpawn)
                return;
			
            var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
            var segment = segmentRequest.Response;

            _presenter.Region = segment.GetRegion(locationSpawn.Region);
            _presenter.SetLocation(locationSpawn);
            
            _scriptsTabControl.SelectedItem = locationSpawn.Scripts.FirstOrDefault(s => s.IsEnabled);
        });
    }
}

public class LocationSpawnViewModel : ObservableRecipient
{
    public string Name => "(Spawn)";
}