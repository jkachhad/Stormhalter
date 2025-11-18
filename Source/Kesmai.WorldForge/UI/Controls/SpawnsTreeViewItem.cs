using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.UI.Controls;

namespace Kesmai.WorldForge.UI;

internal sealed class SpawnsTreeViewItem : TreeViewItem, IDisposable
{
    private const string DragFormat = "Kesmai.WorldForge.SpawnsTreeViewItem.Spawn";

    private readonly Segment _segment;
    private readonly Dictionary<SegmentSpawner, SegmentTreeViewItem> _spawnItems = new();
    private readonly Dictionary<int, TreeViewItem> _regionNodes = new();
    private readonly Dictionary<TreeViewItem, int> _regionLookup = new();
    
    private int _nextSpawnerId;
    
    private Point? _dragStartPoint;
    private SegmentTreeViewItem _dragSourceItem;
    private TreeViewItem _currentDropTarget;
    private DropHighlightAdorner _currentDropAdorner;

    public SpawnsTreeViewItem(Segment segment, object header)
    {
        _segment = segment ?? throw new ArgumentNullException(nameof(segment));
        
        Header = header ?? throw new ArgumentNullException(nameof(header));
        Tag = "category:Spawns";

        ContextMenu = new ContextMenu();
        ContextMenu.AddItem("Add Location Spawner", "Add.png", (s, e) => AddLocationSpawner());
        ContextMenu.AddItem("Add Region Spawner", "Add.png", (s, e) => AddRegionSpawner());
        
        AttachDropTarget(this);

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
        SetDropTarget(null);
        
        _spawnItems.Clear();
        _regionNodes.Clear();
        _regionLookup.Clear();
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

        AttachDropTarget(spawnerItem);
        spawnerItem.PreviewMouseLeftButtonDown += OnSpawnPreviewMouseLeftButtonDown;
        spawnerItem.PreviewMouseMove += OnSpawnPreviewMouseMove;
        spawnerItem.PreviewMouseLeftButtonUp += OnSpawnPreviewMouseLeftButtonUp;

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

        AttachDropTarget(node);
        _regionLookup[node] = regionId;

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

    private void AttachDropTarget(TreeViewItem treeViewItem)
    {
        treeViewItem.AllowDrop = true;

        treeViewItem.PreviewDragOver += OnPreviewDrag;
        treeViewItem.DragLeave += OnDragLeave;
        treeViewItem.Drop += OnDrop;
    }

    private void OnPreviewDrag(object sender, DragEventArgs args)
    {
        if (args.Source is not UIElement dragControl)
            return;

        args.Handled = true;

        if (!TryGetDraggedSpawner(args.Data, out _))
        {
            args.Effects = DragDropEffects.None;
            SetDropTarget(null);
            return;
        }

        var dragAncestor = dragControl.FindAncestor<TreeViewItem>();

        if (dragAncestor is null || ReferenceEquals(dragAncestor, _dragSourceItem))
        {
            args.Effects = DragDropEffects.None;
            SetDropTarget(null);
            return;
        }

        var targetRegion = GetDropRegionFor(dragAncestor);

        if (targetRegion is null)
        {
            args.Effects = DragDropEffects.None;
            SetDropTarget(null);
            return;
        }

        args.Effects = DragDropEffects.Move;
        SetDropTarget(dragAncestor);
    }

    private void OnDragLeave(object sender, DragEventArgs args)
    {
        args.Handled = true;

        if (!TryGetDraggedSpawner(args.Data, out _))
            return;

        if (sender is TreeViewItem treeViewItem && ReferenceEquals(treeViewItem, _currentDropTarget))
            SetDropTarget(null);
    }

    private void OnDrop(object sender, DragEventArgs args)
    {
        args.Handled = true;
        SetDropTarget(null);

        if (!TryGetDraggedSpawner(args.Data, out var spawner))
            return;

        if (sender is not TreeViewItem treeViewItem)
            return;

        var targetRegion = GetDropRegionFor(treeViewItem);

        if (targetRegion is null)
            return;

        var currentRegion = GetRegionValue(spawner);

        if (currentRegion.HasValue && currentRegion.Value == targetRegion.Value)
            return;

        switch (spawner)
        {
            case LocationSegmentSpawner locationSpawner:
                locationSpawner.Region = targetRegion.Value;
                break;
            case RegionSegmentSpawner regionSpawner:
                regionSpawner.Region = targetRegion.Value;
                break;
        }
    }

    private static bool TryGetDraggedSpawner(IDataObject data, out SegmentSpawner spawner)
    {
        if (data.GetDataPresent(DragFormat) && data.GetData(DragFormat) is SegmentSpawner segmentSpawner)
        {
            spawner = segmentSpawner;
            return true;
        }

        spawner = null!;
        return false;
    }

    private void SetDropTarget(TreeViewItem? treeViewItem)
    {
        if (ReferenceEquals(_currentDropTarget, treeViewItem))
            return;

        RemoveCurrentDropAdorner();

        _currentDropTarget = treeViewItem;

        if (_currentDropTarget is null)
            return;

        _currentDropTarget.ApplyTemplate();

        var layer = AdornerLayer.GetAdornerLayer(_currentDropTarget);

        if (layer is null)
        {
            _currentDropTarget = null;
            return;
        }

        _currentDropAdorner = new DropHighlightAdorner(_currentDropTarget);
        layer.Add(_currentDropAdorner);
    }

    private void RemoveCurrentDropAdorner()
    {
        if (_currentDropAdorner is null)
            return;

        var layer = AdornerLayer.GetAdornerLayer(_currentDropAdorner.AdornedElement);
        layer?.Remove(_currentDropAdorner);
        _currentDropAdorner = null;
    }

    private int? GetDropRegionFor(TreeViewItem treeViewItem)
    {
        if (ReferenceEquals(treeViewItem, this))
            return null;

        if (_regionLookup.TryGetValue(treeViewItem, out var regionId))
            return regionId;

        if (treeViewItem is SegmentTreeViewItem segmentItem && segmentItem.Tag is SegmentSpawner spawner)
            return GetRegionValue(spawner);

        return null;
    }

    private static int? GetRegionValue(SegmentSpawner spawner)
    {
        return spawner switch
        {
            LocationSegmentSpawner locationSpawner => locationSpawner.Region,
            RegionSegmentSpawner regionSpawner => regionSpawner.Region,
            _ => null
        };
    }

    private void OnSpawnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
    {
        _dragStartPoint = args.GetPosition(this);
        _dragSourceItem = sender as SegmentTreeViewItem;
    }

    private void OnSpawnPreviewMouseMove(object sender, MouseEventArgs args)
    {
        if (_dragStartPoint is null)
            return;

        if (args.LeftButton != MouseButtonState.Pressed)
        {
            ResetDrag();
            return;
        }

        if (!ReferenceEquals(sender, _dragSourceItem))
            return;

        var currentPosition = args.GetPosition(this);

        if (Math.Abs(currentPosition.X - _dragStartPoint.Value.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(currentPosition.Y - _dragStartPoint.Value.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        if (_dragSourceItem?.Tag is SegmentSpawner spawner)
            DragDrop.DoDragDrop(_dragSourceItem, new DataObject(DragFormat, spawner), DragDropEffects.Move);

        ResetDrag();
    }

    private void OnSpawnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs args)
    {
        ResetDrag();
    }

    private void ResetDrag()
    {
        _dragStartPoint = null;
        _dragSourceItem = null;
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
