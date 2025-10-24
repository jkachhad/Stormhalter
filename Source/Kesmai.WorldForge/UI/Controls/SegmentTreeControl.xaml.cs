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
    public static readonly DependencyProperty SegmentProperty =
        DependencyProperty.Register(nameof(Segment), typeof(Segment), typeof(SegmentTreeControl),
            new PropertyMetadata(null, OnSegmentChanged));

    public Segment Segment
    {
        get => (Segment?)GetValue(SegmentProperty);
        set => SetValue(SegmentProperty, value);
    }

    private ISegmentObject _copyObject;
    
    public SegmentTreeControl()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<SegmentRegionsChanged>(this, (r, m) => UpdateRegions());
        WeakReferenceMessenger.Default.Register<SegmentRegionChanged>(this, (r, m) => UpdateRegions());

        WeakReferenceMessenger.Default.Register<SegmentSubregionsChanged>(this, (r, m) => UpdateRegions());
        WeakReferenceMessenger.Default.Register<SegmentSubregionChanged>(this, (r, m) => UpdateRegions());
        
        WeakReferenceMessenger.Default.Register<SegmentLocationsChanged>(this, (r, m) => UpdateLocations());
        WeakReferenceMessenger.Default.Register<SegmentLocationChanged>(this, (r, m) => UpdateLocations());
        
        WeakReferenceMessenger.Default.Register<SegmentEntitiesChanged>(this, (r, m) => UpdateEntities());
        WeakReferenceMessenger.Default.Register<SegmentEntityChanged>(this, (r, m) => UpdateEntities());
        
        WeakReferenceMessenger.Default.Register<SegmentComponentsChanged>(this, (r, m) => UpdateComponents());
        WeakReferenceMessenger.Default.Register<SegmentComponentCreated>(this, (r, m) => UpdateComponents());
        WeakReferenceMessenger.Default.Register<SegmentComponentDeleted>(this, (r, m) => UpdateComponents());
        
        WeakReferenceMessenger.Default.Register<SegmentTreasuresChanged>(this, (r, m) => UpdateTreasures());
        WeakReferenceMessenger.Default.Register<SegmentTreasureChanged>(this, (r, m) => UpdateTreasures());
        
        WeakReferenceMessenger.Default.Register<SegmentSpawnsChanged>(this, (r, m) => UpdateSpawns());
        WeakReferenceMessenger.Default.Register<SegmentSpawnChanged>(this, (r, m) => UpdateSpawns());
        
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
    private TreeViewItem _regionsNode;
    private TreeViewItem _locationsNode;
    private TreeViewItem _componentsNode;
    private TreeViewItem _entitiesNode;
    private TreeViewItem _spawnersNode;
    private TreeViewItem _treasureNode;

    public void UpdateSegment()
    {
        if (Segment is null)
            return;
        
        if (_segmentNode is null)
        {
            _segmentNode = new TreeViewItem
            {
                Tag = Segment,
                Header = CreateHeader("Segment", "Segment.png"),
            };
        }
    }
    
    public void UpdateRegions()
    {
        if (Segment is null)
            return;
        
        var collection = Segment.Regions;

        if (_regionsNode is null)
        {
            _regionsNode = new TreeViewItem
            {
                Tag = $"category:Regions",
                Header = CreateHeader("Regions", "Regions.png")
            };
            
            _regionsNode.ContextMenu = new ContextMenu();
            _regionsNode.ContextMenu.AddItem("Add Region", "Add.png", (s, e) =>
            {
                var region = AddSegmentObject(collection, "Region");

                // present the new region to the user.
                if (region != null)
                    region.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
            });
        }
        
        var expanded = new HashSet<string>();
        
        foreach (var item in _regionsNode.Items.OfType<TreeViewItem>())
            SaveExpansionState(item, expanded);
        
        _regionsNode.Items.Clear();

        foreach (var child in collection)
            _regionsNode.Items.Add(createRegionNode(child, collection));
        
        foreach (var item in _regionsNode.Items.OfType<TreeViewItem>())
            RestoreExpansionState(item, expanded);
        TreeViewItem createRegionNode(SegmentRegion region, SegmentRegions source)
        {
            var item = new SegmentTreeViewItem(region, Brushes.MediumPurple, false)
            {
                Tag = region,
            };

            if (Segment is not null)
            {
                foreach (var subregion in Segment.Subregions.Where(sr => sr.Region == region.ID))
                    item.Items.Add(createSubregionNode(subregion, Segment.Subregions));
            }

            item.ContextMenu = new ContextMenu();
            item.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => RenameSegmentObject(region, item));
            item.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => DeleteSegmentObject(region, item, source));
            
            return item;
        }
        
        TreeViewItem createSubregionNode(SegmentSubregion subregion, SegmentSubregions source)
        {
            var item = new SegmentTreeViewItem(subregion, Brushes.Gray, false)
            {
                Tag = subregion,
            };

            item.ContextMenu = new ContextMenu();
            item.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => RenameSegmentObject(subregion, item));
            item.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => DeleteSegmentObject(subregion, item, source));

            return item;
        }
    }
    
    public void UpdateLocations()
    {
        if (Segment is null)
            return;

        var collection = Segment.Locations;
        
        if (_locationsNode is null)
        {
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
                    Name = $"Location {collection.Count + 1}" 
                };
                collection.Add(location);

                // present the new location to the user.
                location.Present(ServiceLocator.Current
                    .GetInstance<ApplicationPresenter>());
            });
        }
        
        var expanded = new HashSet<string>();
        
        foreach (var item in _locationsNode.Items.OfType<TreeViewItem>())
            SaveExpansionState(item, expanded);
        
        _locationsNode.Items.Clear();

        foreach (var child in collection)
            _locationsNode.Items.Add(createLocationNode(child, collection));
        
        foreach (var item in _locationsNode.Items.OfType<TreeViewItem>())
            RestoreExpansionState(item, expanded);

        TreeViewItem createLocationNode(SegmentLocation location, SegmentLocations source)
        {
            var isReserved = location.IsReserved;
            
            var item = new SegmentTreeViewItem(location, (isReserved ? Brushes.LightSlateGray : Brushes.LightPink), true)
            {
                Tag = location
            };

            if (!isReserved)
            {
                item.ContextMenu = new ContextMenu();
                item.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => RenameSegmentObject(location, item));
                item.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => DeleteSegmentObject(location, item, source));
            }

            return item;
        }
    }

    public void UpdateComponents()
    {
        if (Segment is null)
            return;

        var collection = Segment.Components;

        if (_componentsNode is null)
        {
            _componentsNode = new TreeViewItem
            {
                Tag = $"category:Components",
                Header = CreateHeader("Components", "Terrain.png")
            };

            _componentsNode.ContextMenu = new ContextMenu();
            _componentsNode.ContextMenu.AddItem("Add Component", "Add.png",
                (s, e) => AddSegmentObject(collection, "Component"));
        }

        var expanded = new HashSet<string>();

        foreach (var item in _componentsNode.Items.OfType<TreeViewItem>())
            SaveExpansionState(item, expanded);

        _componentsNode.Items.Clear();

        foreach (var component in collection)
            _componentsNode.Items.Add(createComponentNode(component, collection));

        foreach (var item in _componentsNode.Items.OfType<TreeViewItem>())
            RestoreExpansionState(item, expanded);
        
        TreeViewItem createComponentNode(SegmentComponent segmentComponent, SegmentComponents source)
        {
            var item = new SegmentTreeViewItem(segmentComponent, Brushes.OrangeRed, false)
            {
                Tag = segmentComponent,
            };
        
            item.ContextMenu = new ContextMenu();
            item.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => RenameSegmentObject(segmentComponent, item));
            item.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => DeleteSegmentObject(segmentComponent, item, source));

            return item;
        }
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
            item.ContextMenu.AddItem("Add Location Spawner", "Add.png",
                (s, e) => AddSpawner(source.Location, "Location Spawner", region.ID));
            item.ContextMenu.AddItem("Add Region Spawner", "Add.png",
                (s, e) => AddSpawner(source.Region, "Region Spawner", region.ID));
            
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
            item.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => DeleteSegmentObject(segmentSpawner, item, source));
            
            return item;
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
            _entitiesNode.ContextMenu.AddItem("Add Entities", "Add.png", 
                (s, e) => addEntity(String.Empty));
        }

        var expanded = new HashSet<string>();
        
        foreach (var item in _entitiesNode.Items.OfType<TreeViewItem>())
            SaveExpansionState(item, expanded);
        
        _entitiesNode.Items.Clear();

        foreach (var grouping in collection)
            createEntityGroupNode(grouping.Key, grouping);
        
        foreach (var item in _entitiesNode.Items.OfType<TreeViewItem>())
            RestoreExpansionState(item, expanded);
        
        TreeViewItem createEntityGroupNode(string groupPath, IEnumerable<SegmentEntity> entities)
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
            
            foreach (var entity in entities)
                parentFolder.Items.Add(createEntityEntryNode(entity));
    
            parentFolder.ContextMenu = new ContextMenu();
            parentFolder.ContextMenu.AddItem("Add Entity", "Add.png", 
                (s, e) => addEntity(groupPath));
            
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
            item.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => DeleteSegmentObject(segmentEntity, item, Segment.Entities));

            return item;
        }
    
        void addEntity(string group)
        {
            var defaultName = $"Entity {Segment.Entities.Count + 1}";
            var name = Interaction.InputBox("Name", "Add Entity", defaultName);
            
            if (string.IsNullOrWhiteSpace(name))
                return;
            
            var entity = new SegmentEntity
            {
                Name = name, 
                Group = group 
            };
            
            Segment.Entities.Add(entity);
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
            _treasureNode.ContextMenu.AddItem("Add Treasures", "Add.png", (s, e) => AddSegmentObject(collection, "Treasure"));
            _treasureNode.ContextMenu.AddItem("Add Hoard", "Add.png", (s, e) => AddHoard());
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
            item.ContextMenu.AddItem("Delete", "Delete.png", (s, e) => DeleteSegmentObject(treasure, item, collection));
            
            return item;
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
        
        UpdateSegment();
        UpdateRegions();
        UpdateLocations();
        UpdateComponents();
        UpdateSpawns();
        UpdateEntities();
        UpdateTreasures();
        
        _tree.Items.Add(_segmentNode);
        _tree.Items.Add(_regionsNode);
        _tree.Items.Add(_locationsNode);
        _tree.Items.Add(_componentsNode);
        _tree.Items.Add(_spawnersNode);
        _tree.Items.Add(_entitiesNode);
        _tree.Items.Add(_treasureNode);

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

    private void AddHoard()
    {
        var count = Segment.Treasures.Count(t => t is SegmentHoard) + 1;
        var defaultName = $"Hoard {count}";
        var name = Interaction.InputBox("Name", "Add Hoard", defaultName);
        if (string.IsNullOrWhiteSpace(name))
            return;
        Segment.Treasures.Add(new SegmentHoard { Name = name });
    }

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

    private T AddSegmentObject<T>(IList<T> collection, string typeName) where T : ISegmentObject, new()
    {
        var defaultName = $"{typeName} {collection.Count + 1}";
        var name = Interaction.InputBox("Name", $"Add {typeName}", defaultName);
        if (String.IsNullOrWhiteSpace(name))
            return default(T);
        var obj = new T { Name = name };
        collection.Add(obj);
        return obj;
    }

    private void RenameSegmentObject(ISegmentObject obj, TreeViewItem item)
    {
        var name = Interaction.InputBox("New name", "Rename", obj.Name);
        if (string.IsNullOrWhiteSpace(name))
            return;
        obj.Name = name;
        if (item.Header is StackPanel panel && panel.Children.Count > 1 && panel.Children[1] is TextBlock text)
            text.Text = name;
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
    public EditableTextBlock EditableTextBlock { get; }
    
    public SegmentTreeViewItem(ISegmentObject segmentObject, Brush brush, bool circleIcon)
    {
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

        innerPanel.Children.Add(EditableTextBlock = new EditableTextBlock()
        {
            Text = segmentObject.Name,
        });
        
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
        
        if (sender is SegmentTreeViewItem segmentTreeViewItem && segmentTreeViewItem.EditableTextBlock != null)
                segmentTreeViewItem.EditableTextBlock.IsInEditMode = true;
    }
}