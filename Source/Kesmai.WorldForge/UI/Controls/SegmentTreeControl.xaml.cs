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

namespace Kesmai.WorldForge.UI;

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

        WeakReferenceMessenger.Default.Register<SegmentEntityAdded>(this, (r, m) => OnEntityAdded(m.Value));
        WeakReferenceMessenger.Default.Register<SegmentEntityRemoved>(this, (r, m) => OnEntityRemoved(m.Value));
        WeakReferenceMessenger.Default.Register<SegmentEntityChanged>(this, (r, m) => OnEntityChanged(m.Value));
        
        WeakReferenceMessenger.Default.Register<SegmentTreasureAdded>(this, (r, m) => OnTreasureAdded(m.Value));
        WeakReferenceMessenger.Default.Register<SegmentTreasureRemoved>(this, (r, m) => OnTreasureRemoved(m.Value));
        WeakReferenceMessenger.Default.Register<SegmentTreasureChanged>(this, (r, m) => OnTreasureChanged(m.Value));
      
        WeakReferenceMessenger.Default.Register<SegmentSpawnAdded>(this, (r, m) => OnSpawnAdded(m.Value));
        WeakReferenceMessenger.Default.Register<SegmentSpawnRemoved>(this, (r, m) => OnSpawnRemoved(m.Value));
        WeakReferenceMessenger.Default.Register<SegmentSpawnChanged>(this, (r, m) => OnSpawnChanged(m.Value));
        
        _tree.SelectedItemChanged += OnItemSelected;
        _tree.KeyDown += OnKeyDown;
    }

    private static void OnSegmentChanged(DependencyObject control, DependencyPropertyChangedEventArgs args)
    {
        if (control is not SegmentTreeControl segmentTreeControl)
            return;
        
        if (args.OldValue is Segment oldSegment)
            oldSegment.PropertyChanged -= segmentTreeControl.OnSegmentPropertyChanged;

        segmentTreeControl.ResetNodes();
        
        if (args.NewValue is Segment newSegment)
            newSegment.PropertyChanged += segmentTreeControl.OnSegmentPropertyChanged;
        segmentTreeControl.Update();
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
    private BrushesTreeViewItem _brushesNode;
    private TemplatesTreeViewItem _templatesNode;
    private RegionsTreeViewItem _regionsNode;
    private LocationsTreeViewItem _locationsNode;
    private ComponentsTreeViewItem _componentsNode;
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
    
    private Dictionary<SegmentEntity, SegmentTreeViewItem> _entityItems = new Dictionary<SegmentEntity, SegmentTreeViewItem>();
    private Dictionary<SegmentTreasure, SegmentTreeViewItem> _treasureItems = new Dictionary<SegmentTreasure, SegmentTreeViewItem>();
    private Dictionary<SegmentSpawner, SegmentTreeViewItem> _spawnItems = new Dictionary<SegmentSpawner, SegmentTreeViewItem>();
    
    private Dictionary<string, TreeViewItem> _entityGroupNodes = new Dictionary<string, TreeViewItem>();
    private Dictionary<int, TreeViewItem> _spawnGroupNodes = new Dictionary<int, TreeViewItem>();

    private void ResetNodes()
    {
        _segmentNode = null;
        _regionsNode?.Dispose();
        _regionsNode = null;
        _locationsNode?.Dispose();
        _locationsNode = null;
        _componentsNode?.Dispose();
        _componentsNode = null;
        _entitiesNode = null;
        _treasureNode = null;
        _spawnersNode = null;
        _brushesNode?.Dispose();
        _brushesNode = null;
        _templatesNode?.Dispose();
        _templatesNode = null;
        
        _entityItems.Clear();
        _treasureItems.Clear();
        _spawnItems.Clear();
        _entityGroupNodes.Clear();
        _spawnGroupNodes.Clear();
    }
    
    private void UpdateTreeItemText<T>(Dictionary<T, SegmentTreeViewItem> lookup, T segmentObject, string format = "") where T : class, ISegmentObject
    {
        if (segmentObject is null)
            return;

        if (lookup.TryGetValue(segmentObject, out var item))
        {
            item.EditableTextBlock.TextFormat = format;
            item.EditableTextBlock.Text = segmentObject.Name;
        }
    }
    private void EnsureLocationsNode()
    {
        if (_locationsNode is not null || Segment is null)
            return;
        
        _locationsNode = new LocationsTreeViewItem(Segment, CreateHeader("Locations", "Locations.png"));
    }
    
    private void EnsureRegionsNode()
    {
        if (_regionsNode is not null || Segment is null)
            return;
        
        _regionsNode = new RegionsTreeViewItem(Segment, CreateHeader("Regions", "Regions.png"));
    }

    private void EnsureComponentsNode()
    {
        if (_componentsNode is not null)
            return;
        
        _componentsNode = new ComponentsTreeViewItem(Segment, CreateHeader("Components", "Terrain.png"));
    }
    
    private void EnsureBrushesNode()
    {
        if (_brushesNode is not null)
            return;

        _brushesNode = new BrushesTreeViewItem(Segment, CreateHeader("Brushes", "Editor-Icon-Paint.png"));
    }
    
    private void EnsureTemplatesNode()
    {
        if (_templatesNode is not null)
            return;
        
        _templatesNode = new TemplatesTreeViewItem(Segment, CreateHeader("Templates", "Gear.png"));
    }

    private void OnEntityAdded(SegmentEntity entity)
    {
        EnsureEntitiesNode();
        
        if (!_entityItems.TryGetValue(entity, out var entityItem))
        {
            entityItem = new SegmentTreeViewItem(entity, Brushes.Yellow, true)
            {
                Tag = entity,
            };

            entityItem.ContextMenu = new ContextMenu();
            entityItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => RenameSegmentObject(entity, entityItem));
            entityItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => DuplicateSegmentObject(entity));
            entityItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => DeleteSegmentObject(entity, entityItem, Segment.Entities));
            
            _entityItems.Add(entity, entityItem);
        }
        
        BindEntityToGroup(entity, entityItem);
    }
    
    private void OnEntityChanged(SegmentEntity entity)
    {
        if (!_entityItems.TryGetValue(entity, out var entityItem))
            return;

        entityItem.EditableTextBlock.Text = entity.Name;
        
        BindEntityToGroup(entity, entityItem);
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

    private void BindEntityToGroup(SegmentEntity entity, SegmentTreeViewItem entityItem)
    {
        EnsureEntitiesNode();

        var groupPath = String.IsNullOrEmpty(entity.Group) ? "Ungrouped" : entity.Group;
        
        if (!_entityGroupNodes.TryGetValue(groupPath, out var parentNode))
        {
            parentNode = _entitiesNode;
            
            var groupFolders = groupPath.Split(@"\");

            for (int i = 0; i < groupFolders.Length; i++)
            {
                var folderPath = String.Join(@"\", groupFolders.Take(i + 1));

                if (!_entityGroupNodes.TryGetValue(folderPath, out var folderNode))
                {
                    parentNode.Items.Add(folderNode = new TreeViewItem
                    {
                        Header = CreateHeader(groupFolders[i], "Folder.png")
                    });
                    
                    folderNode.ContextMenu = new ContextMenu();
                    folderNode.ContextMenu.AddItem("Add Entity", "Add.png", (s, e) =>
                    {
                        var newEntity = new SegmentEntity
                        {
                            Name = $"Entity {_nextId++}",
                            Group = folderPath
                        };
            
                        Segment.Entities.Add(newEntity);
                
                        newEntity.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
                    });
                    
                    _entityGroupNodes.Add(folderPath, folderNode);
                }

                parentNode = folderNode;
            }
		}
        
        if (entityItem.Parent is ItemsControl currentParent && !ReferenceEquals(currentParent, parentNode))
        {
            // If the entity already belongs to a different folder we must detach it before
            // re-attaching, otherwise WPF throws "element already has a logical parent".
            currentParent.Items.Remove(entityItem);
        }

        if (!parentNode.Items.Contains(entityItem))
            parentNode.Items.Add(entityItem);
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
            
            Segment.Entities.Add(entity);
                
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
            treasureItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => DeleteSegmentObject(treasure, treasureItem, Segment.Treasures));
            
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

    private void OnTreasureChanged(SegmentTreasure treasure)
    {
        UpdateTreeItemText(_treasureItems, treasure);
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
            
            Segment.Treasures.Add(treasure);
                
            // present the new treasure to the user.
            treasure.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });
        _treasureNode.ContextMenu.AddItem("Add Hoard", "Add.png", (s, e) =>
        {
            var hoard = new SegmentHoard
            {
                Name = $"Hoard {_nextId++}"
            };
            
            Segment.Treasures.Add(hoard);
                
            // present the new hoard to the user.
            hoard.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });
    }

    private void OnSpawnAdded(SegmentSpawner segmentSpawner)
    {
        EnsureSpawnersNode();
        
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
                IList source = segmentSpawner switch
                {
                    LocationSegmentSpawner => Segment.Spawns.Location,
                    RegionSegmentSpawner => Segment.Spawns.Region,
                    
                    _ => null
                };

                if (source != null)
                    DeleteSegmentObject(segmentSpawner, spawnerItem, source);
            });
            
            _spawnItems.Add(segmentSpawner, spawnerItem);
        }
        
        BindSpawnToGroup(segmentSpawner, spawnerItem);
    }

    private void OnSpawnChanged(SegmentSpawner segmentSpawner)
    {
        if (!_spawnItems.TryGetValue(segmentSpawner, out var spawnerItem))
            return;

        spawnerItem.EditableTextBlock.Text = segmentSpawner.Name;
        
        BindSpawnToGroup(segmentSpawner, spawnerItem);
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

    private void BindSpawnToGroup(SegmentSpawner segmentSpawner, SegmentTreeViewItem spawnerItem)
    {
        EnsureSpawnersNode();

        var regionId = segmentSpawner switch
        {
            LocationSegmentSpawner locationSegmentSpawner => locationSegmentSpawner.Region,
            RegionSegmentSpawner regionSegmentSpawner => regionSegmentSpawner.Region,
            
            _ => -1
        };

        var region = Segment.GetRegion(regionId);
        var parentNode = default(TreeViewItem);

        if (region is not null)
        {
            if (!_spawnGroupNodes.TryGetValue(regionId, out parentNode))
            {
                parentNode = new TreeViewItem
                {
                    Header = CreateHeader($"[{region.ID}] {region.Name}", "Folder.png")
                };

                parentNode.ContextMenu = new ContextMenu();
                parentNode.ContextMenu.AddItem("Add Location Spawner", "Add.png", (s, e) =>
                {
                    var spawn = new LocationSegmentSpawner()
                    {
                        Name = $"Location Spawner {_nextId++}",
                        Region = regionId
                    };

                    Segment.Spawns.Location.Add(spawn);

                    // present the new treasure to the user.
                    spawn.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
                });
                parentNode.ContextMenu.AddItem("Add Region Spawner", "Add.png", (s, e) =>
                {
                    var spawn = new RegionSegmentSpawner()
                    {
                        Name = $"Region Spawner {_nextId++}",
                        Region = regionId
                    };

                    Segment.Spawns.Region.Add(spawn);

                    // present the new treasure to the user.
                    spawn.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
                });

                _spawnersNode.Items.Add(parentNode);
                _spawnGroupNodes.Add(regionId, parentNode);
            }
            else
            {
                parentNode = _spawnGroupNodes[regionId];
            }
        }
        
        parentNode ??= _spawnersNode;

        if (spawnerItem.Parent is ItemsControl currentParent && !ReferenceEquals(currentParent, parentNode))
        {
            // If the entity already belongs to a different folder we must detach it before
            // re-attaching, otherwise WPF throws "element already has a logical parent".
            currentParent.Items.Remove(spawnerItem);
        }

        if (!parentNode.Items.Contains(spawnerItem))
            parentNode.Items.Add(spawnerItem);
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
            
            Segment.Spawns.Location.Add(spawn);
                
            // present the new treasure to the user.
            spawn.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });
        _spawnersNode.ContextMenu.AddItem("Add Region Spawner", "Add.png", (s, e) =>
        {
            var spawn = new RegionSegmentSpawner()
            {
                Name = $"Region Spawner {_nextId++}"
            };
            
            Segment.Spawns.Region.Add(spawn);
                
            // present the new treasure to the user.
            spawn.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());   
        });
    }
    
    private void Update()
    {
        _tree.Items.Clear();
        
        if (Segment is null)
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
    }
    
    private void OnItemSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        // Only send message if the selected item is a segment object.
        if (e.NewValue is TreeViewItem { Tag: ISegmentObject segmentObject })
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

    public SegmentTreeViewItem(ISegmentObject segmentObject, Brush brush, bool circleIcon, string displayFormat = "{0}")
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
            TextFormat = displayFormat,
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
