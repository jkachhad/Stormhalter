using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.UI.Controls;

#nullable enable

namespace Kesmai.WorldForge.UI;

public class SegmentObjectSelected(ISegmentObject target) : ValueChangedMessage<ISegmentObject>(target);

public partial class SegmentTreeControl : UserControl
{
    private static int _nextId;
    
    public static readonly DependencyProperty SegmentProperty =
        DependencyProperty.Register(nameof(Segment), typeof(Segment), typeof(SegmentTreeControl),
            new PropertyMetadata(null, OnSegmentChanged));

    public Segment? Segment
    {
        get => (Segment?)GetValue(SegmentProperty);
        set => SetValue(SegmentProperty, value);
    }
    
    private ISegmentObject? _copyObject;
    private TreeViewItem? _currentSelection;
    
    public SegmentTreeControl()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<SegmentRegionAdded>(this, (r, m) => OnRegionAdded(m.Value));
        WeakReferenceMessenger.Default.Register<SegmentRegionRemoved>(this, (r, m) => OnRegionRemoved(m.Value));

        WeakReferenceMessenger.Default.Register<SegmentSubregionAdded>(this, (r, m) => OnSubregionAdded(m.Value));
        WeakReferenceMessenger.Default.Register<SegmentSubregionRemoved>(this, (r, m) => OnSubregionRemoved(m.Value));

        WeakReferenceMessenger.Default.Register<SegmentLocationAdded>(this, (r, m) => OnLocationAdded(m.Value));
        WeakReferenceMessenger.Default.Register<SegmentLocationRemoved>(this, (r, m) => OnLocationRemoved(m.Value));
        WeakReferenceMessenger.Default.Register<SegmentLocationsReset>(this, (r, m) => OnLocationsReset());

        WeakReferenceMessenger.Default.Register<SegmentComponentAdded>(this, (r, m) => OnComponentAdded(m.Value));
        WeakReferenceMessenger.Default.Register<SegmentComponentRemoved>(this, (r, m) => OnComponentRemoved(m.Value));

        WeakReferenceMessenger.Default.Register<SegmentBrushAdded>(this, (r, m) => OnBrushAdded(m.Value));
        WeakReferenceMessenger.Default.Register<SegmentBrushRemoved>(this, (r, m) => OnBrushRemoved(m.Value));
        
        WeakReferenceMessenger.Default.Register<SegmentTemplateAdded>(this, (r, m) => OnTemplateAdded(m.Value));
        WeakReferenceMessenger.Default.Register<SegmentTemplateRemoved>(this, (r, m) => OnTemplateRemoved(m.Value));
        
        WeakReferenceMessenger.Default.Register<SegmentEntityAdded>(this, (r, m) => OnEntityAdded(m.Value));
        WeakReferenceMessenger.Default.Register<SegmentEntityRemoved>(this, (r, m) => OnEntityRemoved(m.Value));
        WeakReferenceMessenger.Default.Register<SegmentEntityChanged>(this, (r, m) =>
        {
            // for entity changes, we need to update the grouping in the tree.
            OnEntityRemoved(m.Value);
            OnEntityAdded(m.Value);
        });
        
        WeakReferenceMessenger.Default.Register<SegmentTreasureAdded>(this, (r, m) => OnTreasureAdded(m.Value));
        WeakReferenceMessenger.Default.Register<SegmentTreasureRemoved>(this, (r, m) => OnTreasureRemoved(m.Value));
      
        WeakReferenceMessenger.Default.Register<SegmentSpawnAdded>(this, (r, m) => OnSpawnAdded(m.Value));
        WeakReferenceMessenger.Default.Register<SegmentSpawnRemoved>(this, (r, m) => OnSpawnRemoved(m.Value));
        WeakReferenceMessenger.Default.Register<SegmentSpawnChanged>(this, (r, m) =>
        {
            // for spawn changes, we need to update the grouping in the tree.
            OnSpawnRemoved(m.Value);
            OnSpawnAdded(m.Value);
        });
        
