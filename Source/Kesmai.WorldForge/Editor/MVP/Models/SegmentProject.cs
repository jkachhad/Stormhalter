using DigitalRune.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using ProjectModel = Kesmai.WorldForge.Editor.Project;

namespace Kesmai.WorldForge.Editor;

public class SegmentProject : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _rootPath = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public string RootPath
    {
        get => _rootPath;
        set
        {
            if (_rootPath != value)
            {
                _rootPath = value;
                OnPropertyChanged(nameof(RootPath));
            }
        }
    }

    private static readonly List<string> _reservedLocations = new()
    {
        "Entrance", "Resurrect", "Facet", "Thief",
    };

    public static IReadOnlyList<string> ReservedLocations => _reservedLocations;

    public NotifyingCollection<VirtualFile> VirtualFiles { get; } = new();
    public NotifyingCollection<SegmentRegion> Regions { get; } = new();
    public SegmentLocations Locations { get; set; } = new();
    public SegmentSubregions Subregions { get; set; } = new();
    public SegmentEntities Entities { get; set; } = new();
    public NotifyingCollection<ProjectModel.SegmentSpawn> Spawns { get; } = new();
    public NotifyingCollection<ProjectModel.SegmentTreasure> Treasures { get; } = new();
    public NotifyingCollection<ProjectModel.SegmentHoard> Hoards { get; } = new();

    public SegmentProject()
    {
        VirtualFiles.Add(new VirtualFile { Name = "Internal", Text = string.Empty });
        VirtualFiles.Add(new VirtualFile { Name = "WorldForge", Text = string.Empty });

        Regions.Add(new SegmentRegion(1));
        Regions.Add(new SegmentRegion(2));
        Regions.Add(new SegmentRegion(3));

        foreach (var location in _reservedLocations)
            Locations.Add(new SegmentLocation { Name = location });

        Spawns.Add(new ProjectModel.SegmentSpawn { Name = "Spawn 1" });
        Spawns.Add(new ProjectModel.SegmentSpawn { Name = "Spawn 2" });
        Spawns.Add(new ProjectModel.SegmentSpawn { Name = "Spawn 3" });

        Treasures.Add(new ProjectModel.SegmentTreasure { Name = "Treasure 1" });
        Treasures.Add(new ProjectModel.SegmentTreasure { Name = "Treasure 2" });
        Treasures.Add(new ProjectModel.SegmentTreasure { Name = "Treasure 3" });

        Hoards.Add(new ProjectModel.SegmentHoard { Name = "Hoard 1" });
        Hoards.Add(new ProjectModel.SegmentHoard { Name = "Hoard 2" });
        Hoards.Add(new ProjectModel.SegmentHoard { Name = "Hoard 3" });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
