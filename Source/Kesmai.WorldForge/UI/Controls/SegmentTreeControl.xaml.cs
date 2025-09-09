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
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.VisualBasic;
using Kesmai.WorldForge;
using Kesmai.WorldForge.Editor;
using DigitalRune.Collections;

namespace Kesmai.WorldForge.UI;

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
    }

    private static void OnSegmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SegmentTreeControl)d;
        if (e.OldValue is Segment oldSegment)
        {
            oldSegment.PropertyChanged -= control.OnSegmentPropertyChanged;
            /*oldSegment.VirtualFiles.CollectionChanged -= control.OnNotifyingItemsChanged;*/
            oldSegment.Locations.CollectionChanged -= control.OnCollectionChanged;
            oldSegment.Entities.CollectionChanged -= control.OnEntitiesCollectionChanged;
            foreach (var s in oldSegment.Entities)
                s.PropertyChanged -= control.OnEntityPropertyChanged;
            oldSegment.Spawns.Location.CollectionChanged -= control.OnSpawnsCollectionChanged;
            foreach (var s in oldSegment.Spawns.Location)
                s.PropertyChanged -= control.OnSpawnerPropertyChanged;
            oldSegment.Spawns.Region.CollectionChanged -= control.OnSpawnsCollectionChanged;
            foreach (var s in oldSegment.Spawns.Region)
                s.PropertyChanged -= control.OnSpawnerPropertyChanged;
            oldSegment.Treasures.CollectionChanged -= control.OnCollectionChanged;
        }
        if (e.NewValue is Segment newSegment)
        {
            newSegment.PropertyChanged += control.OnSegmentPropertyChanged;
            /*newSegment.VirtualFiles.CollectionChanged += control.OnNotifyingItemsChanged;*/
            newSegment.Locations.CollectionChanged += control.OnCollectionChanged;
            newSegment.Entities.CollectionChanged += control.OnEntitiesCollectionChanged;
            foreach (var en in newSegment.Entities)
                en.PropertyChanged += control.OnEntityPropertyChanged;
            newSegment.Spawns.Location.CollectionChanged += control.OnSpawnsCollectionChanged;
            foreach (var s in newSegment.Spawns.Location)
                s.PropertyChanged += control.OnSpawnerPropertyChanged;
            newSegment.Spawns.Region.CollectionChanged += control.OnSpawnsCollectionChanged;
            foreach (var s in newSegment.Spawns.Region)
                s.PropertyChanged += control.OnSpawnerPropertyChanged;
            newSegment.Treasures.CollectionChanged += control.OnCollectionChanged;
        }
        control.LoadRoot();
    }

    private void OnSegmentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Segment.Path))
        {
            LoadRoot();
        }
        // Segment name changes no longer affect the tree structure, since the
        // segment name is no longer shown as a root node.
    }
    
    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => LoadRoot();
    private void OnSpawnsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
            foreach (Spawner s in e.OldItems)
                s.PropertyChanged -= OnSpawnerPropertyChanged;
        if (e.NewItems != null)
            foreach (Spawner s in e.NewItems)
                s.PropertyChanged += OnSpawnerPropertyChanged;
        LoadRoot();
    }

    private void OnSpawnerPropertyChanged(object? sender, PropertyChangedEventArgs e) => LoadRoot();

    private void OnEntitiesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
            foreach (Entity en in e.OldItems)
                en.PropertyChanged -= OnEntityPropertyChanged;
        if (e.NewItems != null)
            foreach (Entity en in e.NewItems)
                en.PropertyChanged += OnEntityPropertyChanged;
        LoadRoot();
    }

    private void OnEntityPropertyChanged(object? sender, PropertyChangedEventArgs e) => LoadRoot();
    
    private TreeViewItem _regionsNode;
    
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
            _regionsNode.PreviewMouseRightButtonDown += SelectOnRightClick;
        }

        var collection = Segment.Regions;
        
        _regionsNode.Items.Clear();
        
        foreach (var child in collection)
            _regionsNode.Items.Add(CreateCategoryEntryNode(child, collection));

        var menu = new ContextMenu();
        var add = new MenuItem
        {
            Header = $"Add Region" 
        };
        add.Click += (s, e) => AddSegmentObject(collection, "Region");
        
        menu.Items.Add(add);
        
        _regionsNode.ContextMenu = menu;
    }
    

    private void LoadRoot()
    {
        var expanded = new HashSet<string>();
        foreach (var item in Tree.Items.OfType<TreeViewItem>())
            SaveExpansionState(item, expanded);

        Tree.Items.Clear();
        if (Segment == null)
            return;

        var rootPath = Segment.Path;

        // Add Region category
        UpdateRegions();
        
        Tree.Items.Add(_regionsNode);

        // Add Locations category
        Tree.Items.Add(CreateCategoryNode("Location", Segment.Locations));

        // Add Spawns grouped by region
        var spawnsItem = new TreeViewItem { Tag = "category:Spawn" };
        spawnsItem.Header = CreateHeader("Spawn", "Spawn", true);
        spawnsItem.PreviewMouseRightButtonDown += SelectOnRightClick;
        foreach (var region in Segment.Regions)
            spawnsItem.Items.Add(CreateSpawnerRegionNode(region));
        Tree.Items.Add(spawnsItem);

        // Add Entities grouped by entity group
        Tree.Items.Add(CreateEntityCategoryNode());

        // Add Treasure category
        Tree.Items.Add(CreateTreasureCategoryNode());

        // Add Source directory last
        if (!string.IsNullOrEmpty(rootPath) && Directory.Exists(rootPath))
        {
            var sourcePath = Path.Combine(rootPath, "Source");
            Directory.CreateDirectory(sourcePath);
            var sourceItem = CreateDirectoryNode(new DirectoryInfo(sourcePath));

            /*foreach (var file in Segment.VirtualFiles)
                sourceItem.Items.Add(CreateVirtualFileNode(file));*/

            Tree.Items.Add(sourceItem);
        }

        foreach (var item in Tree.Items.OfType<TreeViewItem>())
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
        item.PreviewMouseRightButtonDown += SelectOnRightClick;

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
        item.PreviewMouseRightButtonDown += SelectOnRightClick;

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

    private TreeViewItem CreateCategoryNode<T>(string name, IList<T> collection, string? menuName = null, string? tag = null) where T : ISegmentObject, new()
    {
        var item = new TreeViewItem
        {
            Tag = tag ?? $"category:{name}",
            Header = CreateHeader(name, name, true)
        };
        item.PreviewMouseRightButtonDown += SelectOnRightClick;

        foreach (var child in collection)
            item.Items.Add(CreateCategoryEntryNode(child, (IList)collection));

        var menu = new ContextMenu();
        var addText = menuName ?? name;
        var add = new MenuItem { Header = $"Add {addText}" };
        add.Click += (s, e) => AddSegmentObject(collection, addText);
        menu.Items.Add(add);
        item.ContextMenu = menu;

        return item;
    }

    private TreeViewItem CreateCategoryEntryNode(ISegmentObject obj, IList collection)
    {
        var item = new TreeViewItem { Tag = obj };
        if (obj is SegmentLocation)
            item.Header = CreateColoredHeader(obj.Name, Brushes.LightPink, true);
        else if (obj is SegmentRegion)
            item.Header = CreateColoredHeader(obj.Name, Brushes.MediumPurple, false);
        else
            item.Header = CreateHeader(obj.Name, obj.Name, false);
        item.PreviewMouseRightButtonDown += SelectOnRightClick;

        var menu = new ContextMenu();
        var rename = new MenuItem { Header = "Rename" };
        rename.Click += (s, e) => RenameSegmentObject(obj, item);
        var delete = new MenuItem { Header = "Delete" };
        delete.Click += (s, e) => DeleteSegmentObject(obj, item, collection);
        menu.Items.Add(rename);
        menu.Items.Add(delete);
        item.ContextMenu = menu;

        return item;
    }

    private TreeViewItem CreateEntityCategoryNode()
    {
        var item = new TreeViewItem { Tag = "category:Entity" };
        item.Header = CreateHeader("Entity", "Entity", true);
        item.PreviewMouseRightButtonDown += SelectOnRightClick;

        foreach (var group in Segment.Entities
                     .GroupBy(e => string.IsNullOrEmpty(e.Group) ? "Unassigned" : e.Group)
                     .OrderBy(g => g.Key))
            item.Items.Add(CreateEntityGroupNode(group.Key, group));

        var menu = new ContextMenu();
        var add = new MenuItem { Header = "Add Entity" };
        add.Click += (s, e) => AddEntity("Unassigned");
        menu.Items.Add(add);
        item.ContextMenu = menu;

        return item;
    }

    private TreeViewItem CreateEntityGroupNode(string groupName, IEnumerable<Entity> entities)
    {
        var item = new TreeViewItem { Tag = $"category:Entity/{groupName}" };
        item.Header = CreateHeader(groupName, groupName, true);
        item.PreviewMouseRightButtonDown += SelectOnRightClick;

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
        item.PreviewMouseRightButtonDown += SelectOnRightClick;

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
        item.PreviewMouseRightButtonDown += SelectOnRightClick;

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
        item.PreviewMouseRightButtonDown += SelectOnRightClick;

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

    private TreeViewItem CreateTreasureCategoryNode()
    {
        var item = new TreeViewItem { Tag = "category:Treasure" };
        item.Header = CreateHeader("Treasure", "Treasure", true);
        item.PreviewMouseRightButtonDown += SelectOnRightClick;

        foreach (var treasure in Segment.Treasures)
            item.Items.Add(CreateTreasureEntryNode(treasure));

        var menu = new ContextMenu();
        var addTreasure = new MenuItem { Header = "Add Treasure" };
        addTreasure.Click += (s, e) => AddSegmentObject(Segment.Treasures, "Treasure");
        var addHoard = new MenuItem { Header = "Add Hoard" };
        addHoard.Click += (s, e) => AddHoard();
        menu.Items.Add(addTreasure);
        menu.Items.Add(addHoard);
        item.ContextMenu = menu;

        return item;
    }

    private TreeViewItem CreateTreasureEntryNode(SegmentTreasure treasure)
    {
        var item = new TreeViewItem { Tag = treasure };
        item.PreviewMouseRightButtonDown += SelectOnRightClick;

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
        item.PreviewMouseRightButtonDown += SelectOnRightClick;
        item.ContextMenu = null;
        if (header.Children.Count > 1 && header.Children[1] is TextBlock text)
        {
            text.Foreground = Brushes.LightGray;
            text.FontWeight = FontWeights.Bold;
        }
        item.Header = header;
        return item;
    }

    private void SelectOnRightClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is TreeViewItem item)
            item.IsSelected = true;
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