        _tree.SelectedItemChanged += OnItemSelected;
        _tree.KeyDown += OnKeyDown;
    }

    private static void OnSegmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SegmentTreeControl)d;

        if (e.OldValue is Segment oldSegment)
        {
            oldSegment.PropertyChanged -= control.OnSegmentPropertyChanged;
        }

        control.Reset();

        if (e.NewValue is Segment newSegment)
        {
            newSegment.PropertyChanged += control.OnSegmentPropertyChanged;
        }

        control.Update();
    }

    private void OnSegmentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Segment.Directory))
        {
            Update();
        }
        // Segment name changes no longer affect the tree structure, since the
        // segment name is no longer shown as a root node.
    }
    
    private void OnKeyDown(object sender, KeyEventArgs args)
    {
        // copy
        if (args.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
        {
            if (_tree.SelectedItem is TreeViewItem item && item.Tag is ISegmentObject segmentObject)
                _copyObject = segmentObject;
            
            args.Handled = true;
        }
        else if (args.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
        {
            if (_copyObject != null)
            {
                var applicationPresenter = ServiceLocator.Current
                    .GetInstance<ApplicationPresenter>();
                
                _copyObject.Copy(Segment);
                _copyObject.Present(applicationPresenter);
            }

            args.Handled = true;
        }
    }

    private TreeViewItem? _segmentNode;
    private TreeViewItem? _brushesNode;
    private TreeViewItem? _templatesNode;
    private TreeViewItem? _regionsNode;
    private TreeViewItem? _locationsNode;
    private TreeViewItem? _componentsNode;
    private TreeViewItem? _entitiesNode;
    private TreeViewItem? _spawnersNode;
    private TreeViewItem? _treasureNode;

    private void EnsureSegmentNode()
    {
        if (_segmentNode is not null)
            return;
        
        _segmentNode = new TreeViewItem
        {
            Tag = Segment,
            Header = CreateHeader("Segment", "Segment.png"),
        };
    }
    
    private Dictionary<SegmentRegion, SegmentTreeViewItem> _regionItems = new();
    private Dictionary<SegmentSubregion, SegmentTreeViewItem> _subregionItems = new();
    private Dictionary<SegmentLocation, SegmentTreeViewItem> _locationItems = new();
    private Dictionary<SegmentComponent, SegmentTreeViewItem> _componentItems = new();
    private Dictionary<SegmentBrush, SegmentTreeViewItem> _brushItems = new();
    private Dictionary<SegmentTemplate, SegmentTreeViewItem> _templateItems = new();
    private Dictionary<SegmentEntity, SegmentTreeViewItem> _entityItems = new();
    private Dictionary<SegmentTreasure, SegmentTreeViewItem> _treasureItems = new();
    private Dictionary<SegmentSpawner, SegmentTreeViewItem> _spawnItems = new();

    private Dictionary<string, TreeViewItem> _entityGroupNodes = new();
    private Dictionary<int, TreeViewItem> _spawnGroupNodes = new();

    private void Reset()
    {
        _tree.SelectedItemChanged -= OnItemSelected;

        if (_currentSelection != null)
        {
            _currentSelection.IsSelected = false;
            _currentSelection = null;
        }

        _tree.Items.Clear();
        _tree.SelectedItemChanged += OnItemSelected;

        _segmentNode = null;
        _brushesNode = null;
        _templatesNode = null;
        _regionsNode = null;
        _locationsNode = null;
        _componentsNode = null;
        _entitiesNode = null;
        _spawnersNode = null;
        _treasureNode = null;

        _regionItems.Clear();
        _subregionItems.Clear();
        _locationItems.Clear();
        _componentItems.Clear();
        _brushItems.Clear();
        _templateItems.Clear();
        _entityItems.Clear();
        _treasureItems.Clear();
        _spawnItems.Clear();

        _entityGroupNodes.Clear();
        _spawnGroupNodes.Clear();
    }
    
    
    private void OnRegionAdded(SegmentRegion region)
    {
        EnsureRegionsNode();
        
        // a region has been added, create its tree node.
        if (!_regionItems.TryGetValue(region, out var regionItem))
        {
            regionItem = new SegmentTreeViewItem(region, Brushes.MediumPurple, false)
            {
                Tag = region,
            };

            regionItem.ContextMenu = new ContextMenu();
            regionItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => RenameSegmentObject(region, regionItem));
            regionItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => DuplicateSegmentObject(region));
            regionItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) =>
            {
                var segment = Segment;
                if (segment is null)
                    return;

                DeleteSegmentObject(region, regionItem, segment.Regions);
            });
            
            _regionItems.Add(region, regionItem);
        }
        
        if (!_regionsNode.Items.Contains(regionItem))
            _regionsNode.Items.Add(regionItem);
    }
    
    private void OnRegionRemoved(SegmentRegion region)
    {
        // a region has been removed, delete its tree node.
        if (_regionItems.Remove(region, out var item))
        {
            if (_regionsNode != null)
                _regionsNode.Items.Remove(item);
        }
    }

    private void OnSubregionAdded(SegmentSubregion subregion)
    {
        // subregion has been added, create its tree node.
        // the nodes rest on the parent region.
        
        // find the parent node.
        var parentRegion = _regionItems.Keys.FirstOrDefault(r => r.ID == subregion.Region);

        if (parentRegion is null)
            return;
        
        if (!_subregionItems.TryGetValue(subregion, out var subregionItem))
        {
            subregionItem = new SegmentTreeViewItem(subregion, Brushes.Gray, false)
            {
                Tag = subregion,
            };

            subregionItem.ContextMenu = new ContextMenu();
            subregionItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => RenameSegmentObject(subregion, subregionItem));
            subregionItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => DuplicateSegmentObject(subregion));
            subregionItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) =>
            {
                var segment = Segment;
                if (segment is null)
                    return;

                DeleteSegmentObject(subregion, subregionItem, segment.Subregions);
            });

            _subregionItems.Add(subregion, subregionItem);
        }

        if (_regionItems.TryGetValue(parentRegion, out var parentRegionNode))
            parentRegionNode.Items.Add(subregionItem);
    }
    
    private void OnSubregionRemoved(SegmentSubregion subregion)
    {
        // a subregion has been removed, delete its tree node.
        if (_subregionItems.Remove(subregion, out var item))
        {
            // find the parent region node.
            var parentRegion = _regionItems.Keys.FirstOrDefault(r => r.ID == subregion.Region);

            if (parentRegion is null)
                return;

            if (_regionItems.TryGetValue(parentRegion, out var parentRegionNode))
                parentRegionNode.Items.Remove(item);
        }
    }

    private void OnLocationAdded(SegmentLocation location)
    {
        EnsureLocationsNode();

        // a location has been added, create its tree node.
        if (!_locationItems.TryGetValue(location, out var locationItem))
        {
            var isReserved = location.IsReserved;
            
            locationItem = new SegmentTreeViewItem(location, (isReserved ? Brushes.LightSlateGray : Brushes.LightPink), true)
            {
                Tag = location,
            };

            locationItem.EditableTextBlock.IsEditable = !isReserved;

            locationItem.ContextMenu = new ContextMenu();
            
            if (!isReserved)
                locationItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => RenameSegmentObject(location, locationItem));
            
            locationItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => DuplicateSegmentObject(location));
            
            if (!isReserved)
                locationItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) =>
                {
                    var segment = Segment;
                    if (segment is null)
                        return;

                    DeleteSegmentObject(location, locationItem, segment.Locations);
                });

            _locationItems.Add(location, locationItem);
        }

        if (!_locationsNode.Items.Contains(locationItem))
            _locationsNode.Items.Add(locationItem);
    }
    
    private void OnLocationRemoved(SegmentLocation location)
    {
        // a location has been removed, delete its tree node.
        if (_locationItems.Remove(location, out var item))
        {
            if (_locationsNode != null)
                _locationsNode.Items.Remove(item);
        }
    }

    private void OnLocationsReset()
    {
        if (_locationsNode != null)
            _locationsNode.Items.Clear();
        
        _locationItems.Clear();
    }

    private void EnsureLocationsNode()
    {
        if (_locationsNode is not null)
            return;
        
        _locationsNode = new TreeViewItem
        {
            Tag = $"category:Locations",
            Header = CreateHeader("Locations", "Locations.png")
        };
            
        _locationsNode.ContextMenu = new ContextMenu();
        _locationsNode.ContextMenu.AddItem("Add Location", "Add.png", (s, e) =>
        {
            var location = new SegmentLocation
            {
                Name = $"Location {_nextId++}"
            };
            
            var segment = Segment;
            if (segment is null)
                return;

            segment.Locations.Add(location);

            // present the new location to the user.
            location.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });
    }
    
    private void EnsureRegionsNode()
    {
        if (_regionsNode is not null) 
            return;
        
        _regionsNode = new TreeViewItem
        {
            Tag = $"category:Regions",
            Header = CreateHeader("Regions", "Regions.png")
        };
            
        _regionsNode.ContextMenu = new ContextMenu();
        _regionsNode.ContextMenu.AddItem("Add Region", "Add.png", (s, e) =>
        {
            var region = new SegmentRegion
            {
                Name = $"Region {_nextId++}"
            };
            
            var segment = Segment;
            if (segment is null)
                return;

            segment.Regions.Add(region);

            // present the new region to the user.
            region.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });
    }

    public void OnComponentAdded(SegmentComponent component)
    {
        EnsureComponentsNode();
        
        // a component has been added, create its tree node.
        if (!_componentItems.TryGetValue(component, out var componentItem))
        {
            componentItem = new SegmentTreeViewItem(component, Brushes.OrangeRed, false)
            {
                Tag = component,
            };

            componentItem.ContextMenu = new ContextMenu();
            componentItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => RenameSegmentObject(component, componentItem));
            componentItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => DuplicateSegmentObject(component));
            componentItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) =>
            {
                var segment = Segment;
                if (segment is null)
                    return;

                DeleteSegmentObject(component, componentItem, segment.Components);
            });
            
            _componentItems.Add(component, componentItem);
        }
        
        if (!_componentsNode.Items.Contains(componentItem))
            _componentsNode.Items.Add(componentItem);
    }
    
    public void OnComponentRemoved(SegmentComponent component)
    {
        // a component has been removed, delete its tree node.
        if (_componentItems.Remove(component, out var item))
        {
            if (_componentsNode != null)
                _componentsNode.Items.Remove(item);
        }
    }
    
    private void OnBrushAdded(SegmentBrush brush)
    {
        EnsureBrushesNode();

        // a brush has been added, create its tree node.
        if (!_brushItems.TryGetValue(brush, out var brushItem))
        {
            brushItem = new SegmentTreeViewItem(brush, Brushes.MediumSeaGreen, true)
            {
                Tag = brush,
            };

            brushItem.ContextMenu = new ContextMenu();
            brushItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => RenameSegmentObject(brush, brushItem));
            brushItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => DuplicateSegmentObject(brush));
            brushItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) =>
            {
                var segment = Segment;
                if (segment is null)
                    return;

                DeleteSegmentObject(brush, brushItem, segment.Brushes);
            });
            
            _brushItems.Add(brush, brushItem);
        }
        
        if (!_brushesNode.Items.Contains(brushItem))
            _brushesNode.Items.Add(brushItem);
    }
    
    private void OnBrushRemoved(SegmentBrush brush)
    {
        // a brush has been removed, delete its tree node.
        if (_brushItems.Remove(brush, out var item))
        {
            if (_brushesNode != null)
                _brushesNode.Items.Remove(item);
        }
    }
    
    private void EnsureComponentsNode()
    {
        if (_componentsNode is not null)
            return;
        
        _componentsNode = new TreeViewItem
        {
            Tag = $"category:Components",
            Header = CreateHeader("Components", "Terrain.png")
        };
            
        _componentsNode.ContextMenu = new ContextMenu();
        _componentsNode.ContextMenu.AddItem("Add Component", "Add.png", (s, e) =>
        {
            var component = new SegmentComponent()
            {
                Name = $"Component {_nextId++}"
            };
            
            var segment = Segment;
            if (segment is null)
                return;

            segment.Components.Add(component);
            
            component.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });
    }
    
    private void EnsureBrushesNode()
    {
        if (_brushesNode is not null)
            return;

        _brushesNode = new TreeViewItem
        {
            Tag = $"category:Brushes",
            Header = CreateHeader("Brushes", "Editor-Icon-Paint.png")
        };
        
        _brushesNode.ContextMenu = new ContextMenu();
        _brushesNode.ContextMenu.AddItem("Add Brush", "Add.png", (s, e) =>
        {
            var brush = new SegmentBrush
            {
                Name = $"Brush {_nextId++}"
            };

            var segment = Segment;
            if (segment is null)
                return;

            segment.Brushes.Add(brush);

            brush.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });
    }
    
    private void OnTemplateAdded(SegmentTemplate template)
    {
        EnsureTemplatesNode();

        // a template has been added, create its tree node.
        if (!_templateItems.TryGetValue(template, out var templateItem))
        {
            templateItem = new SegmentTreeViewItem(template, Brushes.SteelBlue, true)
            {
                Tag = template,
            };

            templateItem.ContextMenu = new ContextMenu();
            templateItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => RenameSegmentObject(template, templateItem));
            templateItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => DuplicateSegmentObject(template));
            templateItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) =>
            {
                var segment = Segment;
                if (segment is null)
                    return;

                DeleteSegmentObject(template, templateItem, segment.Templates);
            });
            
            _templateItems.Add(template, templateItem);
        }
        
        if (!_templatesNode.Items.Contains(templateItem))
            _templatesNode.Items.Add(templateItem);
    }
    
    private void OnTemplateRemoved(SegmentTemplate template)
    {
        // a template has been removed, delete its tree node.
        if (_templateItems.Remove(template, out var item))
        {
            if (_templatesNode != null)
                _templatesNode.Items.Remove(item);
        }
    }

    private void EnsureTemplatesNode()
    {
        if (_templatesNode is not null)
            return;
        
        _templatesNode = new TreeViewItem
        {
            Tag = $"category:Templates",
            Header = CreateHeader("Templates", "Gear.png")
        };
            
        _templatesNode.ContextMenu = new ContextMenu();
        _templatesNode.ContextMenu.AddItem("Add Template", "Add.png", (s, e) =>
        {
            var template = new SegmentTemplate
            {
                Name = $"Template {_nextId++}"
            };
            
            var segment = Segment;
            if (segment is null)
                return;

            segment.Templates.Add(template);
                
            // present the new template to the user.
            template.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });
    }

    private void OnEntityAdded(SegmentEntity entity)
    {
        EnsureEntitiesNode();
        
        // an entity has been added, create its tree node.
        if (!_entityItems.TryGetValue(entity, out var entityItem))
        {
            entityItem = new SegmentTreeViewItem(entity, Brushes.Yellow, true)
            {
                Tag = entity,
            };

            entityItem.ContextMenu = new ContextMenu();
            entityItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => RenameSegmentObject(entity, entityItem));
            entityItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => DuplicateSegmentObject(entity));
            entityItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) =>
            {
                var segment = Segment;
                if (segment is null)
                    return;

                DeleteSegmentObject(entity, entityItem, segment.Entities);
            });
            
            _entityItems.Add(entity, entityItem);
        }
        
        // parse the group path and insert into the tree.
        var groupPath = String.IsNullOrEmpty(entity.Group) ? "Ungrouped" : entity.Group;
        
        if (!_entityGroupNodes.TryGetValue(groupPath, out var parentNode))
        {
            parentNode = _entitiesNode!;
            
            // split the path into folders.
            var groupFolders = groupPath.Split(@"\");

            for (int i = 0; i < groupFolders.Length; i++)
            {
                var folderPath = String.Join(@"\", groupFolders.Take(i + 1));

                if (!_entityGroupNodes.TryGetValue(folderPath, out var folderNode))
                {
                    parentNode!.Items.Add(folderNode = new TreeViewItem
                    {
                        Header = CreateHeader(groupFolders[i], "Folder.png")
                    });
                    
                    _entityGroupNodes.Add(folderPath, folderNode);
                }

                parentNode = folderNode;
            }
        }
        
        parentNode?.Items.Add(entityItem);
    }
    
    private void OnEntityRemoved(SegmentEntity entity)
    {
        // an entity has been removed, delete its tree node.
        if (_entityItems.Remove(entity, out var entityNode))
        {
            // find the parent node.
            if (entityNode.Parent is TreeViewItem parentNode)
                parentNode.Items.Remove(entityNode);
        }
    }

    private void EnsureEntitiesNode()
    {
        if (_entitiesNode is not null)
            return;
        
        _entitiesNode = new TreeViewItem
        {
            Tag = $"category:Entities",
            Header = CreateHeader("Entities", "Entities.png")
        };
            
        _entitiesNode.ContextMenu = new ContextMenu();
        _entitiesNode.ContextMenu.AddItem("Add Entity", "Add.png", (s, e) =>
        {
            var entity = new SegmentEntity
            {
                Name = $"Entity {_nextId++}"
            };
            
            var segment = Segment;
            if (segment is null)
                return;

            segment.Entities.Add(entity);
                
            // present the new entity to the user.
            entity.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });
    }
    
    private void OnTreasureAdded(SegmentTreasure treasure)
    {
        EnsureTreasuresNode();
        
        // a treasure has been added, create its tree node.
        if (!_treasureItems.TryGetValue(treasure, out var treasureItem))
        {
            var brush = (treasure is SegmentHoard) ? Brushes.Red : Brushes.Green;
            
            treasureItem = new SegmentTreeViewItem(treasure, brush, true)
            {
                Tag = treasure,
            };

            treasureItem.ContextMenu = new ContextMenu();
            treasureItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => RenameSegmentObject(treasure, treasureItem));
            treasureItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => DuplicateSegmentObject(treasure));
            treasureItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) =>
            {
                var segment = Segment;
                if (segment is null)
                    return;

                DeleteSegmentObject(treasure, treasureItem, segment.Treasures);
            });
            
            _treasureItems.Add(treasure, treasureItem);
        }
        
        if (!_treasureNode.Items.Contains(treasureItem))
            _treasureNode.Items.Add(treasureItem);
    }
    
    private void OnTreasureRemoved(SegmentTreasure treasure)
    {
        // a treasure has been removed, delete its tree node.
        if (_treasureItems.Remove(treasure, out var item))
        {
            if (_treasureNode != null)
                _treasureNode.Items.Remove(item);
        }
    }

    private void EnsureTreasuresNode()
    {
        if (_treasureNode is not null)
            return;
        
        _treasureNode = new TreeViewItem
        {
            Tag = $"category:Treasure",
            Header = CreateHeader("Treasure", "Treasures.png")
        };
            
        _treasureNode.ContextMenu  = new ContextMenu();
        _treasureNode.ContextMenu.AddItem("Add Treasures", "Add.png", (s, e) =>
        {
            var treasure = new SegmentTreasure
            {
                Name = $"Treasure {_nextId++}"
            };
            
            var segment = Segment;
            if (segment is null)
                return;

            segment.Treasures.Add(treasure);
                
            // present the new treasure to the user.
            treasure.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });
        _treasureNode.ContextMenu.AddItem("Add Hoard", "Add.png", (s, e) =>
        {
            var hoard = new SegmentHoard
            {
                Name = $"Hoard {_nextId++}"
            };
            
            var segment = Segment;
            if (segment is null)
                return;

            segment.Treasures.Add(hoard);
                
            // present the new hoard to the user.
            hoard.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });
    }

    private void OnSpawnAdded(SegmentSpawner segmentSpawner)
    {
        var segment = Segment;
        if (segment is null)
            return;

        EnsureSpawnersNode();

        // a spawner has been added, create its tree node.
        if (!_spawnItems.TryGetValue(segmentSpawner, out var spawnerItem))
        {
            var brush = segmentSpawner switch
            {
                LocationSegmentSpawner => Brushes.SkyBlue,
                RegionSegmentSpawner => Brushes.Orange,
                
                _ => Brushes.Gray
            };
            
            spawnerItem = new SegmentTreeViewItem(segmentSpawner, brush, true)
            {
                Tag = segmentSpawner,
            };

            spawnerItem.ContextMenu = new ContextMenu();
            spawnerItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => RenameSegmentObject(segmentSpawner, spawnerItem));
            spawnerItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => DuplicateSegmentObject(segmentSpawner));
            spawnerItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) =>
            {
                var segment = Segment;
                if (segment is null)
                    return;

                IList? source = segmentSpawner switch
                {
                    LocationSegmentSpawner => segment.Spawns.Location,
                    RegionSegmentSpawner => segment.Spawns.Region,

                    _ => null
                };

                if (source != null)
                    DeleteSegmentObject(segmentSpawner, spawnerItem, source);
            });
            
            _spawnItems.Add(segmentSpawner, spawnerItem);
        }
        
        var regionId = segmentSpawner switch
        {
            LocationSegmentSpawner locationSegmentSpawner => locationSegmentSpawner.Region,
            RegionSegmentSpawner regionSegmentSpawner => regionSegmentSpawner.Region,
            
            _ => -1
        };

        var region = segment.GetRegion(regionId);
        var parentNode = default(TreeViewItem);
        
        if (region is not null && !_spawnGroupNodes.TryGetValue(regionId, out parentNode))
        {
            parentNode = new TreeViewItem
            {
                Header = CreateHeader(region.Name, "Folder.png")
            };

            _spawnersNode.Items.Add(parentNode);
            _spawnGroupNodes.Add(regionId, parentNode);
        }
        
        parentNode ??= _spawnersNode;
        
        if (parentNode != null && !parentNode.Items.Contains(spawnerItem))
            parentNode.Items.Add(spawnerItem);
    }
    
    private void OnSpawnRemoved(SegmentSpawner segmentSpawner)
    {
        // a spawner has been removed, delete its tree node.
        if (_spawnItems.Remove(segmentSpawner, out var spawnNode))
        {
            if (spawnNode.Parent is TreeViewItem parent)
                parent.Items.Remove(spawnNode);
        }
    }
    
    private void EnsureSpawnersNode()
    {
        if (_spawnersNode is not null)
            return;
        
        _spawnersNode = new TreeViewItem
        {
            Tag = $"category:Spawns",
            Header = CreateHeader("Spawns", "Spawns.png")
        };
            
        _spawnersNode.ContextMenu = new ContextMenu();
        _spawnersNode.ContextMenu.AddItem("Add Location Spawner", "Add.png", (s, e) =>
        {
            var spawn = new LocationSegmentSpawner()
            {
                Name = $"Location Spawner {_nextId++}"
            };
            
            var segment = Segment;
            if (segment is null)
                return;

            segment.Spawns.Location.Add(spawn);
                
            // present the new treasure to the user.
            spawn.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });
        _spawnersNode.ContextMenu.AddItem("Add Region Spawner", "Add.png", (s, e) =>
        {
            var spawn = new RegionSegmentSpawner()
            {
                Name = $"Region Spawner {_nextId++}"
            };
            
            var segment = Segment;
            if (segment is null)
                return;

            segment.Spawns.Region.Add(spawn);
                
            // present the new treasure to the user.
            spawn.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());   
        });
    }
    
    private void Update()
    {
        _tree.Items.Clear();
        
        var segment = Segment;
        if (segment is null)
            return;

        EnsureSegmentNode();
        EnsureRegionsNode();
        EnsureLocationsNode();
        EnsureEntitiesNode();
        EnsureTreasuresNode();
        EnsureSpawnersNode();
        EnsureComponentsNode();
        EnsureBrushesNode();
        EnsureTemplatesNode();

        _tree.Items.Add(_segmentNode);

        _tree.Items.Add(_regionsNode);
        _tree.Items.Add(_locationsNode);
        _tree.Items.Add(_entitiesNode);
        _tree.Items.Add(_treasureNode);
        _tree.Items.Add(_spawnersNode);
        _tree.Items.Add(_componentsNode);
        _tree.Items.Add(_brushesNode);
        _tree.Items.Add(_templatesNode);

        foreach (var region in segment.Regions)
            OnRegionAdded(region);

        foreach (var subregion in segment.Subregions)
            OnSubregionAdded(subregion);

        foreach (var location in segment.Locations)
            OnLocationAdded(location);

        foreach (var component in segment.Components)
            OnComponentAdded(component);

        foreach (var brush in segment.Brushes)
            OnBrushAdded(brush);

        foreach (var template in segment.Templates)
            OnTemplateAdded(template);

        foreach (var entity in segment.Entities)
            OnEntityAdded(entity);

        foreach (var treasure in segment.Treasures)
            OnTreasureAdded(treasure);

        foreach (var regionSpawn in segment.Spawns.Region)
            OnSpawnAdded(regionSpawn);

        foreach (var locationSpawn in segment.Spawns.Location)
            OnSpawnAdded(locationSpawn);
    }
    
    private void OnItemSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        _currentSelection = e.NewValue as TreeViewItem;

        // Only send message if the selected item is a segment object.
        if (_currentSelection?.Tag is ISegmentObject segmentObject)
            WeakReferenceMessenger.Default.Send(new SegmentObjectSelected(segmentObject));
    }
    
    private StackPanel CreateHeader(string name, string icon)
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

        if (!String.IsNullOrEmpty(icon))
        {
            image.Source =
                new BitmapImage(new Uri($"pack://application:,,,/Kesmai.WorldForge;component/Resources/{icon}"));
        }

        panel.Children.Add(image);
        panel.Children.Add(new TextBlock { Text = name, FontSize = 12, VerticalAlignment = VerticalAlignment.Center });
        
        return panel;
    }
    
    private void RenameSegmentObject(ISegmentObject obj, SegmentTreeViewItem item)
    {
        item.Rename();
    }

    private void DuplicateSegmentObject(ISegmentObject obj)
    {
        if (Segment is null)
            return;

        obj.Copy(Segment);
    }

    private void DeleteSegmentObject(ISegmentObject obj, TreeViewItem item, IList collection)
    {
        collection.Remove(obj);
        
        if (item.Parent is ItemsControl parent)
            parent.Items.Remove(item);
    }
}

