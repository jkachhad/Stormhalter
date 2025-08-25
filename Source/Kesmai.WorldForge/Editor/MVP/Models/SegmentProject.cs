using System.Collections.ObjectModel;
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

    public ObservableCollection<VirtualFile> VirtualFiles { get; } = new();
    public ObservableCollection<Region> Regions { get; } = new();
    public ObservableCollection<Spawn> Spawns { get; } = new();
    public ObservableCollection<Treasure> Treasures { get; } = new();
    public ObservableCollection<Hoard> Hoards { get; } = new();

    public SegmentProject()
    {
        VirtualFiles.Add(new VirtualFile { Name = "Internal", Text = string.Empty });
        VirtualFiles.Add(new VirtualFile { Name = "WorldForge", Text = string.Empty });

        Regions.Add(new Region { Name = "Region 1" });
        Regions.Add(new Region { Name = "Region 2" });
        Regions.Add(new Region { Name = "Region 3" });

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
