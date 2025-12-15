using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.UI;

internal sealed class LocationsTreeViewItem : TreeViewItem, IDisposable
{
    private readonly Segment _segment;
    private readonly Dictionary<SegmentLocation, SegmentTreeViewItem> _locationItems = new();
    
    private int _nextLocationId;

    public LocationsTreeViewItem(Segment segment, object header)
    {
        _segment = segment ?? throw new ArgumentNullException(nameof(segment));
        
        Header = header ?? throw new ArgumentNullException(nameof(header));
        Tag = "category:Locations";

        ContextMenu = new ContextMenu();
        ContextMenu.AddItem("Add Location", "Add.png", (s, e) =>
        {
            var location = new SegmentLocation
            {
                Name = $"Location {_nextLocationId++}"
            };

            _segment.Locations.Add(location);

            location.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });

        _nextLocationId = 0;

        var messenger = WeakReferenceMessenger.Default;

        messenger.Register<SegmentLocationAdded>(this, (_, message) =>
        {
            var locationItem = CreateLocationItem(message.Value);

            if (!Items.Contains(locationItem))
                Items.Add(locationItem);
        });
        messenger.Register<SegmentLocationRemoved>(this, (_, message) =>
        {
            if (_locationItems.Remove(message.Value, out var item))
                Items.Remove(item);
        });
        messenger.Register<SegmentLocationChanged>(this, (_, message) =>
        {
            if (_locationItems.TryGetValue(message.Value, out var item) && item.EditableTextBlock is not null)
                item.EditableTextBlock.Text = message.Value.Name;
        });
        messenger.Register<SegmentLocationsReset>(this, (_, _) => 
            ResetLocations());

        foreach (var location in _segment.Locations)
            Items.Add(CreateLocationItem(location));
    }

    public void Dispose()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);

        ResetLocations();
    }
    
    private void ResetLocations()
    {
        Items.Clear();
        
        _locationItems.Clear();
    }
    
    private SegmentTreeViewItem CreateLocationItem(SegmentLocation location)
    {
        if (_locationItems.TryGetValue(location, out var existing))
            return existing;

        var isReserved = location.IsReserved;

        var locationItem = new SegmentTreeViewItem(location, isReserved ? Brushes.LightSlateGray : Brushes.LightPink, true)
        {
            Tag = location,
        };

        locationItem.EditableTextBlock.IsEditable = !isReserved;

        locationItem.ContextMenu = new ContextMenu();

        if (!isReserved)
            locationItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => locationItem.Rename());

        locationItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => location.Copy(_segment));

        if (!isReserved)
        {
            locationItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) =>
            {
                _segment.Locations.Remove(location);

                if (locationItem.Parent is ItemsControl parent)
                    parent.Items.Remove(locationItem);
            });
        }

        _locationItems.Add(location, locationItem);
        
        return locationItem;
    }
}
