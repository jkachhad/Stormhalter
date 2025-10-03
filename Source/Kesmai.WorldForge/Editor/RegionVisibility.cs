using CommunityToolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge;

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
}

