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
using Kesmai.WorldForge;
using Kesmai.WorldForge.Editor;
using DigitalRune.Collections;

namespace Kesmai.WorldForge.UI;

public class SegmentObjectDoubleClick(ISegmentObject target) : ValueChangedMessage<ISegmentObject>(target);
public class SegmentObjectSelected(ISegmentObject target) : ValueChangedMessage<ISegmentObject>(target);

public partial class SegmentTreeControl : UserControl
{
    public static readonly DependencyProperty SegmentProperty =
        DependencyProperty.Register(nameof(Segment), typeof(Segment), typeof(SegmentTreeControl),
            new PropertyMetadata(null, OnSegmentChanged));

    public Segment? Segment
    {
        get => (Segment?)GetValue(SegmentProperty);
        set => SetValue(SegmentProperty, value);
    }
    
    public SegmentTreeControl()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<SegmentRegionsChanged>(this, (r, m) => UpdateRegions());
        WeakReferenceMessenger.Default.Register<SegmentRegionChanged>(this, (r, m) => UpdateRegions());
        
        WeakReferenceMessenger.Default.Register<SegmentLocationsChanged>(this, (r, m) => UpdateLocations());
        WeakReferenceMessenger.Default.Register<SegmentLocationChanged>(this, (r, m) => UpdateLocations());
        
        WeakReferenceMessenger.Default.Register<SegmentEntitiesChanged>(this, (r, m) => UpdateEntities());
        WeakReferenceMessenger.Default.Register<SegmentEntityChanged>(this, (r, m) => UpdateEntities());
        
        WeakReferenceMessenger.Default.Register<SegmentTreasuresChanged>(this, (r, m) => UpdateTreasures());
        WeakReferenceMessenger.Default.Register<SegmentTreasureChanged>(this, (r, m) => UpdateTreasures());
        
        WeakReferenceMessenger.Default.Register<SegmentSpawnsChanged>(this, (r, m) => UpdateSpawns());
        WeakReferenceMessenger.Default.Register<SegmentSpawnChanged>(this, (r, m) => UpdateSpawns());
        
        _tree.SelectedItemChanged += OnItemSelected;
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

    private TreeViewItem _regionsNode;
    private TreeViewItem _locationsNode;
    private TreeViewItem _entitiesNode;
    private TreeViewItem _spawnersNode;
    private TreeViewItem _treasureNode;
    
    public void UpdateRegions()
    {
        if (Segment is null)
            return;

        if (_regionsNode is null)
        {
            _regionsNode = new TreeViewItem
            {
                Tag = $"category:Regions",
                Header = CreateHeader("Regions", "Regions", true)
            };
        }

        var collection = Segment.Regions;
        
        _regionsNode.Items.Clear();

        foreach (var child in collection)
        {
            _regionsNode.Items.Add(createRegionNode(child, collection));
        }

        var categoryMenu = new ContextMenu();
        var add = new MenuItem
        {
            Header = $"Add Region" 
        };
        add.Click += (s, e) => AddSegmentObject(collection, "Region");
        
        categoryMenu.Items.Add(add);
        
        _regionsNode.ContextMenu = categoryMenu;

        TreeViewItem createRegionNode(SegmentRegion region, SegmentRegions source)
        {
            var item = new TreeViewItem
            {
                Tag = region,
                Header = CreateColoredHeader(region.Name, Brushes.MediumPurple, false)
            };
            item.MouseDoubleClick += OnDoubleClick;

            var itemMenu = new ContextMenu();
            
            var rename = new MenuItem { Header = "Rename" };
            rename.Click += (s, e) => RenameSegmentObject(region, item);
            
            var delete = new MenuItem { Header = "Delete" };
            delete.Click += (s, e) => DeleteSegmentObject(region, item, source);
            
            itemMenu.Items.Add(rename);
            itemMenu.Items.Add(delete);
            
            item.ContextMenu = itemMenu;

            return item;
        }
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
                Header = CreateHeader("Locations", "Locations", true)
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
        
        _locationsNode.Items.Clear();

        foreach (var child in collection)
        {
            var item = new TreeViewItem
            {
                Header = CreateColoredHeader(child.Name, Brushes.LightPink, true),
                Tag = child
            };
            
            item.ContextMenu = new ContextMenu();
            item.ContextMenu.AddItem("Rename", String.Empty, (s, e) 
                => RenameSegmentObject(child, item));
            item.ContextMenu.AddItem("Delete", "Delete.png", (s, e) 
                => DeleteSegmentObject(child, item, collection));
            
            _locationsNode.Items.Add(item);
        }
    }

