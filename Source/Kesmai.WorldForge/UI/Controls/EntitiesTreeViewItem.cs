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
using Kesmai.WorldForge.UI.Documents;

namespace Kesmai.WorldForge.UI;

internal sealed class EntitiesTreeViewItem : TreeViewItem, IDisposable
{
    private const string UndefinedGroup = "Ungrouped";
    private const string DragFormat = "Kesmai.WorldForge.EntitiesTreeViewItem.Entity";
    private const string GroupDragFormat = "Kesmai.WorldForge.EntitiesTreeViewItem.Group";

    private readonly Segment _segment;
    private readonly Dictionary<SegmentEntity, SegmentTreeViewItem> _entityItems = new();
    private readonly Dictionary<string, TreeViewItem> _groupNodes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<TreeViewItem, string> _groupLookup = new();
    
    private int _nextEntityId;
    
    private Point? _dragStartPoint;
    private SegmentTreeViewItem _dragSourceItem;
    private TreeViewItem _dragSourceGroupItem;
    private TreeViewItem _currentDropTarget;
    private DropHighlightAdorner _currentDropAdorner;
    private bool _orderApplyPending;

    public EntitiesTreeViewItem(Segment segment, object header)
    {
        _segment = segment ?? throw new ArgumentNullException(nameof(segment));
        
        Header = header ?? throw new ArgumentNullException(nameof(header));
        Tag = "category:Entities";

        ContextMenu = new ContextMenu();
        ContextMenu.AddItem("Add Entity", "Add.png", (s, e) => AddEntity());
        
        AttachDropTarget(this);

        _nextEntityId = 0;

        var messenger = WeakReferenceMessenger.Default;

        messenger.Register<SegmentEntityAdded>(this, (_, message) =>
        {
            Bind(message.Value, CreateEntityItem(message.Value));
            ScheduleXmlGroupOrder();
        });

        messenger.Register<SegmentEntityRemoved>(this, (_, message) =>
        {
            if (_entityItems.Remove(message.Value, out var entityNode) && entityNode.Parent is ItemsControl parent)
            {
                parent.Items.Remove(entityNode);

                if (parent is TreeViewItem treeViewItem)
                    PruneEmptyGroups(treeViewItem);
            }
        });

        messenger.Register<SegmentEntitiesReset>(this, (_, _) => ResetEntityNodes());

        messenger.Register<SegmentEntityChanged>(this, (_, message) =>
        {
            if (!_entityItems.TryGetValue(message.Value, out var entityItem))
                return;

            entityItem.EditableTextBlock.Text = message.Value.Name;
            
            Bind(message.Value, entityItem);
        });

        using (PerformanceTrace.Measure(
                   "Build entity tree", () => $"{_entityItems.Count} entities, {_groupNodes.Count} groups"))
        {
            foreach (var entity in _segment.Entities)
                Bind(entity, CreateEntityItem(entity));

            ApplyXmlGroupOrder(this);
        }
    }

    public void Dispose()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        
        Items.Clear();
        SetDropTarget(null);
        
