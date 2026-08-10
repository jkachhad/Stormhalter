using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Diagnostics;
using Kesmai.WorldForge.UI.Controls;

namespace Kesmai.WorldForge.UI;

internal sealed class SpawnsTreeViewItem : TreeViewItem, IDisposable
{
    private const string DragFormat = "Kesmai.WorldForge.SpawnsTreeViewItem.Spawn";
    private const string RegionDragFormat = "Kesmai.WorldForge.SpawnsTreeViewItem.Region";

    private readonly Segment _segment;
    private readonly Dictionary<SegmentSpawner, SegmentTreeViewItem> _spawnItems = new();
    private readonly Dictionary<int, TreeViewItem> _regionNodes = new();
    private readonly Dictionary<TreeViewItem, int> _regionLookup = new();
    
    private int _nextSpawnerId;
    
    private Point? _dragStartPoint;
    private SegmentTreeViewItem _dragSourceItem;
    private TreeViewItem _dragSourceRegionItem;
    private TreeViewItem _currentDropTarget;
    private DropHighlightAdorner _currentDropAdorner;
    private bool _orderApplyPending;

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
            ScheduleXmlRegionOrder();
        });

        messenger.Register<SegmentSpawnRemoved>(this, (_, message) =>
        {
            if (_spawnItems.Remove(message.Value, out var item) && item.Parent is ItemsControl parent)
            {
                parent.Items.Remove(item);

                if (parent is TreeViewItem treeViewItem)
                    PruneEmptyRegionNode(treeViewItem);
            }
        });

        messenger.Register<SegmentSpawnsReset>(this, (_, _) => ResetSpawnNodes());

        messenger.Register<SegmentSpawnChanged>(this, (_, message) =>
        {
            if (!_spawnItems.TryGetValue(message.Value, out var item))
                return;

            item.EditableTextBlock.Text = message.Value.Name;
            
            Bind(message.Value, item);
        });
        
        using (PerformanceTrace.Measure(
                   "Build spawn tree", () => $"{_spawnItems.Count} spawns, {_regionNodes.Count} regions"))
        {
            foreach (var locationSpawner in _segment.Spawns.Location)
                Bind(locationSpawner, CreateSpawnItem(locationSpawner));

            foreach (var regionSpawner in _segment.Spawns.Region)
                Bind(regionSpawner, CreateSpawnItem(regionSpawner));

            ApplyXmlRegionOrder();
        }
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

    private void ResetSpawnNodes()
    {
        SetDropTarget(null);
        Items.Clear();
        _spawnItems.Clear();
        _regionNodes.Clear();
        _regionLookup.Clear();
    }

    private void ScheduleXmlRegionOrder()
    {
        if (_orderApplyPending)
            return;

        _orderApplyPending = true;
        Dispatcher.BeginInvoke(new Action(() =>
        {
            _orderApplyPending = false;
            using (PerformanceTrace.Measure(
                       "Apply spawn XML order",
                       () => $"{_spawnItems.Count} spawns, {_regionNodes.Count} regions"))
                ApplyXmlRegionOrder();
        }), DispatcherPriority.Background);
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
        {
            currentParent.Items.Remove(spawnerItem);

            if (currentParent is TreeViewItem treeViewItem)
                PruneEmptyRegionNode(treeViewItem);
        }

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
        node.PreviewMouseLeftButtonDown += OnRegionPreviewMouseLeftButtonDown;
        node.PreviewMouseMove += OnRegionPreviewMouseMove;
        node.PreviewMouseLeftButtonUp += OnRegionPreviewMouseLeftButtonUp;
        _regionLookup[node] = regionId;

        Items.Add(node);
        _regionNodes.Add(regionId, node);
        return node;
    }

    private void PruneEmptyRegionNode(TreeViewItem? regionNode)
    {
        if (regionNode is null)
            return;

        if (regionNode.Items.Count > 0)
            return;

        if (!_regionLookup.TryGetValue(regionNode, out var regionId))
            return;

        if (regionNode.Parent is ItemsControl parent)
            parent.Items.Remove(regionNode);

        _regionLookup.Remove(regionNode);
        _regionNodes.Remove(regionId);
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

        if (TryGetDraggedRegion(args.Data, out var draggedRegion))
        {
            var regionTarget = dragControl.FindAncestor<TreeViewItem>();

            if (!CanReorderRegion(draggedRegion, regionTarget))
            {
                args.Effects = DragDropEffects.None;
                SetDropTarget(null);
                return;
            }

            args.Effects = DragDropEffects.Move;
            SetDropTarget(regionTarget);
            return;
        }

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

        if (!TryGetDraggedSpawner(args.Data, out _) && !TryGetDraggedRegion(args.Data, out _))
            return;

        if (sender is TreeViewItem treeViewItem && ReferenceEquals(treeViewItem, _currentDropTarget))
            SetDropTarget(null);
    }

    private void OnDrop(object sender, DragEventArgs args)
    {
        args.Handled = true;
        SetDropTarget(null);

        if (TryGetDraggedRegion(args.Data, out var draggedRegion))
        {
            if (sender is TreeViewItem regionFolderTarget && CanReorderRegion(draggedRegion, regionFolderTarget))
                ReorderRegion(draggedRegion, regionFolderTarget,
                    args.GetPosition(regionFolderTarget).Y >= regionFolderTarget.ActualHeight / 2);

            return;
        }

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

    private static bool TryGetDraggedRegion(IDataObject data, out TreeViewItem region)
    {
        if (data.GetDataPresent(RegionDragFormat) && data.GetData(RegionDragFormat) is TreeViewItem treeViewItem)
        {
            region = treeViewItem;
            return true;
        }

        region = null!;
        return false;
    }

    private bool CanReorderRegion(TreeViewItem draggedRegion, TreeViewItem targetRegion)
    {
        return draggedRegion is not null &&
               targetRegion is not null &&
               !ReferenceEquals(draggedRegion, targetRegion) &&
               _regionLookup.ContainsKey(draggedRegion) &&
               _regionLookup.ContainsKey(targetRegion) &&
               ReferenceEquals(draggedRegion.Parent, targetRegion.Parent);
    }

    private void ReorderRegion(TreeViewItem draggedRegion, TreeViewItem targetRegion, bool placeAfter)
    {
        if (draggedRegion.Parent is not ItemsControl parent)
            return;

        var oldIndex = parent.Items.IndexOf(draggedRegion);

        if (oldIndex < 0 || parent.Items.IndexOf(targetRegion) < 0)
            return;

        parent.Items.RemoveAt(oldIndex);
        var targetIndex = parent.Items.IndexOf(targetRegion);
        parent.Items.Insert(targetIndex + (placeAfter ? 1 : 0), draggedRegion);

        var regionOrder = parent.Items.OfType<TreeViewItem>()
            .Where(item => _regionLookup.ContainsKey(item))
            .Select(item => _regionLookup[item])
            .ToList();

        for (var sortId = 0; sortId < regionOrder.Count; sortId++)
        {
            var regionId = regionOrder[sortId];

            foreach (var spawn in _segment.Spawns.GetSpawns().Where(spawn => GetRegionValue(spawn) == regionId))
                spawn.SortId = sortId;
        }
    }

    private void ApplyXmlRegionOrder()
    {
        var folders = Items.OfType<TreeViewItem>()
            .Where(item => _regionLookup.ContainsKey(item))
            .OrderBy(item => _segment.Spawns.GetSpawns()
                .Where(spawn => GetRegionValue(spawn) == _regionLookup[item] && spawn.SortId.HasValue)
                .Select(spawn => spawn.SortId.Value)
                .DefaultIfEmpty(Int32.MaxValue)
                .Min())
            .ToList();

        foreach (var folder in folders)
        {
            Items.Remove(folder);
            Items.Add(folder);
        }
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

    private void OnRegionPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
    {
        if (args.OriginalSource is not DependencyObject source ||
            source.FindAncestor<SegmentTreeViewItem>() is not null)
            return;

        _dragStartPoint = args.GetPosition(this);
        _dragSourceRegionItem = sender as TreeViewItem;
    }

    private void OnRegionPreviewMouseMove(object sender, MouseEventArgs args)
    {
        if (_dragStartPoint is null || !ReferenceEquals(sender, _dragSourceRegionItem))
            return;

        if (args.LeftButton != MouseButtonState.Pressed)
        {
            ResetDrag();
            return;
        }

        var currentPosition = args.GetPosition(this);

        if (Math.Abs(currentPosition.X - _dragStartPoint.Value.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(currentPosition.Y - _dragStartPoint.Value.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        DragDrop.DoDragDrop(_dragSourceRegionItem,
            new DataObject(RegionDragFormat, _dragSourceRegionItem), DragDropEffects.Move);

        ResetDrag();
    }

    private void OnRegionPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs args)
    {
        ResetDrag();
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
        _dragSourceRegionItem = null;
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