    public void UpdateSpawns()
    {
        if (Segment is null)
            return;

        if (_spawnersNode is null)
        {
            _spawnersNode = new TreeViewItem
            {
                Tag = $"category:Spawns",
                Header = CreateHeader("Spawns", "Spawns", true)
            };
        }

        var collection = Segment.Spawns;
        
        _spawnersNode.Items.Clear();
        
        foreach (var region in Segment.Regions)
            _spawnersNode.Items.Add(CreateSpawnerRegionNode(region));

        var menu = new ContextMenu();
        var addLocation = new MenuItem
        {
            Header = $"Add Location Spawner" 
        };
        addLocation.Click += (s, e) => AddSpawner(collection.Location, "Location Spawner", 0);
        var addRegion = new MenuItem
        {
            Header = $"Add Region Spawner" 
        };
        addRegion.Click += (s, e) => AddSpawner(collection.Region, "Region Spawner", 0);
        
        menu.Items.Add(addLocation);
        menu.Items.Add(addRegion);
        
        _spawnersNode.ContextMenu = menu;
    }

    public void UpdateEntities()
    {
        if (Segment is null)
            return;

        if (_entitiesNode is null)
        {
            _entitiesNode = new TreeViewItem
            {
                Tag = $"category:Entity",
                Header = CreateHeader("Entity", "Entity", true)
            };
        }

        var collection = Segment.Entities
            .GroupBy(e => string.IsNullOrEmpty(e.Group) ? "Unassigned" : e.Group)
            .OrderBy(g => g.Key).ToList();
        
        _entitiesNode.Items.Clear();
        
        foreach (var group in collection)
            _entitiesNode.Items.Add(CreateEntityGroupNode(group.Key, group));

        var menu = new ContextMenu();
        var add = new MenuItem
        {
            Header = $"Add Entity" 
        };
        add.Click += (s, e) => AddEntity("Unassigned");
        
        menu.Items.Add(add);
        
        _entitiesNode.ContextMenu = menu;
    }

    public void UpdateTreasures()
    {
        if (Segment is null)
            return;

        if (_treasureNode is null)
        {
            _treasureNode = new TreeViewItem
            {
                Tag = $"category:Treasure",
                Header = CreateHeader("Treasure", "Treasure", true)
            };
        }

        var collection = Segment.Treasures;
        
        _treasureNode.Items.Clear();
        
        foreach (var child in collection)
            _treasureNode.Items.Add(CreateTreasureEntryNode(child));

        var menu = new ContextMenu();
        var addTreasure = new MenuItem
        {
            Header = $"Add Treasure" 
        };
        addTreasure.Click += (s, e) => AddSegmentObject(collection, "Treasure");
        var addHoard = new MenuItem
        {
            Header = $"Add Hoard" 
        };
        addHoard.Click += (s, e) => AddHoard();
        
        menu.Items.Add(addTreasure);
        menu.Items.Add(addHoard);
        
        _treasureNode.ContextMenu = menu;
    }
    
