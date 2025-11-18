using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.UI.Controls;

namespace Kesmai.WorldForge.UI;

internal sealed class BrushesTreeViewItem : TreeViewItem, IDisposable
{
    private readonly Segment _segment;
    private readonly Dictionary<SegmentBrush, SegmentTreeViewItem> _brushItems = new();
    
    private int _nextBrushId;

    public BrushesTreeViewItem(Segment segment, object header)
    {
        _segment = segment ?? throw new ArgumentNullException(nameof(segment));
        
        Header = header ?? throw new ArgumentNullException(nameof(header));
        Tag = "category:Brushes";

        ContextMenu = new ContextMenu();
        ContextMenu.AddItem("Add Brush", "Add.png", (s, e) =>
        {
            var brush = new SegmentBrush
            {
                Name = $"Brush {_nextBrushId++}"
            };

            _segment.Brushes.Add(brush);
            brush.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });

        _nextBrushId = 0;

        var messenger = WeakReferenceMessenger.Default;

        messenger.Register<SegmentBrushAdded>(this, (_, message) =>
        {
            var brushItem = CreateBrushItem(message.Value);

            if (!Items.Contains(brushItem))
                Items.Add(brushItem);
        });

        messenger.Register<SegmentBrushRemoved>(this, (_, message) =>
        {
            if (_brushItems.Remove(message.Value, out var item))
                Items.Remove(item);
        });

        messenger.Register<SegmentBrushChanged>(this, (_, message) =>
        {
            if (_brushItems.TryGetValue(message.Value, out var item) && item.EditableTextBlock is not null)
                item.EditableTextBlock.Text = message.Value.Name;
        });

        messenger.Register<SegmentBrushesReset>(this, (_, _) =>
        {
            Items.Clear();
            _brushItems.Clear();
        });

        foreach (var brush in _segment.Brushes)
            Items.Add(CreateBrushItem(brush));
    }

    public void Dispose()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        
        Items.Clear();
        
        _brushItems.Clear();
    }

    private SegmentTreeViewItem CreateBrushItem(SegmentBrush brush)
    {
        if (_brushItems.TryGetValue(brush, out var existing))
            return existing;

        var brushItem = new SegmentTreeViewItem(brush, Brushes.MediumSeaGreen, true)
        {
            Tag = brush,
        };

        brushItem.ContextMenu = new ContextMenu();
        brushItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => brushItem.Rename());
        brushItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => brush.Copy(_segment));
        brushItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) =>
        {
            _segment.Brushes.Remove(brush);

            if (brushItem.Parent is ItemsControl parent)
                parent.Items.Remove(brushItem);
        });

        _brushItems.Add(brush, brushItem);
        return brushItem;
    }
}
