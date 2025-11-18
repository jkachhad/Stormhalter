using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.UI.Controls;

namespace Kesmai.WorldForge.UI;

internal sealed class RegionsTreeViewItem : TreeViewItem, IDisposable
{
    private readonly Segment _segment;
    private readonly Dictionary<SegmentRegion, SegmentTreeViewItem> _regionItems = new();
    private readonly Dictionary<SegmentSubregion, SegmentTreeViewItem> _subregionItems = new();
    
    private int _nextRegionId;

    public RegionsTreeViewItem(Segment segment, object header)
    {
        _segment = segment ?? throw new ArgumentNullException(nameof(segment));

        Header = header ?? throw new ArgumentNullException(nameof(header));
        Tag = "category:Regions";

        ContextMenu = new ContextMenu();
        ContextMenu.AddItem("Add Region", "Add.png", (s, e) =>
        {
            var region = new SegmentRegion
            {
                Name = $"Region {_nextRegionId++}"
            };

            _segment.Regions.Add(region);
            
            region.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });

        _nextRegionId = 0;

        var messenger = WeakReferenceMessenger.Default;
        
        messenger.Register<SegmentRegionAdded>(this, (_, message) =>
        {
            var regionItem = CreateRegionItem(message.Value);

            if (!Items.Contains(regionItem))
                Items.Add(regionItem);

            ApplyRegionSort();
        });
        messenger.Register<SegmentRegionRemoved>(this, (_, message) =>
        {
            if (_regionItems.Remove(message.Value, out var item))
                Items.Remove(item);
        });
        messenger.Register<SegmentRegionChanged>(this, (_, message) =>
        {
            var region = message.Value;

            if (_regionItems.TryGetValue(region, out var item))
            {
                UpdateTreeItemText(item, $"[{region.ID}] {{0}}");
                ApplyRegionSort();
            }
        });
        messenger.Register<SegmentSubregionAdded>(this, (_, message) =>
        {
            CreateSubregionItem(message.Value);
        });
        messenger.Register<SegmentSubregionRemoved>(this, (_, message) =>
        {
            if (_subregionItems.Remove(message.Value, out var item))
            {
                var subregion = message.Value;
                var parentRegion = _segment.GetRegion(subregion.Region);

                if (parentRegion is not null && _regionItems.TryGetValue(parentRegion, out var parentNode))
                    parentNode.Items.Remove(item);
            }
        });
        messenger.Register<SegmentSubregionChanged>(this, (_, message) =>
        {
            if (_subregionItems.TryGetValue(message.Value, out var item))
                UpdateTreeItemText(item);
        });

        foreach (var region in _segment.Regions)
            Items.Add(CreateRegionItem(region));

        foreach (var subregion in _segment.Subregions)
            CreateSubregionItem(subregion);

        ApplyRegionSort();
    }

    public void Dispose()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);

        Items.Clear();
        
        _regionItems.Clear();
        _subregionItems.Clear();
    }

    private SegmentTreeViewItem CreateRegionItem(SegmentRegion region)
    {
        if (_regionItems.TryGetValue(region, out var existing))
            return existing;

        var regionItem = new SegmentTreeViewItem(region, Brushes.MediumPurple, false, $"[{region.ID}] {{0}}")
        {
            Tag = region,
        };

        regionItem.ContextMenu = new ContextMenu();
        regionItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => regionItem.Rename());
        regionItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => region.Copy(_segment));
        regionItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) =>
        {
            _segment.Regions.Remove(region);

            if (regionItem.Parent is ItemsControl parent)
                parent.Items.Remove(regionItem);
        });

        _regionItems.Add(region, regionItem);
        return regionItem;
    }

    private void CreateSubregionItem(SegmentSubregion subregion)
    {
        if (!_subregionItems.TryGetValue(subregion, out var subregionItem))
        {
            subregionItem = new SegmentTreeViewItem(subregion, Brushes.Gray, false)
            {
                Tag = subregion,
            };

            subregionItem.ContextMenu = new ContextMenu();
            subregionItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => subregionItem.Rename());
            subregionItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => subregion.Copy(_segment));
            subregionItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) =>
            {
                _segment.Subregions.Remove(subregion);

                if (subregionItem.Parent is ItemsControl parent)
                    parent.Items.Remove(subregionItem);
            });

            _subregionItems.Add(subregion, subregionItem);
        }

        var parentRegion = _segment.GetRegion(subregion.Region);

        if (parentRegion is null)
            return;

        if (_regionItems.TryGetValue(parentRegion, out var regionItem))
        {
            if (!regionItem.Items.Contains(subregionItem))
                regionItem.Items.Add(subregionItem);
        }
    }

    private static void UpdateTreeItemText(SegmentTreeViewItem item, string format = "")
    {
        if (item is null || item.EditableTextBlock is null)
            return;

        if (!string.IsNullOrWhiteSpace(format))
            item.EditableTextBlock.TextFormat = format;

        if (item.Tag is ISegmentObject segmentObject)
            item.EditableTextBlock.Text = segmentObject.Name;
    }

    private void ApplyRegionSort()
    {
        var sortedItems = Items.OfType<SegmentTreeViewItem>()
            .OrderBy(item => item.Tag is SegmentRegion region ? region.ID : int.MaxValue)
            .ThenBy(item => item.EditableTextBlock.Text, StringComparer.OrdinalIgnoreCase)
            .ToList();

        Items.Clear();

        foreach (var item in sortedItems)
            Items.Add(item);
    }
}