    private void Update()
    {
        var expanded = new HashSet<string>();
        foreach (var item in _tree.Items.OfType<TreeViewItem>())
            SaveExpansionState(item, expanded);

        _tree.Items.Clear();
        if (Segment == null)
            return;

        var rootPath = Segment.Directory;
        
        UpdateRegions();
        UpdateLocations();
        UpdateSpawns();
        UpdateEntities();
        UpdateTreasures();
        
        _tree.Items.Add(_regionsNode);
        _tree.Items.Add(_locationsNode);
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
        if (item.Tag is string path && item.IsExpanded)
            expanded.Add(path);
        foreach (var child in item.Items.OfType<TreeViewItem>())
            SaveExpansionState(child, expanded);
    }

    private static void RestoreExpansionState(TreeViewItem item, HashSet<string> expanded)
    {
        if (item.Tag is string path && expanded.Contains(path))
            item.IsExpanded = true;
        foreach (var child in item.Items.OfType<TreeViewItem>())
            RestoreExpansionState(child, expanded);
    }

    private StackPanel CreateHeader(string name, string path, bool isDirectory)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal 
        };
        
        var image = new Image
        {
            Width = 16,
            Height = 16,
            Margin = new Thickness(2, 0, 2, 0),
            
            Source = GetIcon(path, isDirectory)
        };
        
        panel.Children.Add(image);
        panel.Children.Add(new TextBlock { Text = name });
        
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
        item.Header = CreateHeader(dir.Name, dir.FullName, true);

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
        item.Header = CreateHeader(file.Name, file.FullName, false);

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

    private TreeViewItem CreateEntityGroupNode(string groupName, IEnumerable<Entity> entities)
    {
        var item = new TreeViewItem { Tag = $"category:Entity/{groupName}" };
        item.Header = CreateHeader(groupName, groupName, true);

        foreach (var entity in entities)
            item.Items.Add(CreateEntityEntryNode(entity));

        var menu = new ContextMenu();
        var add = new MenuItem { Header = "Add Entity" };
        add.Click += (s, e) => AddEntity(groupName);
        menu.Items.Add(add);
        item.ContextMenu = menu;

        return item;
    }

    private TreeViewItem CreateEntityEntryNode(Entity entity)
    {
        var item = new TreeViewItem { Tag = entity };

        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        var icon = new Ellipse { Width = 10, Height = 10, Fill = Brushes.Yellow, Margin = new Thickness(2, 0, 2, 0) };
        panel.Children.Add(icon);
        panel.Children.Add(new TextBlock { Text = entity.Name });
        item.Header = panel;

        var menu = new ContextMenu();
        var rename = new MenuItem { Header = "Rename" };
        rename.Click += (s, e) => RenameSegmentObject(entity, item);
        var delete = new MenuItem { Header = "Delete" };
        delete.Click += (s, e) => DeleteSegmentObject(entity, item, Segment.Entities);
        menu.Items.Add(rename);
        menu.Items.Add(delete);
        item.ContextMenu = menu;

        return item;
    }

    private void AddEntity(string group)
    {
        var defaultName = $"Entity {Segment.Entities.Count + 1}";
        var name = Interaction.InputBox("Name", "Add Entity", defaultName);
        if (string.IsNullOrWhiteSpace(name))
            return;
        var entity = new Entity { Name = name, Group = group };
        Segment.Entities.Add(entity);
    }

    private TreeViewItem CreateSpawnerRegionNode(SegmentRegion region)
    {
        var item = new TreeViewItem { Tag = $"category:Spawn/{region.ID}" };
        item.Header = CreateColoredHeader(region.Name, Brushes.MediumPurple, false);

        foreach (var spawner in Segment.Spawns.Location.Where(s => GetRegionId(s) == region.ID))
            item.Items.Add(CreateSpawnerEntryNode(spawner, Segment.Spawns.Location));

        foreach (var spawner in Segment.Spawns.Region.Where(s => GetRegionId(s) == region.ID))
            item.Items.Add(CreateSpawnerEntryNode(spawner, Segment.Spawns.Region));

        var menu = new ContextMenu();
        var addLocation = new MenuItem { Header = "Add Location Spawner" };
        addLocation.Click += (s, e) => AddSpawner(Segment.Spawns.Location, "Location Spawner", region.ID);
        var addRegion = new MenuItem { Header = "Add Region Spawner" };
        addRegion.Click += (s, e) => AddSpawner(Segment.Spawns.Region, "Region Spawner", region.ID);
        menu.Items.Add(addLocation);
        menu.Items.Add(addRegion);
        item.ContextMenu = menu;

        return item;
    }

    private TreeViewItem CreateSpawnerEntryNode(Spawner spawner, IList collection)
    {
        var item = new TreeViewItem { Tag = spawner };

        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        Shape icon = spawner switch
        {
            LocationSpawner => new Ellipse { Width = 10, Height = 10, Fill = Brushes.SkyBlue },
            RegionSpawner => new Rectangle { Width = 10, Height = 10, Fill = Brushes.Orange },
            _ => new Rectangle { Width = 10, Height = 10, Fill = Brushes.Gray }
        };
        icon.Margin = new Thickness(2, 0, 2, 0);
        panel.Children.Add(icon);
        panel.Children.Add(new TextBlock { Text = spawner.Name });
        item.Header = panel;

        var menu = new ContextMenu();
        var rename = new MenuItem { Header = "Rename" };
        rename.Click += (s, e) => RenameSegmentObject(spawner, item);
        var delete = new MenuItem { Header = "Delete" };
        delete.Click += (s, e) => DeleteSegmentObject(spawner, item, collection);
        menu.Items.Add(rename);
        menu.Items.Add(delete);
        item.ContextMenu = menu;

        return item;
    }
    
    private TreeViewItem CreateTreasureEntryNode(SegmentTreasure treasure)
    {
        var item = new TreeViewItem { Tag = treasure };

        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        Shape icon = treasure is SegmentHoard
            ? new Rectangle { Width = 10, Height = 10, Fill = Brushes.Red }
            : new Ellipse { Width = 10, Height = 10, Fill = Brushes.Green };
        icon.Margin = new Thickness(2, 0, 2, 0);
        panel.Children.Add(icon);
        panel.Children.Add(new TextBlock { Text = treasure.Name });
        item.Header = panel;

        var menu = new ContextMenu();
        var rename = new MenuItem { Header = "Rename" };
        rename.Click += (s, e) => RenameSegmentObject(treasure, item);
        var delete = new MenuItem { Header = "Delete" };
        delete.Click += (s, e) => DeleteSegmentObject(treasure, item, Segment.Treasures);
        menu.Items.Add(rename);
        menu.Items.Add(delete);
        item.ContextMenu = menu;

        return item;
    }

    private void AddHoard()
    {
        var count = Segment.Treasures.Count(t => t is SegmentHoard) + 1;
        var defaultName = $"Hoard {count}";
        var name = Interaction.InputBox("Name", "Add Hoard", defaultName);
        if (string.IsNullOrWhiteSpace(name))
            return;
        Segment.Treasures.Add(new SegmentHoard { Name = name });
    }

    private void AddSpawner<T>(IList<T> collection, string typeName, int regionId) where T : Spawner, new()
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

    private static int GetRegionId(Spawner spawner) => spawner switch
    {
        LocationSpawner ls => ls.Region,
        RegionSpawner rs => rs.Region,
        _ => 0
    };

    private TreeViewItem CreateInMemoryNode(ISegmentObject obj, string displayName)
    {
        var item = new TreeViewItem { Tag = obj };
        var header = CreateHeader(displayName, displayName, false);
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

    private void AddSegmentObject<T>(IList<T> collection, string typeName) where T : ISegmentObject, new()
    {
        var defaultName = $"{typeName} {collection.Count + 1}";
        var name = Interaction.InputBox("Name", $"Add {typeName}", defaultName);
        if (string.IsNullOrWhiteSpace(name))
            return;
        var obj = new T { Name = name };
        collection.Add(obj);
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
