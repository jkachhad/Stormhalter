using CommonServiceLocator;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using DotNext.Buffers;
using ICSharpCode.AvalonEdit.Document;
using Kesmai.WorldForge.Editor;
using Syncfusion.Windows.PropertyGrid;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;

namespace Kesmai.WorldForge.UI.Documents;

public partial class SpawnsDocument : UserControl
{
    public class GetActiveEntity : RequestMessage<Entity>
    {
    }
    public class GetCurrentTypeSelection : RequestMessage<int>
    {
    }
    public SpawnsDocument()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<SpawnsDocument, UnregisterEvents>(this,
            (r, m) => { WeakReferenceMessenger.Default.UnregisterAll(this); });
    }
    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is not SpawnsViewModel viewModel)
            return;

        if (e.NewValue is LocationSpawner location)
            viewModel.SelectedLocationSpawner = location;
        else if (e.NewValue is RegionSpawner region)
            viewModel.SelectedRegionSpawner = region;
    }
    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        ScrollViewer scroller = sender as ScrollViewer;
        if (scroller != null)
        {
            scroller.ScrollToVerticalOffset(scroller.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
    private void LocationPropertyGrid_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        if ((e.Property.Name != "Name" && e.Property.Name != "Region") || Equals(e.OldValue, e.NewValue))
            return;
        if (!(DataContext is SpawnsViewModel vm))
            return;

        var pg = (PropertyGrid)sender;
        var spawner = pg.SelectedObject as LocationSpawner;
        if (spawner == null) return;
        var name = spawner.Name;
        var region = spawner.Region;
        string regionNewName = "", regionOldName = "";
        if (e.Property.Name == "Region")
        {
            regionNewName = vm.GetSegment()?.GetRegion((int)e.NewValue)?.Name;
            regionOldName = vm.GetSegment()?.GetRegion((int)e.OldValue)?.Name;
        }

        var expanded = vm.LocationGroups.Groups
                            .Where(g => g.IsExpanded)
                            .Select(g => g.Name)
                            .ToHashSet();

        vm.LocationGroups.Import(vm.Source.Location);

        foreach (var grp in vm.LocationGroups.Groups)
        {
            foreach (var spn in grp.Spawners)
            {
                grp.IsSelected = false;
                if (spn.Name == name)
                    spn.IsSelected = true;
                grp.Debug = $"{e.Property.Name} , {grp.Name} , {regionNewName} , {regionOldName} , {regionOldName}  ";
            }
            if (e.Property.Name == "Region")
            {
                if (grp.Name == regionNewName)
                    grp.IsExpanded = true;
                else if (grp.Name == regionOldName)
                    grp.IsExpanded = false;
                else
                    grp.IsExpanded = expanded.Contains(grp.Name);
            }
            else
                grp.IsExpanded = expanded.Contains(grp.Name);

        }
        CollectionViewSource.GetDefaultView(vm.LocationGroups.Groups)
                            .Refresh();
    }

    private void RegionPropertyGrid_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        if ((e.Property.Name != "Name" && e.Property.Name != "Region") || Equals(e.OldValue, e.NewValue))
            return;
        if (!(DataContext is SpawnsViewModel vm))
            return;

        var pg = (PropertyGrid)sender;
        var spawner = pg.SelectedObject as RegionSpawner;
        if (spawner == null) return;
        var name = spawner.Name;
        var region = spawner.Region;
        string regionNewName = "", regionOldName = "";
        if (e.Property.Name == "Region")
        {
            regionNewName = vm.GetSegment()?.GetRegion((int)e.NewValue)?.Name;
            regionOldName = vm.GetSegment()?.GetRegion((int)e.OldValue)?.Name;
        }


        var expanded = vm.RegionGroups.Groups
                         .Where(g => g.IsExpanded)
                         .Select(g => g.Name)
                         .ToHashSet();

        vm.RegionGroups.Import(vm.Source.Region);

        foreach (var grp in vm.RegionGroups.Groups)
        {
            foreach (var spn in grp.Spawners)
            {
                grp.IsSelected = false;
                if (spn.Name == name)
                    spn.IsSelected = true;
                grp.Debug = $" ";
            }
            if (e.Property.Name == "Region")
            {
                if (grp.Name == regionNewName)
                    grp.IsExpanded = true;
                else if (grp.Name == regionOldName)
                    grp.IsExpanded = false;
                else
                    grp.IsExpanded = expanded.Contains(grp.Name);
            }
            else
                grp.IsExpanded = expanded.Contains(grp.Name);

        }

        CollectionViewSource.GetDefaultView(vm.RegionGroups.Groups)
                            .Refresh();
    }
}

public class SpawnsViewModel : ObservableRecipient
{
    private int _newSpawnerCount = 1;

    public class SelectedLocationSpawnerChangedMessage : ValueChangedMessage<LocationSpawner>
    {
        public string Source { get; }
        public SelectedLocationSpawnerChangedMessage(LocationSpawner spawner, string source = null) : base(spawner)
        {
            Source = source;
        }
    }

