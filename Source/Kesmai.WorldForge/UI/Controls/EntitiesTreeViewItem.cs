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

internal sealed class EntitiesTreeViewItem : TreeViewItem, IDisposable
{
    private const string UndefinedGroup = "Ungrouped";
    private const string DragFormat = "Kesmai.WorldForge.EntitiesTreeViewItem.Entity";

    private readonly Segment _segment;
    private readonly Dictionary<SegmentEntity, SegmentTreeViewItem> _entityItems = new();
    private readonly Dictionary<string, TreeViewItem> _groupNodes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<TreeViewItem, string> _groupLookup = new();
    
    private int _nextEntityId;
    
    private Point? _dragStartPoint;
    private SegmentTreeViewItem _dragSourceItem;
    private TreeViewItem _currentDropTarget;
    private DropHighlightAdorner _currentDropAdorner;

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
        });

        messenger.Register<SegmentEntityRemoved>(this, (_, message) =>
        {
            if (_entityItems.Remove(message.Value, out var entityNode) && entityNode.Parent is ItemsControl parent)
                parent.Items.Remove(entityNode);
        });

        messenger.Register<SegmentEntityChanged>(this, (_, message) =>
        {
            if (!_entityItems.TryGetValue(message.Value, out var entityItem))
                return;

            entityItem.EditableTextBlock.Text = message.Value.Name;
            
            Bind(message.Value, entityItem);
        });

        foreach (var entity in _segment.Entities)
            Bind(entity, CreateEntityItem(entity));
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
            currentParent.Items.Remove(entityItem);

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
                folderNode.ContextMenu.AddItem("Add Entity", "Add.png", (s, e) => AddEntity(path));

                parentNode.Items.Add(folderNode);
                _groupNodes.Add(path, folderNode);
                _groupLookup[folderNode] = path;
                AttachDropTarget(folderNode);
            }

            parentNode = folderNode;
        }

        return parentNode;
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
        if (args.Source is not UIElement dragControl)
            return;
        
        args.Handled = true;

        if (!TryGetDraggedEntity(args.Data, out var entity))
        {
            args.Effects = DragDropEffects.None;
            
            SetDropTarget(null);
            return;
        }
        
        var dragAncestor = dragControl.FindAncestor<TreeViewItem>();
        
        // find ancestor TreeViewItem
        if (dragAncestor is null || ReferenceEquals(dragAncestor, _dragSourceItem))
        {
            args.Effects = DragDropEffects.None;
            SetDropTarget(null);
            return;
        }

        var targetGroup = GetGroupPathFor(dragAncestor);

        if (targetGroup is null)
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

        if (!TryGetDraggedEntity(args.Data, out _))
            return;

        if (sender is TreeViewItem treeViewItem && ReferenceEquals(treeViewItem, _currentDropTarget))
            SetDropTarget(null);
    }

    private void OnDrop(object sender, DragEventArgs args)
    {
        args.Handled = true;
        SetDropTarget(null);

        if (!TryGetDraggedEntity(args.Data, out var entity))
            return;

        if (sender is not TreeViewItem treeViewItem)
            return;

        var targetGroup = GetGroupPathFor(treeViewItem);

        if (targetGroup is null)
            return;

        var normalizedCurrent = string.IsNullOrEmpty(entity.Group) ? string.Empty : entity.Group;

        if (string.Equals(normalizedCurrent, targetGroup, StringComparison.OrdinalIgnoreCase))
            return;

        entity.Group = targetGroup;
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
    }
}