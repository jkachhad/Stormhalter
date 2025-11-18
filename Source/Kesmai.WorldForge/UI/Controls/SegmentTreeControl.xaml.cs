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
    private EntitiesTreeViewItem _entitiesNode;
    private TreeViewItem _spawnersNode;
    private TreasuresTreeViewItem _treasureNode;

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
    
    private Dictionary<SegmentSpawner, SegmentTreeViewItem> _spawnItems = new Dictionary<SegmentSpawner, SegmentTreeViewItem>();
    
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
        _entitiesNode?.Dispose();
        _entitiesNode = null;
        _treasureNode?.Dispose();
        _treasureNode = null;
        _spawnersNode = null;
        _brushesNode?.Dispose();
        _brushesNode = null;
        _templatesNode?.Dispose();
        _templatesNode = null;
        
        _spawnItems.Clear();
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

    private void EnsureEntitiesNode()
    {
        if (_entitiesNode is not null)
            return;
        
        _entitiesNode = new EntitiesTreeViewItem(Segment, CreateHeader("Entities", "Entities.png"));
    }
    
    private void EnsureTreasuresNode()
    {
        if (_treasureNode is not null)
            return;
        
        _treasureNode = new TreasuresTreeViewItem(Segment, CreateHeader("Treasure", "Treasures.png"));
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
