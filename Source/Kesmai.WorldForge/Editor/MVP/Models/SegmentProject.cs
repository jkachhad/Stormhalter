using DigitalRune.Collections;
using System.ComponentModel;
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

    public NotifyingCollection<VirtualFile> VirtualFiles { get; } = new();
    public NotifyingCollection<SegmentRegion> Regions { get; } = new();
    public NotifyingCollection<Spawn> Spawns { get; } = new();
    public NotifyingCollection<Treasure> Treasures { get; } = new();
    public NotifyingCollection<Hoard> Hoards { get; } = new();

    public SegmentProject()
    {
        VirtualFiles.Add(new VirtualFile { Name = "Internal", Text = string.Empty });
        VirtualFiles.Add(new VirtualFile { Name = "WorldForge", Text = string.Empty });

        Regions.Add(new SegmentRegion(1));
        Regions.Add(new SegmentRegion(2));
        Regions.Add(new SegmentRegion(3));

        Spawns.Add(new Spawn { Name = "Spawn 1" });
        Spawns.Add(new Spawn { Name = "Spawn 2" });
        Spawns.Add(new Spawn { Name = "Spawn 3" });

        Treasures.Add(new Treasure { Name = "Treasure 1" });
        Treasures.Add(new Treasure { Name = "Treasure 2" });
        Treasures.Add(new Treasure { Name = "Treasure 3" });

        Hoards.Add(new Hoard { Name = "Hoard 1" });
        Hoards.Add(new Hoard { Name = "Hoard 2" });
        Hoards.Add(new Hoard { Name = "Hoard 3" });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