public class SegmentTreeViewItem : TreeViewItem
{
    private readonly ISegmentObject _segmentObject;
    
    public EditableTextBlock EditableTextBlock { get; }
    
    public SegmentTreeViewItem(ISegmentObject segmentObject, Brush brush, bool circleIcon)
    {
        _segmentObject = segmentObject ?? throw new ArgumentNullException(nameof(segmentObject));
        
        var innerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
        };
        
        Shape icon = circleIcon ? new Ellipse() : new Rectangle();
        
        icon.Width = 10;
        icon.Height = 10;
        icon.Fill = brush;
        icon.Margin = new Thickness(2, 0, 2, 0);
        
        innerPanel.Children.Add(icon);

        EditableTextBlock = new EditableTextBlock()
        {
            Text = segmentObject.Name,
        };
        EditableTextBlock.TextChanged += (s, e) => _segmentObject.Name = EditableTextBlock.Text;
            
        innerPanel.Children.Add(EditableTextBlock);
        
        Header = innerPanel;
        
        PreviewMouseDoubleClick += OnPreviewMouseDoubleClick;
    }

    private void OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs args)
    {
        if (args.OriginalSource is not DependencyObject originalSource)
            return;
        
        if (!ReferenceEquals(this, originalSource.FindAncestor<TreeViewItem>()))
            return;
        
        args.Handled = true;
        
        Rename();
    }

    public void Rename()
    {
        if (EditableTextBlock != null)
            EditableTextBlock.IsInEditMode = true;
    }
}
