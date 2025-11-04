using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using Path = System.IO.Path;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.VisualBasic;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.UI.Controls;

namespace Kesmai.WorldForge.UI;

public class SegmentObjectDoubleClick(ISegmentObject target) : ValueChangedMessage<ISegmentObject>(target);
public class SegmentObjectSelected(ISegmentObject target) : ValueChangedMessage<ISegmentObject>(target);

public partial class SegmentTreeControl : UserControl
{
    private static int _nextId;
    
    public static readonly DependencyProperty SegmentProperty =
        DependencyProperty.Register(nameof(Segment), typeof(Segment), typeof(SegmentTreeControl),
            new PropertyMetadata(null, OnSegmentChanged));

    public Segment Segment
    {
        get => (Segment)GetValue(SegmentProperty);
        set => SetValue(SegmentProperty, value);
    }
    
    private ISegmentObject _copyObject;
    
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
        
        /*
        WeakReferenceMessenger.Default.Register<SegmentEntitiesChanged>(this, (r, m) => UpdateEntities());
        WeakReferenceMessenger.Default.Register<SegmentEntityChanged>(this, (r, m) => UpdateEntities());

        WeakReferenceMessenger.Default.Register<SegmentTreasuresChanged>(this, (r, m) => UpdateTreasures());
        WeakReferenceMessenger.Default.Register<SegmentTreasureChanged>(this, (r, m) => UpdateTreasures());

        WeakReferenceMessenger.Default.Register<SegmentSpawnsChanged>(this, (r, m) => UpdateSpawns());
        WeakReferenceMessenger.Default.Register<SegmentSpawnChanged>(this, (r, m) => UpdateSpawns());
        */
        
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

    private TreeViewItem _segmentNode;
    private TreeViewItem _brushesNode;
    private TreeViewItem _templatesNode;
    private TreeViewItem _regionsNode;
    private TreeViewItem _locationsNode;
    private TreeViewItem _componentsNode;
    private TreeViewItem _entitiesNode;
    private TreeViewItem _spawnersNode;
    private TreeViewItem _treasureNode;

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
    