    public class SelectedRegionSpawnerChangedMessage : ValueChangedMessage<RegionSpawner>
    {
        public string Source { get; }
        public SelectedRegionSpawnerChangedMessage(RegionSpawner spawner, string source = null) : base(spawner)
        {
            Source = source;
        }
    }
    public class SpawnerGroup : ObservableObject
    {
        public string Name { get; set; }
        public string SpawnerName { get; set; }
        public ObservableCollection<Spawner> Spawners { get; set; } = new();

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        private string _debug;
        public string Debug
        {
            get => _debug;
            set => SetProperty(ref _debug, value);
        }
    }
    public SpawnerGroups LocationGroups { get; private set; }
    public SpawnerGroups RegionGroups { get; private set; }
    public class SpawnerGroups : ObservableObject
    {
        private readonly Segment _segment;

        public SpawnerGroups(Segment segment)
        {
            _segment = segment;
        }

        public ObservableCollection<SpawnerGroup> Groups { get; set; } = new();

        public void Import(IEnumerable<LocationSpawner> spawners)
        {
            Groups.Clear();
            foreach (var spawner in spawners.OrderBy(s => s.Region))
            {
                string groupName = _segment.GetRegion(spawner.Region) is { Name: var name }
                    ? $"{spawner.Region}: {name}"
                    : $"Region {spawner.Region}";
                var group = Groups.FirstOrDefault(g => g.Name == groupName);
                if (group == null)
                {
                    group = new SpawnerGroup { Name = groupName };
                    Groups.Add(group);
                }
                group.Spawners.Add(spawner);
            }
        }

        public void Import(IEnumerable<RegionSpawner> spawners)
        {
            Groups.Clear();
            foreach (var spawner in spawners.OrderBy(s => s.Region))
            {
                string groupName = _segment.GetRegion(spawner.Region) is { Name: var name }
                    ? $"{spawner.Region}: {name}"
                    : $"Region {spawner.Region}";
                var group = Groups.FirstOrDefault(g => g.Name == groupName);
                if (group == null)
                {
                    group = new SpawnerGroup { Name = groupName };
                    Groups.Add(group);
                }
                group.Spawners.Add(spawner);
            }
        }
    }

    public string Name => "(Spawns)";

    private Segment _segment;
    private LocationSpawner _selectedLocationSpawner;
    private RegionSpawner _selectedRegionSpawner;

    public Segment GetSegment() => _segment;
    public LocationSpawner SelectedLocationSpawner
    {
        get => _selectedLocationSpawner;
        set
        {
            SetProperty(ref _selectedLocationSpawner, value, true);

            if (value != null)
                WeakReferenceMessenger.Default.Send(new SelectedLocationSpawnerChangedMessage(value, source: "TreeView"));
        }
    }

    public RegionSpawner SelectedRegionSpawner
    {
        get => _selectedRegionSpawner;
        set
        {
            SetProperty(ref _selectedRegionSpawner, value, true);

            if (value != null)
            {
                WeakReferenceMessenger.Default.Send(new SelectedRegionSpawnerChangedMessage(value, source: "TreeView"));
            }
        }
    }

    public SegmentSpawns Source => _segment.Spawns;
    public ObservableCollection<Entity> Entities
    {
        get
        {
            var sortedEntities = _segment.Entities.OrderBy(e => e.Name).ToList();
            return new ObservableCollection<Entity>(sortedEntities);
        }
    }
    
    public SpawnsViewModel(Segment segment)
    {
        _segment = segment;

        LocationGroups = new SpawnerGroups(_segment);
        RegionGroups = new SpawnerGroups(_segment);

        LocationGroups.Import(_segment.Spawns.Location);
        RefreshLocationGroups();
        RegionGroups.Import(_segment.Spawns.Region);
        RefreshRegionGroups();
    }
    public void RefreshLocationGroups()
    {
        var expandedGroupNames = LocationGroups.Groups
            .Where(g => g.IsExpanded)
            .Select(g => g.Name)
            .ToHashSet();

        CollectionViewSource.GetDefaultView(LocationGroups.Groups).Refresh();

        foreach (var group in LocationGroups.Groups)
        {
            group.IsExpanded = expandedGroupNames.Contains(group.Name);
        }
    }

    public void RefreshRegionGroups()
    {
        var expandedGroupNames = RegionGroups.Groups
            .Where(g => g.IsExpanded)
            .Select(g => g.Name)
            .ToHashSet();

        CollectionViewSource.GetDefaultView(RegionGroups.Groups).Refresh();

        foreach (var group in RegionGroups.Groups)
        {
            group.IsExpanded = expandedGroupNames.Contains(group.Name);
        }
    }

    public void JumpEntity()
    {
        var entityRequest = WeakReferenceMessenger.Default.Send<SpawnsDocument.GetActiveEntity>();
        var entity = entityRequest.Response;
        WeakReferenceMessenger.Default.Send(entity);
    }
}