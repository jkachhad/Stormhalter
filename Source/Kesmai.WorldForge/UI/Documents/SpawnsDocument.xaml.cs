using CommonServiceLocator;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using DigitalRune.ServiceLocation;
using DotNext.Collections.Generic;
using Kesmai.WorldForge.Editor;
using Microsoft.Xna.Framework;
using System;
using Syncfusion.Windows.PropertyGrid;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;
using static Kesmai.WorldForge.UI.Documents.SpawnsViewModel;

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
        _regionPresenter.Initialize();
        _locationPresenter.Initialize();

        WeakReferenceMessenger.Default
            .Register<SpawnsDocument, SpawnsViewModel.SelectedLocationSpawnerChangedMessage>(
                this, OnLocationSpawnerChanged);

        WeakReferenceMessenger.Default
            .Register<SpawnsDocument, SpawnsViewModel.SelectedRegionSpawnerChangedMessage>(
                this, OnRegionSpawnerChanged);

        WeakReferenceMessenger.Default
            .Register<SpawnsDocument, Spawner>(
                this, (r, m) => { _typeSelector.SelectedIndex = m is LocationSpawner ? 0 : 1; });

        WeakReferenceMessenger.Default.Register<SpawnsDocument, GetActiveEntity>(this,
            (r, m) => m.Reply(GetSelectedEntity()));

        WeakReferenceMessenger.Default.Register<SpawnsDocument, GetCurrentTypeSelection>(this,
            (r, m) => m.Reply(_typeSelector.SelectedIndex));

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
        if (DataContext is SpawnsViewModel vm)
        {
            vm.LocationGroups.Import(vm.Source.Location);
            vm.RefreshLocationGroups();   // rebuild & re-expand your Location tree
        }
    }

    private void RegionPropertyGrid_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (DataContext is SpawnsViewModel vm)
        {
            vm.RegionGroups.Import(vm.Source.Region);
            vm.RefreshRegionGroups();     // rebuild & re-expand your Region tree
        }
    }

    public Entity GetSelectedEntity()
    {
        SpawnEntry entry = null;
        var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
        if (presenter.ActiveDocument is not SpawnsViewModel)
            return null;
        if (_typeSelector.SelectedIndex == 0)
        {
            entry = _locationEntities.SelectedItem as SpawnEntry;
        }
        else
        {
            entry = _regionEntities.SelectedItem as SpawnEntry;
        }
        if (entry is null)
            return null as Entity;

        return entry.Entity;
    }
    private void OnLocationSpawnerChanged(SpawnsDocument recipient, SpawnsViewModel.SelectedLocationSpawnerChangedMessage message)
    {
        _scriptsTabControl.SelectedIndex = 0;

        var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
        var segment = segmentRequest.Response;

        var spawn = message.Value;
        if (spawn != null)
        {
            _locationPresenter.Region = segment.GetRegion(spawn.Region);
            _locationPresenter.SetLocation(spawn);
        }
        _locationSpawnerList.ScrollIntoView(_locationSpawnerList.SelectedItem);
        if (DataContext is not SpawnsViewModel viewModel)
            return;
        if (message?.Source == null)
            viewModel.RefreshLocationGroups();
    }

    private void OnRegionSpawnerChanged(SpawnsDocument recipient, SpawnsViewModel.SelectedRegionSpawnerChangedMessage message)
    {
        _scriptsTabControl.SelectedIndex = 0;
        var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
        var segment = segmentRequest.Response;

        var spawn = message.Value;
        if (spawn != null)
        {
            _regionPresenter.Region = segment.GetRegion(spawn.Region);
            _regionPresenter.SetLocation(spawn);
        }
        _regionSpawnerList.ScrollIntoView(_regionSpawnerList.SelectedItem);

        if (DataContext is not SpawnsViewModel viewModel)
            return;
        if (message?.Source == null)
            viewModel.RefreshRegionGroups();
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
        public ObservableCollection<Spawner> Spawners { get; set; } = new();

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
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
            foreach (var spawner in spawners.OrderBy(s => s.Name))
            {
                string groupName = _segment.GetRegion(spawner.Region)?.Name ?? $"Region {spawner.Region}";
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
            foreach (var spawner in spawners.OrderBy(s => s.Name))
            {
                string groupName = _segment.GetRegion(spawner.Region)?.Name ?? $"Region {spawner.Region}";
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
                value.CalculateStats();
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
    public RelayCommand AddLocationSpawnerCommand { get; set; }
    public RelayCommand<LocationSpawner> RemoveLocationSpawnerCommand { get; set; }
    public RelayCommand<LocationSpawner> CopyLocationSpawnerCommand { get; set; }
    public RelayCommand<LocationSpawner> CloneLocationSpawnerCommand { get; set; }

    public RelayCommand AddRegionSpawnerCommand { get; set; }
    public RelayCommand<RegionSpawner> RemoveRegionSpawnerCommand { get; set; }
    public RelayCommand<RegionSpawner> CopyRegionSpawnerCommand { get; set; }
    public RelayCommand<RegionSpawner> CloneRegionSpawnerCommand { get; set; }

    public RelayCommand PasteSpawnerCommand { get; set; }

    public RelayCommand JumpEntityCommand { get; set; }
    public SpawnsViewModel(Segment segment)
    {
        _segment = segment;

        LocationGroups = new SpawnerGroups(_segment);
        RegionGroups = new SpawnerGroups(_segment);

        LocationGroups.Import(_segment.Spawns.Location);
        RegionGroups.Import(_segment.Spawns.Region);

        AddLocationSpawnerCommand = new RelayCommand(AddLocationSpawner);
        RemoveLocationSpawnerCommand = new RelayCommand<LocationSpawner>(RemoveLocationSpawner,
            (spawner) => SelectedLocationSpawner != null);
        RemoveLocationSpawnerCommand.DependsOn(() => SelectedLocationSpawner);
        CopyLocationSpawnerCommand = new RelayCommand<LocationSpawner>(CopySpawner,
            (spawner) => SelectedLocationSpawner != null);
        CopyLocationSpawnerCommand.DependsOn(() => SelectedLocationSpawner);
        CloneLocationSpawnerCommand = new RelayCommand<LocationSpawner>(CloneSpawner,
            (spawner) => SelectedLocationSpawner != null);
        CloneLocationSpawnerCommand.DependsOn(() => SelectedLocationSpawner);

        AddRegionSpawnerCommand = new RelayCommand(AddRegionSpawner);
        RemoveRegionSpawnerCommand = new RelayCommand<RegionSpawner>(RemoveRegionSpawner,
            (spawner) => SelectedRegionSpawner != null);
        RemoveRegionSpawnerCommand.DependsOn(() => SelectedRegionSpawner);
        CopyRegionSpawnerCommand = new RelayCommand<RegionSpawner>(CopySpawner,
            (spawner) => SelectedRegionSpawner != null);
        CopyRegionSpawnerCommand.DependsOn(() => SelectedRegionSpawner);
        CloneRegionSpawnerCommand = new RelayCommand<RegionSpawner>(CloneSpawner,
            (spawner) => SelectedRegionSpawner != null);
        CloneRegionSpawnerCommand.DependsOn(() => SelectedRegionSpawner);

        PasteSpawnerCommand = new RelayCommand(PasteSpawner);

        JumpEntityCommand = new RelayCommand(JumpEntity);
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

    public void AddLocationSpawner()
    {
        var newSpawner = new LocationSpawner()
        {
            Name = $"Spawner {_newSpawnerCount++}"
        };

        Source.Location.Add(newSpawner);
        SelectedLocationSpawner = newSpawner;

        LocationGroups.Import(Source.Location); //  Refresh group view
    }

    public void RemoveLocationSpawner(LocationSpawner spawner)
    {
        var result = MessageBox.Show($"Are you sure you wish to delete '{spawner.Name}'?",
            "WorldForge", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.No)
        {
            Source.Location.Remove(spawner);
            LocationGroups.Import(Source.Location); //  Refresh group view
        }
    }


    public void AddRegionSpawner()
    {
        var newSpawner = new RegionSpawner()
        {
            Name = $"Spawner {_newSpawnerCount++}"
        };

        Source.Region.Add(newSpawner);
        SelectedRegionSpawner = newSpawner;

        RegionGroups.Import(Source.Region); //  Refresh group view
    }

    public void RemoveRegionSpawner(RegionSpawner spawner)
    {
        var result = MessageBox.Show($"Are you sure you wish to delete '{spawner.Name}'?",
            "WorldForge", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.No)
        {
            Source.Region.Remove(spawner);
            RegionGroups.Import(Source.Region); //  Refresh group view
        }
    }

    public void CopySpawner(Spawner spawner)
    {
        if (spawner is LocationSpawner l)
            Clipboard.SetText(l.GetXElement().ToString());
        else if (spawner is RegionSpawner r)
            Clipboard.SetText(r.GetXElement().ToString());
    }

    public void PasteSpawner()
    {
        XDocument clipboard = null;
        try
        {
            clipboard = XDocument.Parse(Clipboard.GetText());
        }
        catch { }

        if (clipboard is null || clipboard.Root.Name.ToString() != "spawn")
            return;

        Spawner spawner = null;

        if (clipboard.Root.Attribute("type").Value == "LocationSpawner")
        {
            spawner = new LocationSpawner(clipboard.Root);
            while (Source.Location.Any(s => s.Name == spawner.Name))
                spawner.Name = $"Copy of {spawner.Name}";

            Source.Location.Add((LocationSpawner)spawner);
            LocationGroups.Import(Source.Location); //  Refresh group view
        }
        else if (clipboard.Root.Attribute("type").Value == "RegionSpawner")
        {
            spawner = new RegionSpawner(clipboard.Root);
            while (Source.Region.Any(s => s.Name == spawner.Name))
                spawner.Name = $"Copy of {spawner.Name}";

            Source.Region.Add((RegionSpawner)spawner);
            RegionGroups.Import(Source.Region); //  Refresh group view
        }

        if (spawner != null)
        {
            foreach (var entryElement in clipboard.Root.Elements("entry"))
            {
                var entry = new SpawnEntry(entryElement);
                var entityName = (string)entryElement.Attribute("entity");

                entry.Entity = _segment.Entities.FirstOrDefault(e => e.Name == entityName);
                if (entry.Entity != null)
                    spawner.Entries.Add(entry);
            }
        }
    }
    public void CloneSpawner(Spawner spawner)
    {
        Spawner newSpawner = null;

        if (spawner is LocationSpawner l)
        {
            newSpawner = new LocationSpawner(l.GetXElement());
            while (Source.Location.Any(s => s.Name == newSpawner.Name))
                newSpawner.Name = $"Copy of {newSpawner.Name}";

            Source.Location.Add((LocationSpawner)newSpawner);
            LocationGroups.Import(Source.Location); //  Refresh group view
        }
        else if (spawner is RegionSpawner r)
        {
            newSpawner = new RegionSpawner(r.GetXElement());
            while (Source.Region.Any(s => s.Name == newSpawner.Name))
                newSpawner.Name = $"Copy of {newSpawner.Name}";

            Source.Region.Add((RegionSpawner)newSpawner);
            RegionGroups.Import(Source.Region); //  Refresh group view
        }

        foreach (var entry in spawner.Entries)
        {
            newSpawner.Entries.Add(entry);
        }
    }

}