    private Dictionary<SegmentRegion, SegmentTreeViewItem> _regionItems = new Dictionary<SegmentRegion, SegmentTreeViewItem>();
    private Dictionary<SegmentSubregion, SegmentTreeViewItem> _subregionItems = new Dictionary<SegmentSubregion, SegmentTreeViewItem>();
    private Dictionary<SegmentLocation, SegmentTreeViewItem> _locationItems = new Dictionary<SegmentLocation, SegmentTreeViewItem>();
    private Dictionary<SegmentComponent, SegmentTreeViewItem> _componentItems = new Dictionary<SegmentComponent, SegmentTreeViewItem>();
    private Dictionary<SegmentBrush, SegmentTreeViewItem> _brushItems = new Dictionary<SegmentBrush, SegmentTreeViewItem>();
    private Dictionary<SegmentTemplate, SegmentTreeViewItem> _templateItems = new Dictionary<SegmentTemplate, SegmentTreeViewItem>();
    
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
            regionItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => DeleteSegmentObject(region, regionItem, Segment.Regions));
            
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
            subregionItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => DeleteSegmentObject(subregion, subregionItem, Segment.Subregions));

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
                locationItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => DeleteSegmentObject(location, locationItem, Segment.Locations));

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
            
            Segment.Locations.Add(location);
                
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
            
            Segment.Regions.Add(region);
                
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
            componentItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => DeleteSegmentObject(component, componentItem, Segment.Components));
            
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
            brushItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => DeleteSegmentObject(brush, brushItem, Segment.Brushes));
            
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
            
            Segment.Components.Add(component);
            
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

            Segment.Brushes.Add(brush);

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
            templateItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => DeleteSegmentObject(template, templateItem, Segment.Templates));
            
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
            
            Segment.Templates.Add(template);
                
            // present the new template to the user.
            template.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });
    }
    
    public void UpdateSpawns()
    {
        if (Segment is null)
            return;

        var collection = Segment.Spawns;
        
        if (_spawnersNode is null)
        {
            _spawnersNode = new TreeViewItem
            {
                Tag = $"category:Spawns",
                Header = CreateHeader("Spawns", "Spawns.png")
            };
            
            _spawnersNode.ContextMenu = new ContextMenu();
            _spawnersNode.ContextMenu.AddItem("Add Location Spawner", "Add.png", (s, e) => AddSpawner(collection.Location, "Location Spawner", 0));
            _spawnersNode.ContextMenu.AddItem("Add Region Spawner", "Add.png", (s, e) => AddSpawner(collection.Region, "Region Spawner", 0));
        }

        var expanded = new HashSet<string>();
        
        foreach (var item in _spawnersNode.Items.OfType<TreeViewItem>())
            SaveExpansionState(item, expanded);
        
        _spawnersNode.Items.Clear();
        
        foreach (var region in Segment.Regions)
            _spawnersNode.Items.Add(createSpawnerRegionNode(region, collection));
        
        foreach (var item in _spawnersNode.Items.OfType<TreeViewItem>())
            RestoreExpansionState(item, expanded);
        
        TreeViewItem createSpawnerRegionNode(SegmentRegion region, SegmentSpawns source)
        {
            var item = new TreeViewItem
            {
                Tag = $"category:Spawn/{region.ID}"
            };
            item.Header = CreateColoredHeader(region.Name, Brushes.MediumPurple, false);

            foreach (var spawner in source.Location.Where(s => GetRegionId(s) == region.ID))
                item.Items.Add(createSpawnerNode(spawner, source.Location));

            foreach (var spawner in source.Region.Where(s => GetRegionId(s) == region.ID))
                item.Items.Add(createSpawnerNode(spawner, source.Region));

            item.ContextMenu = new ContextMenu();
            item.ContextMenu.AddItem("Add Location Spawner", "Add.png", (s, e) =>
            {
                var spawner = addSpawn(source.Location, "Location Spawner");

                if (spawner != null)
                {
                    spawner.Region = region.ID;
                    spawner.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
                }
            });
            item.ContextMenu.AddItem("Add Region Spawner", "Add.png", (s, e) =>
            {
                var spawner = addSpawn(source.Region, "Region Spawner");

                if (spawner != null)
                {
                    spawner.Region = region.ID;
                    spawner.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
                }
            });
            
            return item;
        }
        
        TreeViewItem createSpawnerNode(SegmentSpawner segmentSpawner, IList source)
        {
            var brush = segmentSpawner switch
            {
                LocationSegmentSpawner => Brushes.SkyBlue,
                RegionSegmentSpawner => Brushes.Orange,
                
                _ => Brushes.Gray
            };
            
            var item = new SegmentTreeViewItem(segmentSpawner, brush, true)
            {
                Tag = segmentSpawner 
            };
            
            item.ContextMenu = new ContextMenu();
            item.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => RenameSegmentObject(segmentSpawner, item));
            item.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => DuplicateSegmentObject(segmentSpawner));
            item.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => DeleteSegmentObject(segmentSpawner, item, source));
            
            return item;
        }
        
        T addSpawn<T>(IList<T> source, string typeName) where T : ISegmentObject, new()
        {
            var obj = new T
            {
                Name = $"{typeName} {_nextId++}"
            };
            
            source.Add(obj);
            
            return obj;
        }
    }

    public void UpdateEntities()
    {
        if (Segment is null)
            return;

        var collection = Segment.Entities.GroupBy(e => e.Group)
            .OrderBy(g => g.Key);
        
        if (_entitiesNode is null)
        {
            _entitiesNode = new TreeViewItem
            {
                Tag = $"category:Entity",
                Header = CreateHeader("Entity", "Entities.png")
            };
            
            _entitiesNode.ContextMenu = new ContextMenu();
            _entitiesNode.ContextMenu.AddItem("Add Entity", "Add.png", (s, e) =>
            {
                var entity = addEntity(Segment.Entities, String.Empty, "Entity");
                
                // present the new entity to the user.
                if (entity != null)
                    entity.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
            });
        }

        var expanded = new HashSet<string>();
        
        foreach (var item in _entitiesNode.Items.OfType<TreeViewItem>())
            SaveExpansionState(item, expanded);
        
        _entitiesNode.Items.Clear();

        foreach (var grouping in collection)
            createEntityGroupNode(grouping.Key, grouping, Segment.Entities);
        
        foreach (var item in _entitiesNode.Items.OfType<TreeViewItem>())
            RestoreExpansionState(item, expanded);
        
        TreeViewItem createEntityGroupNode(string groupPath, IEnumerable<SegmentEntity> grouping, IList<SegmentEntity> source)
        {
            // separate the groupPath into its components
            var groupFolders = String.IsNullOrEmpty(groupPath) ? ["Ungrouped"] : groupPath.Split(@"\");
            
            // for each folder in the path, find or create the corresponding tree view node.
            var parentFolder = _entitiesNode;
            
            foreach (var folder in groupFolders)
            {
                // find the folder node if it already exists.
                var folderNode = parentFolder.Items.OfType<TreeViewItem>().FirstOrDefault(n => n.Tag is string path 
                    && path.EndsWith($"/{folder}", StringComparison.OrdinalIgnoreCase));
    
                // if it doesn't exist, create it.
                if (folderNode is null)
                {
                    folderNode = new TreeViewItem
                    {
                        Tag = $"{parentFolder.Tag}/{folder}",
                        Header = CreateHeader(folder, "Folder.png")
                    };
                }
    
                // add it to the parent if it doesn't already exist.
                parentFolder.Items.Add(folderNode);
                
                // move to the next child folder.
                parentFolder = folderNode;
            }
            
            foreach (var entity in grouping)
                parentFolder.Items.Add(createEntityEntryNode(entity));
    
            parentFolder.ContextMenu = new ContextMenu();
            parentFolder.ContextMenu.AddItem("Add Entity", "Add.png", (s, e) =>
            {
                var entity = addEntity(source, groupPath, "Entity");
                
                // present the new entity to the user.
                if (entity != null)
                    entity.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
            });
            
            return parentFolder;
        }
    
        TreeViewItem createEntityEntryNode(SegmentEntity segmentEntity)
        {
            var item = new SegmentTreeViewItem(segmentEntity, Brushes.Yellow, true)
            {
                Tag = segmentEntity 
            };
            
            item.ContextMenu = new ContextMenu();
            item.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => RenameSegmentObject(segmentEntity, item));
            item.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => DuplicateSegmentObject(segmentEntity));
            item.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => DeleteSegmentObject(segmentEntity, item, Segment.Entities));

            return item;
        }
        
        SegmentEntity addEntity(IList<SegmentEntity> source, string group, string typeName)
        {
            var obj = new SegmentEntity
            {
                Name = $"{typeName} {_nextId++}",
                Group = group 
            };
            
            source.Add(obj);
            
            return obj;
        }
    }

    public void UpdateTreasures()
    {
        if (Segment is null)
            return;

        var collection = Segment.Treasures;
        
        if (_treasureNode is null)
        {
            _treasureNode = new TreeViewItem
            {
                Tag = $"category:Treasure",
                Header = CreateHeader("Treasure", "Treasures.png")
            };
            
            _treasureNode.ContextMenu  = new ContextMenu();
            _treasureNode.ContextMenu.AddItem("Add Treasures", "Add.png", (s, e) =>
            {
                var treasure = addTreasure(collection, "Treasure");
                
                // present the new treasure to the user.
                if (treasure != null)
                    treasure.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
            });
            _treasureNode.ContextMenu.AddItem("Add Hoard", "Add.png", (s, e) =>
            {
                var hoard = addTreasure(collection, "Hoard");
                
                // present the new hoard to the user.
                if (hoard != null)
                    hoard.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
            });
        }

        var expanded = new HashSet<string>();
        
        foreach (var item in _treasureNode.Items.OfType<TreeViewItem>())
            SaveExpansionState(item, expanded);
        
        _treasureNode.Items.Clear();
        
        foreach (var child in collection)
            _treasureNode.Items.Add(createTreasureEntryNode(child));
        
        foreach (var item in _treasureNode.Items.OfType<TreeViewItem>())
            RestoreExpansionState(item, expanded);
        
        TreeViewItem createTreasureEntryNode(SegmentTreasure treasure)
        {
            var brush = (treasure is SegmentHoard) ? Brushes.Red : Brushes.Green;
            
            var item = new SegmentTreeViewItem(treasure, brush, false)
            {
                Tag = treasure 
            };
            
            item.ContextMenu = new ContextMenu();
            item.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => RenameSegmentObject(treasure, item));
            item.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => DuplicateSegmentObject(treasure));
            item.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => DeleteSegmentObject(treasure, item, collection));
            
            return item;
        }
        
        T addTreasure<T>(IList<T> source, string typeName) where T : ISegmentObject, new()
        {
            var obj = new T
            {
                Name = $"{typeName} {_nextId++}"
            };
            
            source.Add(obj);
            
            return obj;
        }
    }
    
    private void Update()
    {
        var expanded = new HashSet<string>();
        
        foreach (var item in _tree.Items.OfType<TreeViewItem>())
            SaveExpansionState(item, expanded);

        _tree.Items.Clear();
        
        if (Segment is null)
            return;

        var rootPath = Segment.Directory;
        
        EnsureSegmentNode();
        EnsureRegionsNode();
        EnsureLocationsNode();
        UpdateEntities();
        UpdateTreasures();
        UpdateSpawns();
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

        // Add Source directory last
        if (!string.IsNullOrEmpty(rootPath) && Directory.Exists(rootPath))
        {
            var sourcePath = Path.Combine(rootPath, "Source");
            Directory.CreateDirectory(sourcePath);
            var sourceItem = CreateDirectoryNode(new DirectoryInfo(sourcePath));

            /*foreach (var file in Segment.VirtualFiles)
                sourceItem.Items.Add(CreateVirtualFileNode(file));*/

            _tree.Items.Add(sourceItem);
        }

        foreach (var item in _tree.Items.OfType<TreeViewItem>())
            RestoreExpansionState(item, expanded);
    }

    private static void SaveExpansionState(TreeViewItem item, HashSet<string> expanded)
    {
        if (item.IsExpanded)
        {
            if (item.Tag is ISegmentObject segmentObject)
                expanded.Add(segmentObject.Name);
            else if (item.Tag is string path)
                expanded.Add(path);
        }
        foreach (var child in item.Items.OfType<TreeViewItem>())
            SaveExpansionState(child, expanded);
    }

    private static void RestoreExpansionState(TreeViewItem item, HashSet<string> expanded)
    {
        if (item.Tag is ISegmentObject segmentObject && expanded.Contains(segmentObject.Name))
            item.IsExpanded = true;
        else if (item.Tag is string path && expanded.Contains(path))
            item.IsExpanded = true;
        
        foreach (var child in item.Items.OfType<TreeViewItem>())
            RestoreExpansionState(child, expanded);
    }
    
    private void OnItemSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        // Only send message if the selected item is a segment object.
        if (e.NewValue is TreeViewItem { Tag: ISegmentObject segmentObject })
            WeakReferenceMessenger.Default.Send(new SegmentObjectSelected(segmentObject));
    }

    private void OnDoubleClick(object sender, MouseButtonEventArgs args)
    {
        if (sender is TreeViewItem item)
        {
            item.IsSelected = true;

            if (item.Tag is ISegmentObject obj)
            {
                WeakReferenceMessenger.Default.Send(new SegmentObjectDoubleClick(obj));
                WeakReferenceMessenger.Default.Send(new SegmentObjectSelected(obj));
            }
        }
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

    private static StackPanel CreateColoredHeader(string text, Brush brush, bool isCircle)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        Shape icon = isCircle ? new Ellipse() : new Rectangle();
        icon.Width = 10;
        icon.Height = 10;
        icon.Fill = brush;
        icon.Margin = new Thickness(2, 0, 2, 0);
        panel.Children.Add(icon);
        panel.Children.Add(new TextBlock { Text = text });
        return panel;
    }

    private TreeViewItem CreateDirectoryNode(DirectoryInfo dir)
    {
        var item = new TreeViewItem { Tag = dir.FullName };
        item.Header = CreateHeader(dir.Name, "Folder.png");

        foreach (var subDir in dir.GetDirectories())
            item.Items.Add(CreateDirectoryNode(subDir));

        foreach (var file in dir.GetFiles("*.cs"))
            item.Items.Add(CreateFileNode(file));

        var menu = new ContextMenu();
        var addFile = new MenuItem { Header = "Add File" };
        addFile.Click += (s, e) => AddFile(dir.FullName, item);
        var addFolder = new MenuItem { Header = "Add Folder" };
        addFolder.Click += (s, e) => AddFolder(dir.FullName, item);
        var rename = new MenuItem { Header = "Rename" };
        rename.Click += (s, e) => Rename(dir.FullName, item, true);
        var delete = new MenuItem { Header = "Delete" };
        delete.Click += (s, e) => Delete(dir.FullName, item, true);

        menu.Items.Add(addFile);
        menu.Items.Add(addFolder);
        menu.Items.Add(rename);
        menu.Items.Add(delete);
        item.ContextMenu = menu;

        return item;
    }

    private TreeViewItem CreateFileNode(FileInfo file)
    {
        var item = new TreeViewItem { Tag = file.FullName };
        item.Header = CreateHeader(file.Name, "Folder.png");

        var menu = new ContextMenu();
        var rename = new MenuItem { Header = "Rename" };
        rename.Click += (s, e) => Rename(file.FullName, item, false);
        var delete = new MenuItem { Header = "Delete" };
        delete.Click += (s, e) => Delete(file.FullName, item, false);

        menu.Items.Add(rename);
        menu.Items.Add(delete);
        item.ContextMenu = menu;

        return item;
    }

    /*
    private TreeViewItem CreateVirtualFileNode(VirtualFile file) =>
        CreateInMemoryNode(file, file.Name + ".cs");
        */

    private void AddSpawner<T>(IList<T> collection, string typeName, int regionId) where T : SegmentSpawner, new()
    {
        var defaultName = $"{typeName} {collection.Count + 1}";
        var name = Interaction.InputBox("Name", $"Add {typeName}", defaultName);
        if (string.IsNullOrWhiteSpace(name))
            return;
        var spawner = new T { Name = name };
        dynamic dyn = spawner;
        dyn.Region = regionId;
        collection.Add(spawner);
    }

    private static int GetRegionId(SegmentSpawner segmentSpawner) => segmentSpawner switch
    {
        LocationSegmentSpawner ls => ls.Region,
        RegionSegmentSpawner rs => rs.Region,
        _ => 0
    };

    private TreeViewItem CreateInMemoryNode(ISegmentObject obj, string displayName)
    {
        var item = new TreeViewItem { Tag = obj };
        var header = CreateHeader(displayName, "Folder.png");
        item.ContextMenu = null;
        if (header.Children.Count > 1 && header.Children[1] is TextBlock text)
        {
            text.Foreground = Brushes.LightGray;
            text.FontWeight = FontWeights.Bold;
        }
        item.Header = header;
        return item;
    }

    private void AddFile(string directory, TreeViewItem parent)
    {
        var name = Interaction.InputBox("File name", "Add File", "NewFile.cs");
        if (string.IsNullOrWhiteSpace(name))
            return;
        if (Path.GetExtension(name) != ".cs")
            name += ".cs";
        var path = Path.Combine(directory, name);
        File.Create(path).Close();
        parent.Items.Add(CreateFileNode(new FileInfo(path)));
    }

    private void AddFolder(string directory, TreeViewItem parent)
    {
        var name = Interaction.InputBox("Folder name", "Add Folder", "NewFolder");
        if (string.IsNullOrWhiteSpace(name))
            return;
        var path = Path.Combine(directory, name);
        Directory.CreateDirectory(path);
        parent.Items.Add(CreateDirectoryNode(new DirectoryInfo(path)));
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

    private void Rename(string path, TreeViewItem item, bool isDirectory)
    {
        var name = Interaction.InputBox("New name", "Rename", Path.GetFileName(path));
        if (string.IsNullOrWhiteSpace(name))
            return;
        if (!isDirectory && Path.GetExtension(name) != ".cs")
            name += ".cs";
        var newPath = Path.Combine(Path.GetDirectoryName(path)!, name);
        if (isDirectory)
            Directory.Move(path, newPath);
        else
            File.Move(path, newPath);
        item.Tag = newPath;
        if (item.Header is StackPanel panel)
        {
            if (panel.Children[0] is Image img)
                img.Source = GetIcon(newPath, isDirectory);
            if (panel.Children[1] is TextBlock text)
                text.Text = name;
        }
    }

    private void Delete(string path, TreeViewItem item, bool isDirectory)
    {
        if (isDirectory)
            Directory.Delete(path, true);
        else
            File.Delete(path);
        if (item.Parent is ItemsControl parent)
            parent.Items.Remove(item);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
        out SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private const uint SHGFI_ICON = 0x000000100;
    private const uint SHGFI_SMALLICON = 0x000000001;
    private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
    private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

    private static ImageSource? GetIcon(string path, bool isDirectory)
    {
        var flags = SHGFI_ICON | SHGFI_SMALLICON;
        var attributes = isDirectory ? FILE_ATTRIBUTE_DIRECTORY : 0u;
        if (!File.Exists(path) && !Directory.Exists(path))
            flags |= SHGFI_USEFILEATTRIBUTES;
        SHGetFileInfo(path, attributes, out var info, (uint)Marshal.SizeOf<SHFILEINFO>(), flags);
        if (info.hIcon == IntPtr.Zero)
            return null;
        try
        {
            return Imaging.CreateBitmapSourceFromHIcon(info.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
        finally
        {
            DestroyIcon(info.hIcon);
        }
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
