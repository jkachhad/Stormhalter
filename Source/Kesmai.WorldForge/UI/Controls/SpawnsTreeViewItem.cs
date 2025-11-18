using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.UI.Controls;

namespace Kesmai.WorldForge.UI;

internal sealed class SpawnsTreeViewItem : TreeViewItem, IDisposable
{
    private readonly Segment _segment;
    private readonly Dictionary<SegmentSpawner, SegmentTreeViewItem> _spawnItems = new();
    private readonly Dictionary<int, TreeViewItem> _regionNodes = new();
    
    private int _nextSpawnerId;

    public SpawnsTreeViewItem(Segment segment, object header)
    {
        _segment = segment ?? throw new ArgumentNullException(nameof(segment));
        
        Header = header ?? throw new ArgumentNullException(nameof(header));
        Tag = "category:Spawns";

        ContextMenu = new ContextMenu();
        ContextMenu.AddItem("Add Location Spawner", "Add.png", (s, e) => AddLocationSpawner());
        ContextMenu.AddItem("Add Region Spawner", "Add.png", (s, e) => AddRegionSpawner());

        _nextSpawnerId = 0;

        var messenger = WeakReferenceMessenger.Default;

        messenger.Register<SegmentSpawnAdded>(this, (_, message) =>
        {
            Bind(message.Value, CreateSpawnItem(message.Value));
        });

        messenger.Register<SegmentSpawnRemoved>(this, (_, message) =>
        {
            if (_spawnItems.Remove(message.Value, out var item) && item.Parent is ItemsControl parent)
                parent.Items.Remove(item);
        });

        messenger.Register<SegmentSpawnChanged>(this, (_, message) =>
        {
            if (!_spawnItems.TryGetValue(message.Value, out var item))
                return;

            item.EditableTextBlock.Text = message.Value.Name;
            
            Bind(message.Value, item);
        });
        
        foreach (var locationSpawner in _segment.Spawns.Location)
            Bind(locationSpawner, CreateSpawnItem(locationSpawner));

        foreach (var regionSpawner in _segment.Spawns.Region)
            Bind(regionSpawner, CreateSpawnItem(regionSpawner));
    }

    public void Dispose()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        
        Items.Clear();
        
        _spawnItems.Clear();
        _regionNodes.Clear();
    }

    private SegmentTreeViewItem CreateSpawnItem(SegmentSpawner spawner)
    {
        if (_spawnItems.TryGetValue(spawner, out var existing))
            return existing;

        var brush = spawner switch
        {
            LocationSegmentSpawner => Brushes.SkyBlue,
            RegionSegmentSpawner => Brushes.Orange,
            _ => Brushes.Gray
        };

        var spawnerItem = new SegmentTreeViewItem(spawner, brush, true)
        {
            Tag = spawner,
        };

        spawnerItem.ContextMenu = new ContextMenu();
        spawnerItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => spawnerItem.Rename());
        spawnerItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => spawner.Copy(_segment));
        spawnerItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => RemoveSpawner(spawner));

        _spawnItems.Add(spawner, spawnerItem);
        
        return spawnerItem;
    }

    private void Bind(SegmentSpawner spawner, SegmentTreeViewItem spawnerItem)
    {
        var regionId = spawner switch
        {
            LocationSegmentSpawner locationSpawner => locationSpawner.Region,
            RegionSegmentSpawner regionSpawner => regionSpawner.Region,
            _ => -1
        };

        var parentNode = regionId >= 0 ? EnsureRegionNode(regionId) : this;

        if (spawnerItem.Parent is ItemsControl currentParent && !ReferenceEquals(currentParent, parentNode))
            currentParent.Items.Remove(spawnerItem);

        if (!parentNode.Items.Contains(spawnerItem))
            parentNode.Items.Add(spawnerItem);
    }

    private TreeViewItem EnsureRegionNode(int regionId)
    {
        if (_regionNodes.TryGetValue(regionId, out var node))
            return node;

        var region = _segment.GetRegion(regionId);
        var headerText = region is null ? $"Region {regionId}" : $"[{region.ID}] {region.Name}";

        node = new TreeViewItem
        {
            Header = CreateHeader(headerText, "Folder.png")
        };

        node.ContextMenu = new ContextMenu();
        node.ContextMenu.AddItem("Add Location Spawner", "Add.png", (s, e) => AddLocationSpawner(regionId));
        node.ContextMenu.AddItem("Add Region Spawner", "Add.png", (s, e) => AddRegionSpawner(regionId));

        Items.Add(node);
        _regionNodes.Add(regionId, node);
        return node;
    }

    private void AddLocationSpawner(int? regionId = null)
    {
        var spawn = new LocationSegmentSpawner
        {
            Name = $"Location Spawner {_nextSpawnerId++}"
        };

        if (regionId.HasValue)
            spawn.Region = regionId.Value;

        _segment.Spawns.Location.Add(spawn);
        spawn.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
    }

    private void AddRegionSpawner(int? regionId = null)
    {
        var spawn = new RegionSegmentSpawner
        {
            Name = $"Region Spawner {_nextSpawnerId++}"
        };

        if (regionId.HasValue)
            spawn.Region = regionId.Value;

        _segment.Spawns.Region.Add(spawn);
        spawn.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
    }

    private void RemoveSpawner(SegmentSpawner spawner)
    {
        switch (spawner)
        {
            case LocationSegmentSpawner locationSpawner:
                _segment.Spawns.Location.Remove(locationSpawner);
                break;
            case RegionSegmentSpawner regionSpawner:
                _segment.Spawns.Region.Remove(regionSpawner);
                break;
        }
    }

    private static object CreateHeader(string name, string icon)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var image = new Image
        {
            Width = 24,
            Height = 24,
            Margin = new Thickness(2, 0, 2, 0),
        };

        if (!string.IsNullOrEmpty(icon))
        {
            image.Source = new BitmapImage(new Uri($"pack://application:,,,/Kesmai.WorldForge;component/Resources/{icon}"));
        }

        panel.Children.Add(image);
        panel.Children.Add(new TextBlock { Text = name, FontSize = 12, VerticalAlignment = VerticalAlignment.Center });

        return panel;
    }
}
