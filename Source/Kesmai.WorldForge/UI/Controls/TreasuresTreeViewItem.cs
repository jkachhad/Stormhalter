using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.UI.Controls;

namespace Kesmai.WorldForge.UI;

internal sealed class TreasuresTreeViewItem : TreeViewItem, IDisposable
{
    private readonly Segment _segment;
    private readonly Dictionary<SegmentTreasure, SegmentTreeViewItem> _treasureItems = new();
    
    private int _nextTreasureId;

    public TreasuresTreeViewItem(Segment segment, object header)
    {
        _segment = segment ?? throw new ArgumentNullException(nameof(segment));
        
        Header = header ?? throw new ArgumentNullException(nameof(header));
        Tag = "category:Treasure";

        ContextMenu = new ContextMenu();
        ContextMenu.AddItem("Add Treasures", "Add.png", (s, e) =>
        {
            var treasure = new SegmentTreasure
            {
                Name = $"Treasure {_nextTreasureId++}"
            };

            _segment.Treasures.Add(treasure);
            treasure.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });
        ContextMenu.AddItem("Add Hoard", "Add.png", (s, e) =>
        {
            var hoard = new SegmentHoard
            {
                Name = $"Hoard {_nextTreasureId++}"
            };

            _segment.Treasures.Add(hoard);
            hoard.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });

        _nextTreasureId = 0;

        var messenger = WeakReferenceMessenger.Default;

        messenger.Register<SegmentTreasureAdded>(this, (_, message) =>
        {
            var treasureItem = CreateTreasureItem(message.Value);

            if (!Items.Contains(treasureItem))
                Items.Add(treasureItem);
        });

        messenger.Register<SegmentTreasureRemoved>(this, (_, message) =>
        {
            if (_treasureItems.Remove(message.Value, out var item))
                Items.Remove(item);
        });

        messenger.Register<SegmentTreasureChanged>(this, (_, message) =>
        {
            if (_treasureItems.TryGetValue(message.Value, out var item) && item.EditableTextBlock is not null)
                item.EditableTextBlock.Text = message.Value.Name;
        });

        messenger.Register<SegmentTreasuresReset>(this, (_, _) =>
        {
            Items.Clear();
            _treasureItems.Clear();
        });

        foreach (var treasure in _segment.Treasures)
            Items.Add(CreateTreasureItem(treasure));
    }

    public void Dispose()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        
        Items.Clear();
        
        _treasureItems.Clear();
    }

    private SegmentTreeViewItem CreateTreasureItem(SegmentTreasure treasure)
    {
        if (_treasureItems.TryGetValue(treasure, out var existing))
            return existing;

        var brush = treasure is SegmentHoard ? Brushes.Red : Brushes.Green;

        var treasureItem = new SegmentTreeViewItem(treasure, brush, true)
        {
            Tag = treasure,
        };

        treasureItem.ContextMenu = new ContextMenu();
        treasureItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => treasureItem.Rename());
        treasureItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => treasure.Copy(_segment));
        treasureItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) =>
        {
            _segment.Treasures.Remove(treasure);

            if (treasureItem.Parent is ItemsControl parent)
                parent.Items.Remove(treasureItem);
        });

        _treasureItems.Add(treasure, treasureItem);
        return treasureItem;
    }
}