        _entityItems.Clear();
        _groupNodes.Clear();
        _groupLookup.Clear();
    }

    private void ResetEntityNodes()
    {
        SetDropTarget(null);
        Items.Clear();
        _entityItems.Clear();
        _groupNodes.Clear();
        _groupLookup.Clear();
    }

    private void ScheduleXmlGroupOrder()
    {
        if (_orderApplyPending)
            return;

        _orderApplyPending = true;
        Dispatcher.BeginInvoke(new Action(() =>
        {
            _orderApplyPending = false;
            using (PerformanceTrace.Measure(
                       "Apply entity XML order",
                       () => $"{_entityItems.Count} entities, {_groupNodes.Count} groups"))
                ApplyXmlGroupOrder(this);
        }), DispatcherPriority.Background);
    }

    private SegmentTreeViewItem CreateEntityItem(SegmentEntity entity)
    {
        if (_entityItems.TryGetValue(entity, out var existing))
            return existing;

        var entityItem = new SegmentTreeViewItem(entity, Brushes.Yellow, true)
        {
            Tag = entity,
        };

        entityItem.ContextMenu = new ContextMenu();
        entityItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => entityItem.Rename());
        entityItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => entity.Copy(_segment));
        entityItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) =>
        {
            _segment.Entities.Remove(entity);

            if (entityItem.Parent is ItemsControl parent)
                parent.Items.Remove(entityItem);
        });
        
        AttachDropTarget(entityItem);
        entityItem.PreviewMouseLeftButtonDown += OnEntityPreviewMouseLeftButtonDown;
        entityItem.PreviewMouseMove += OnEntityPreviewMouseMove;
        entityItem.PreviewMouseLeftButtonUp += OnEntityPreviewMouseLeftButtonUp;

        _entityItems.Add(entity, entityItem);
        
        return entityItem;
    }

    private void Bind(SegmentEntity entity, SegmentTreeViewItem entityItem)
    {
        var groupPath = string.IsNullOrEmpty(entity.Group) ? UndefinedGroup : entity.Group;

        var parentNode = EnsurePath(groupPath);

        if (entityItem.Parent is ItemsControl currentParent && !ReferenceEquals(currentParent, parentNode))
        {
            currentParent.Items.Remove(entityItem);

            if (currentParent is TreeViewItem treeViewItem)
                PruneEmptyGroups(treeViewItem);
        }

        if (!parentNode.Items.Contains(entityItem))
            parentNode.Items.Add(entityItem);
    }

    private TreeViewItem EnsurePath(string groupPath)
    {
        var parentNode = (TreeViewItem)this;
        var segments = groupPath.Split('\\');

        for (var i = 0; i < segments.Length; i++)
        {
            var path = string.Join("\\", segments.Take(i + 1));

            if (!_groupNodes.TryGetValue(path, out var folderNode))
            {
                folderNode = new TreeViewItem
                {
                    Header = CreateHeader(segments[i], "Folder.png")
                };

                folderNode.ContextMenu = new ContextMenu();
                folderNode.ContextMenu.AddItem("Rename Group", "Rename.png", (s, e) => RenameGroup(path));
                folderNode.ContextMenu.AddItem("Add Entity", "Add.png", (s, e) => AddEntity(path));
                folderNode.ContextMenu.AddItem("Add Subgroup", "Folder.png", (s, e) => AddSubgroup(path));

                parentNode.Items.Add(folderNode);
                _groupNodes.Add(path, folderNode);
                _groupLookup[folderNode] = path;
                AttachDropTarget(folderNode);
                folderNode.PreviewMouseLeftButtonDown += OnGroupPreviewMouseLeftButtonDown;
                folderNode.PreviewMouseMove += OnGroupPreviewMouseMove;
                folderNode.PreviewMouseLeftButtonUp += OnGroupPreviewMouseLeftButtonUp;
            }

            parentNode = folderNode;
        }

        return parentNode;
    }

    private void RenameGroup(string groupPath)
    {
        var separatorIndex = groupPath.LastIndexOf('\\');
        var parentPath = separatorIndex >= 0 ? groupPath.Substring(0, separatorIndex) : String.Empty;
        var currentName = separatorIndex >= 0 ? groupPath.Substring(separatorIndex + 1) : groupPath;

        var dialog = new InputDialog("Rename Entity Group", currentName)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() != true)
            return;

        var newName = dialog.Input?.Trim();

        if (String.IsNullOrWhiteSpace(newName) || newName.Contains('\\') || newName.Contains('/'))
        {
            MessageBox.Show("Group names cannot be empty or contain slash characters.",
                "Invalid Group Name", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var newGroupPath = String.IsNullOrEmpty(parentPath) ? newName : $"{parentPath}\\{newName}";

        if (String.Equals(groupPath, newGroupPath, StringComparison.OrdinalIgnoreCase))
            return;

        var affectedEntities = _segment.Entities.Where(entity =>
        {
            var entityPath = String.IsNullOrEmpty(entity.Group) ? UndefinedGroup : entity.Group;
            return String.Equals(entityPath, groupPath, StringComparison.OrdinalIgnoreCase) ||
                   entityPath.StartsWith(groupPath + "\\", StringComparison.OrdinalIgnoreCase);
        }).ToList();

        foreach (var entity in affectedEntities)
        {
            var entityPath = String.IsNullOrEmpty(entity.Group) ? UndefinedGroup : entity.Group;
            var suffix = entityPath.Substring(groupPath.Length);
            entity.Group = newGroupPath + suffix;
        }

        ScheduleXmlGroupOrder();
    }

    private void AddSubgroup(string parentGroupPath)
    {
        var dialog = new InputDialog("Add Entity Subgroup", "New Group")
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() != true)
            return;

        var subgroupName = dialog.Input?.Trim();

        if (String.IsNullOrWhiteSpace(subgroupName) || subgroupName.Contains('\\') || subgroupName.Contains('/'))
        {
            MessageBox.Show("Group names cannot be empty or contain slash characters.",
                "Invalid Group Name", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var subgroupPath = $"{parentGroupPath}\\{subgroupName}";

        if (_groupNodes.ContainsKey(subgroupPath))
        {
            MessageBox.Show("That subgroup already exists.", "Duplicate Group",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        AddEntity(subgroupPath);
    }

    private void PruneEmptyGroups(TreeViewItem? groupNode)
    {
        while (groupNode is not null && !ReferenceEquals(groupNode, this))
        {
            if (groupNode.Items.Count > 0)
                break;

            if (!_groupLookup.TryGetValue(groupNode, out var groupPath))
                break;

            if (groupNode.Parent is not ItemsControl parent)
                break;

            parent.Items.Remove(groupNode);
            
            _groupLookup.Remove(groupNode);
            _groupNodes.Remove(groupPath);

            groupNode = parent as TreeViewItem;
        }
    }

    private void AddEntity(string groupPath = null)
    {
        var entity = new SegmentEntity
        {
            Name = $"Entity {_nextEntityId++}",
            Group = groupPath
        };

        _segment.Entities.Add(entity);
        
        entity.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
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

    private void AttachDropTarget(TreeViewItem treeViewItem)
    {
        treeViewItem.AllowDrop = true;
        
        treeViewItem.PreviewDragOver += OnPreviewDrag;
        treeViewItem.DragLeave += OnDragLeave;
        treeViewItem.Drop += OnDrop;
    }

    private void OnPreviewDrag(object sender, DragEventArgs args)
    {
        args.Handled = true;
        var dropTarget = GetDropTarget(args);

        if (TryGetDraggedGroup(args.Data, out var draggedGroup))
        {
            if (dropTarget is null || !CanDropGroup(draggedGroup, dropTarget))
            {
                args.Effects = DragDropEffects.None;
                SetDropTarget(null);
                return;
            }

            args.Effects = DragDropEffects.Move;
            SetDropTarget(dropTarget);
            return;
        }

        if (!TryGetDraggedEntity(args.Data, out _))
        {
            args.Effects = DragDropEffects.None;
            
            SetDropTarget(null);
            return;
        }
        
        if (dropTarget is null || ReferenceEquals(dropTarget, _dragSourceItem))
        {
            args.Effects = DragDropEffects.None;
            SetDropTarget(null);
            return;
        }

        var targetGroup = GetGroupPathFor(dropTarget);

        if (targetGroup is null)
        {
            args.Effects = DragDropEffects.None;
            SetDropTarget(null);
            return;
        }

        args.Effects = DragDropEffects.Move;
        SetDropTarget(dropTarget);
    }

    private void OnDragLeave(object sender, DragEventArgs args)
    {
        args.Handled = true;

        if (!TryGetDraggedEntity(args.Data, out _) && !TryGetDraggedGroup(args.Data, out _))
            return;

        if (sender is TreeViewItem treeViewItem && ReferenceEquals(treeViewItem, _currentDropTarget))
            SetDropTarget(null);
    }

    private void OnDrop(object sender, DragEventArgs args)
    {
        args.Handled = true;
        SetDropTarget(null);

        if (TryGetDraggedGroup(args.Data, out var draggedGroup))
        {
            var groupTarget = GetDropTarget(args);

            if (groupTarget is not null && CanDropGroup(draggedGroup, groupTarget))
            {
                if (ReferenceEquals(groupTarget, this))
                {
                    MoveGroupToRoot(draggedGroup);
                    return;
                }

                if (groupTarget is SegmentTreeViewItem &&
                    groupTarget.Parent is TreeViewItem destinationGroup &&
                    _groupLookup.ContainsKey(destinationGroup))
                {
                    if (ReferenceEquals(draggedGroup.Parent, destinationGroup))
                    {
                        ReorderGroup(draggedGroup, groupTarget,
                            args.GetPosition(groupTarget).Y >= groupTarget.ActualHeight / 2);
                    }
                    else
                    {
                        MoveGroupInside(draggedGroup, destinationGroup);
                    }
                }
                else if (_groupLookup.ContainsKey(groupTarget))
                {
                    MoveGroupBeside(draggedGroup, groupTarget,
                        args.GetPosition(groupTarget).Y >= groupTarget.ActualHeight / 2);
                }
            }

            return;
        }

        if (!TryGetDraggedEntity(args.Data, out var entity))
            return;

        var treeViewItem = GetDropTarget(args);

        if (treeViewItem is null)
            return;

        var targetGroup = GetGroupPathFor(treeViewItem);

        if (targetGroup is null)
            return;

        var normalizedCurrent = string.IsNullOrEmpty(entity.Group) ? string.Empty : entity.Group;
        var targetEntity = treeViewItem is SegmentTreeViewItem targetItem &&
                           targetItem.Tag is SegmentEntity segmentEntity
            ? segmentEntity
            : null;

        if (!string.Equals(normalizedCurrent, targetGroup, StringComparison.OrdinalIgnoreCase))
            entity.Group = targetGroup;

        if (targetEntity is not null && !ReferenceEquals(entity, targetEntity))
            ReorderEntity(entity, targetEntity,
                args.GetPosition(treeViewItem).Y >= treeViewItem.ActualHeight / 2);
        else
            SaveTreeOrderToEntities();
    }

    private static TreeViewItem? GetDropTarget(DragEventArgs args)
    {
        if (args.OriginalSource is TreeViewItem treeViewItem)
            return treeViewItem;

        return (args.OriginalSource as DependencyObject)?.FindAncestor<TreeViewItem>();
    }

    private static bool TryGetDraggedEntity(IDataObject data, out SegmentEntity entity)
    {
        if (data.GetDataPresent(DragFormat) && data.GetData(DragFormat) is SegmentEntity segmentEntity)
        {
            entity = segmentEntity;
            return true;
        }

        entity = null!;
        return false;
    }

    private static bool TryGetDraggedGroup(IDataObject data, out TreeViewItem group)
    {
        if (data.GetDataPresent(GroupDragFormat) && data.GetData(GroupDragFormat) is TreeViewItem treeViewItem)
        {
            group = treeViewItem;
            return true;
        }

        group = null!;
        return false;
    }

    private bool CanDropGroup(TreeViewItem draggedGroup, TreeViewItem targetGroup)
    {
        if (draggedGroup is null || targetGroup is null ||
            ReferenceEquals(draggedGroup, targetGroup) ||
            !_groupLookup.TryGetValue(draggedGroup, out var draggedPath))
            return false;

        if (ReferenceEquals(targetGroup, this))
            return !ReferenceEquals(draggedGroup.Parent, this);

        var destinationGroup = targetGroup is SegmentTreeViewItem
            ? targetGroup.Parent as TreeViewItem
            : targetGroup;

        if (destinationGroup is null ||
            !_groupLookup.TryGetValue(destinationGroup, out var targetPath))
            return false;

        return !String.Equals(targetPath, draggedPath, StringComparison.OrdinalIgnoreCase) &&
               !targetPath.StartsWith(draggedPath + "\\", StringComparison.OrdinalIgnoreCase);
    }

    private void MoveGroupToRoot(TreeViewItem draggedGroup)
    {
        var oldPath = _groupLookup[draggedGroup];
        var folderName = oldPath.Substring(oldPath.LastIndexOf('\\') + 1);

        if (_groupNodes.TryGetValue(folderName, out var existing) && !ReferenceEquals(existing, draggedGroup))
        {
            ShowDuplicateGroupMessage();
            return;
        }

        MoveGroupPath(oldPath, folderName);
        SaveTreeOrderToEntities();
    }

    private void MoveGroupBeside(TreeViewItem draggedGroup, TreeViewItem targetGroup, bool placeAfter)
    {
        if (ReferenceEquals(draggedGroup.Parent, targetGroup.Parent))
        {
            ReorderGroup(draggedGroup, targetGroup, placeAfter);
            return;
        }

        var oldPath = _groupLookup[draggedGroup];
        var targetPath = _groupLookup[targetGroup];
        var targetSeparator = targetPath.LastIndexOf('\\');
        var targetParentPath = targetSeparator >= 0 ? targetPath.Substring(0, targetSeparator) : String.Empty;
        var folderName = oldPath.Substring(oldPath.LastIndexOf('\\') + 1);
        var newPath = String.IsNullOrEmpty(targetParentPath)
            ? folderName
            : $"{targetParentPath}\\{folderName}";

        if (!String.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
        {
            if (_groupNodes.ContainsKey(newPath))
            {
                ShowDuplicateGroupMessage();
                return;
            }

            MoveGroupPath(oldPath, newPath);
            draggedGroup = _groupNodes[newPath];
        }

        ReorderGroup(draggedGroup, targetGroup, placeAfter);
    }

    private void ReorderGroup(TreeViewItem draggedGroup, TreeViewItem targetGroup, bool placeAfter)
    {
        if (draggedGroup.Parent is not ItemsControl parent)
            return;

        var oldIndex = parent.Items.IndexOf(draggedGroup);
        var targetIndex = parent.Items.IndexOf(targetGroup);

        if (oldIndex < 0 || targetIndex < 0)
            return;

        parent.Items.RemoveAt(oldIndex);

        targetIndex = parent.Items.IndexOf(targetGroup);
        var newIndex = targetIndex + (placeAfter ? 1 : 0);
        parent.Items.Insert(newIndex, draggedGroup);

        SaveTreeOrderToEntities();
    }

    private void MoveGroupInside(TreeViewItem draggedGroup, TreeViewItem targetGroup)
    {
        var oldPath = _groupLookup[draggedGroup];
        var targetPath = _groupLookup[targetGroup];
        var folderName = oldPath.Substring(oldPath.LastIndexOf('\\') + 1);
        var newPath = $"{targetPath}\\{folderName}";

        if (_groupNodes.ContainsKey(newPath))
        {
            ShowDuplicateGroupMessage();
            return;
        }

        MoveGroupPath(oldPath, newPath);
        SaveTreeOrderToEntities();
    }

    private void MoveGroupPath(string oldPath, string newPath)
    {
        var affectedEntities = _segment.Entities.Where(entity =>
        {
            var entityPath = String.IsNullOrEmpty(entity.Group) ? UndefinedGroup : entity.Group;
            return String.Equals(entityPath, oldPath, StringComparison.OrdinalIgnoreCase) ||
                   entityPath.StartsWith(oldPath + "\\", StringComparison.OrdinalIgnoreCase);
        }).ToList();

        foreach (var entity in affectedEntities)
        {
            var entityPath = String.IsNullOrEmpty(entity.Group) ? UndefinedGroup : entity.Group;
            entity.Group = newPath + entityPath.Substring(oldPath.Length);
        }
    }

    private static void ShowDuplicateGroupMessage()
    {
        MessageBox.Show("A group with that name already exists in the destination.",
            "Duplicate Group", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ReorderEntity(SegmentEntity entity, SegmentEntity targetEntity, bool placeAfter)
    {
        if (!_entityItems.TryGetValue(entity, out var entityItem) ||
            !_entityItems.TryGetValue(targetEntity, out var targetItem) ||
            targetItem.Parent is not ItemsControl parent ||
            !ReferenceEquals(entityItem.Parent, parent))
            return;

        parent.Items.Remove(entityItem);
        var targetIndex = parent.Items.IndexOf(targetItem);
        parent.Items.Insert(targetIndex + (placeAfter ? 1 : 0), entityItem);
        SaveTreeOrderToEntities();
    }

    private void ApplyXmlGroupOrder(ItemsControl parent)
    {
        var orderedItems = parent.Items.Cast<object>()
            .OrderBy(GetTreeItemSortId)
            .ToList();

        foreach (var item in orderedItems)
        {
            parent.Items.Remove(item);
            parent.Items.Add(item);

            if (item is TreeViewItem folder && _groupLookup.ContainsKey(folder))
                ApplyXmlGroupOrder(folder);
        }
    }

    private int GetTreeItemSortId(object item)
    {
        if (item is SegmentTreeViewItem entityItem && entityItem.Tag is SegmentEntity entity)
            return entity.SortId ?? Int32.MaxValue;

        if (item is TreeViewItem folder && _groupLookup.TryGetValue(folder, out var groupPath))
            return GetFolderSortId(groupPath);

        return Int32.MaxValue;
    }

    private int GetFolderSortId(string groupPath)
    {
        return _segment.Entities
            .Where(entity =>
            {
                var entityPath = String.IsNullOrEmpty(entity.Group) ? UndefinedGroup : entity.Group;
                return String.Equals(entityPath, groupPath, StringComparison.OrdinalIgnoreCase) ||
                       entityPath.StartsWith(groupPath + "\\", StringComparison.OrdinalIgnoreCase);
            })
            .Where(entity => entity.SortId.HasValue)
            .Select(entity => entity.SortId.Value)
            .DefaultIfEmpty(Int32.MaxValue)
            .Min();
    }

    private void SaveTreeOrderToEntities()
    {
        var sortId = 0;
        AssignEntitySortIds(this, ref sortId);
    }

    private void AssignEntitySortIds(ItemsControl parent, ref int sortId)
    {
        foreach (var item in parent.Items)
        {
            if (item is SegmentTreeViewItem entityItem && entityItem.Tag is SegmentEntity entity)
                entity.SortId = sortId++;
            else if (item is TreeViewItem folder && _groupLookup.ContainsKey(folder))
                AssignEntitySortIds(folder, ref sortId);
        }
    }

    private IEnumerable<TreeViewItem> GetGroupsInTreeOrder(ItemsControl parent)
    {
        foreach (var folder in parent.Items.OfType<TreeViewItem>().Where(item => _groupLookup.ContainsKey(item)))
        {
            yield return folder;

            foreach (var child in GetGroupsInTreeOrder(folder))
                yield return child;
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

    private string GetGroupPathFor(TreeViewItem treeViewItem)
    {
        if (ReferenceEquals(treeViewItem, this))
            return String.Empty;

        if (_groupLookup.TryGetValue(treeViewItem, out var groupPath))
            return String.Equals(groupPath, UndefinedGroup, StringComparison.OrdinalIgnoreCase) ? String.Empty : groupPath;

        if (treeViewItem is SegmentTreeViewItem segmentItem && segmentItem.Tag is SegmentEntity entity)
            return string.IsNullOrEmpty(entity.Group) ? String.Empty : entity.Group;

        return null;
    }

    private void OnEntityPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
    {
        _dragStartPoint = args.GetPosition(this);
        _dragSourceItem = sender as SegmentTreeViewItem;
    }

    private void OnGroupPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
    {
        // A click on an entity bubbles through its folder. Only begin a folder
        // drag when the folder header itself was pressed.
        if (args.OriginalSource is not DependencyObject source ||
            source.FindAncestor<SegmentTreeViewItem>() is not null)
            return;

        _dragStartPoint = args.GetPosition(this);
        _dragSourceGroupItem = sender as TreeViewItem;
    }

    private void OnGroupPreviewMouseMove(object sender, MouseEventArgs args)
    {
        if (_dragStartPoint is null || !ReferenceEquals(sender, _dragSourceGroupItem))
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

        DragDrop.DoDragDrop(_dragSourceGroupItem,
            new DataObject(GroupDragFormat, _dragSourceGroupItem), DragDropEffects.Move);

        ResetDrag();
    }

    private void OnGroupPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs args)
    {
        ResetDrag();
    }

    private void OnEntityPreviewMouseMove(object sender, MouseEventArgs args)
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

        if (_dragSourceItem?.Tag is SegmentEntity entity)
            DragDrop.DoDragDrop(_dragSourceItem, new DataObject(DragFormat, entity), DragDropEffects.Move);

        ResetDrag();
    }

    private void OnEntityPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        ResetDrag();
    }

    private void ResetDrag()
    {
        _dragStartPoint = null;
        _dragSourceItem = null;
        _dragSourceGroupItem = null;
    }
}
