using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge;

public class RegionVisibilityChanged(RegionVisibility Visibility, string PropertyName) : ValueChangedMessage<RegionVisibility>(Visibility)
{
    /// <summary>
    /// Gets the name of the visibility setting that changed.
    /// </summary>
    public string PropertyName { get; } = PropertyName;
}

public class RegionVisibility : ObservableRecipient
{
    private bool _breakWalls;
    private bool _openDoors;
    private bool _hideSecretDoors;
    private bool _showTeleporters;
    private bool _showSpawns;
    private bool _showComments;

    public bool BreakWalls
    {
        get => _breakWalls;
        set => SetProperty(ref _breakWalls, value);
    }
    
    public bool OpenDoors
    {
        get => _openDoors;
        set => SetProperty(ref _openDoors, value);
    }
    
    public bool HideSecretDoors
    {
        get => _hideSecretDoors;
        set => SetProperty(ref _hideSecretDoors, value);
    }
    
    public bool ShowTeleporters
    {
        get => _showTeleporters;
        set => SetProperty(ref _showTeleporters, value);
    }
    
    public bool ShowSpawns
    {
        get => _showSpawns;
        set => SetProperty(ref _showSpawns, value);
    }
    
    public bool ShowComments
    {
        get => _showComments;
        set => SetProperty(ref _showComments, value);
    }

    /// <summary>
    /// Gets a value indicating whether the setting changes component renders (walls and doors),
    /// as opposed to overlays drawn on top of the terrain.
    /// </summary>
    public static bool AffectsTerrain(string propertyName)
    {
        // a null or empty name means every property changed.
        return string.IsNullOrEmpty(propertyName)
            || propertyName is nameof(BreakWalls) or nameof(OpenDoors) or nameof(HideSecretDoors);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        // Notify that visibility settings have changed
        WeakReferenceMessenger.Default.Send(new RegionVisibilityChanged(this, e.PropertyName));
    }
}

