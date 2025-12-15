using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.UI.Controls;

namespace Kesmai.WorldForge.UI;

internal sealed class ComponentsTreeViewItem : TreeViewItem, IDisposable
{
    private readonly Segment _segment;
    private readonly Dictionary<SegmentComponent, SegmentTreeViewItem> _componentItems = new();
    
    private int _nextComponentId;

    public ComponentsTreeViewItem(Segment segment, object header)
    {
        _segment = segment ?? throw new ArgumentNullException(nameof(segment));
        
        Header = header ?? throw new ArgumentNullException(nameof(header));
        Tag = "category:Components";

        ContextMenu = new ContextMenu();
        ContextMenu.AddItem("Add Component", "Add.png", (s, e) =>
        {
            var component = new SegmentComponent
            {
                Name = $"Component {_nextComponentId++}"
            };

            _segment.Components.Add(component);
            component.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });

        _nextComponentId = 0;

        var messenger = WeakReferenceMessenger.Default;

        messenger.Register<SegmentComponentAdded>(this, (_, message) =>
        {
            var componentItem = CreateComponentItem(message.Value);

            if (!Items.Contains(componentItem))
                Items.Add(componentItem);
        });

        messenger.Register<SegmentComponentRemoved>(this, (_, message) =>
        {
            if (_componentItems.Remove(message.Value, out var item))
                Items.Remove(item);
        });

        messenger.Register<SegmentComponentChanged>(this, (_, message) =>
        {
            if (_componentItems.TryGetValue(message.Value, out var item) && item.EditableTextBlock is not null)
                item.EditableTextBlock.Text = message.Value.Name;
        });

        messenger.Register<SegmentComponentsReset>(this, (_, _) =>
        {
            Items.Clear();
            _componentItems.Clear();
        });

        foreach (var component in _segment.Components)
            Items.Add(CreateComponentItem(component));
    }

    public void Dispose()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        
        Items.Clear();
        
        _componentItems.Clear();
    }

    private SegmentTreeViewItem CreateComponentItem(SegmentComponent component)
    {
        if (_componentItems.TryGetValue(component, out var existing))
            return existing;

        var componentItem = new SegmentTreeViewItem(component, Brushes.OrangeRed, false)
        {
            Tag = component,
        };

        componentItem.ContextMenu = new ContextMenu();
        componentItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => componentItem.Rename());
        componentItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => component.Copy(_segment));
        componentItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) =>
        {
            _segment.Components.Remove(component);

            if (componentItem.Parent is ItemsControl parent)
                parent.Items.Remove(componentItem);
        });

        _componentItems.Add(component, componentItem);
        return componentItem;
    }
}
