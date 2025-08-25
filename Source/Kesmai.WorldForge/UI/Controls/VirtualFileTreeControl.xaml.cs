using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualBasic;
using Kesmai.WorldForge.Editor;
using DigitalRune.Collections;

namespace Kesmai.WorldForge.UI;

public partial class VirtualFileTreeControl : UserControl
{
    public static readonly DependencyProperty ProjectProperty =
        DependencyProperty.Register(nameof(Project), typeof(SegmentProject), typeof(VirtualFileTreeControl),
            new PropertyMetadata(null, OnProjectChanged));

    public SegmentProject? Project
    {
        get => (SegmentProject?)GetValue(ProjectProperty);
        set => SetValue(ProjectProperty, value);
    }

    private FileSystemWatcher? _watcher;

    public VirtualFileTreeControl()
    {
        InitializeComponent();
        Unloaded += (_, _) => _watcher?.Dispose();
    }

    private static void OnProjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (VirtualFileTreeControl)d;
        if (e.OldValue is SegmentProject oldProject)
        {
            oldProject.PropertyChanged -= control.OnProjectPropertyChanged;
            oldProject.VirtualFiles.CollectionChanged -= control.OnItemsChanged;
            oldProject.Regions.CollectionChanged -= control.OnItemsChanged;
            oldProject.Spawns.CollectionChanged -= control.OnItemsChanged;
            oldProject.Treasures.CollectionChanged -= control.OnItemsChanged;
            oldProject.Hoards.CollectionChanged -= control.OnItemsChanged;
        }
        if (e.NewValue is SegmentProject newProject)
        {
            newProject.PropertyChanged += control.OnProjectPropertyChanged;
            newProject.VirtualFiles.CollectionChanged += control.OnItemsChanged;
            newProject.Regions.CollectionChanged += control.OnItemsChanged;
            newProject.Spawns.CollectionChanged += control.OnItemsChanged;
            newProject.Treasures.CollectionChanged += control.OnItemsChanged;
            newProject.Hoards.CollectionChanged += control.OnItemsChanged;
        }
        control.SetupWatcher();
        control.LoadRoot();
    }

    private void OnProjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SegmentProject.RootPath))
        {
            SetupWatcher();
            LoadRoot();
        }
        else if (e.PropertyName == nameof(SegmentProject.Name))
        {
            LoadRoot();
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => LoadRoot();

    private void SetupWatcher()
    {
        _watcher?.Dispose();
        _watcher = null;
        if (Project == null)
            return;
        var root = Project.RootPath;
        if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            return;

        _watcher = new FileSystemWatcher(root)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };
        _watcher.Created += OnFileSystemChanged;
        _watcher.Deleted += OnFileSystemChanged;
        _watcher.Renamed += OnFileSystemRenamed;
    }

    private void OnFileSystemChanged(object sender, FileSystemEventArgs e)
    {
        if (!ShouldRefresh(e.FullPath))
            return;
        Dispatcher.Invoke(LoadRoot);
    }

    private void OnFileSystemRenamed(object sender, RenamedEventArgs e)
    {
        if (!ShouldRefresh(e.FullPath) && !ShouldRefresh(e.OldFullPath))
            return;
        Dispatcher.Invoke(LoadRoot);
    }

    private static bool ShouldRefresh(string path)
    {
        var ext = Path.GetExtension(path);
        return string.IsNullOrEmpty(ext) || string.Equals(ext, ".cs", StringComparison.OrdinalIgnoreCase);
    }

    private void LoadRoot()
    {
        var expanded = new HashSet<string>();
        foreach (var item in Tree.Items.OfType<TreeViewItem>())
            SaveExpansionState(item, expanded);

        Tree.Items.Clear();
        if (Project == null)
            return;

        var rootPath = Project.RootPath;
        var rootItem = new TreeViewItem { Tag = rootPath };
        rootItem.Header = CreateHeader(Project.Name, rootPath, true);
        rootItem.PreviewMouseRightButtonDown += SelectOnRightClick;

        TreeViewItem? sourceItem = null;
        if (!string.IsNullOrEmpty(rootPath) && Directory.Exists(rootPath))
        {
            var sourcePath = Path.Combine(rootPath, "Source");
            Directory.CreateDirectory(sourcePath);
            sourceItem = CreateDirectoryNode(new DirectoryInfo(sourcePath));

            foreach (var file in Project.VirtualFiles)
                sourceItem.Items.Add(CreateVirtualFileNode(file));

            rootItem.Items.Add(sourceItem);
        }

        rootItem.Items.Add(CreateCategoryNode("Region", Project.Regions));
        rootItem.Items.Add(CreateCategoryNode("Spawn", Project.Spawns));
        rootItem.Items.Add(CreateCategoryNode("Treasure", Project.Treasures));
        rootItem.Items.Add(CreateCategoryNode("Hoard", Project.Hoards));

        if (!string.IsNullOrEmpty(rootPath))
        {
            var menu = new ContextMenu();
            var addFile = new MenuItem { Header = "Add System File" };
            addFile.Click += (s, e) =>
            {
                var dir = Path.Combine(rootPath, "Source");
                Directory.CreateDirectory(dir);
                AddFile(dir, sourceItem ?? rootItem);
            };
            menu.Items.Add(addFile);
            rootItem.ContextMenu = menu;
        }

        Tree.Items.Add(rootItem);
        RestoreExpansionState(rootItem, expanded);
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
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
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

    private TreeViewItem CreateVirtualFileNode(VirtualFile file) =>
        CreateInMemoryNode(file, file.Name + ".cs");

    private TreeViewItem CreateCategoryNode<T>(string name, NotifyingCollection<T> collection) where T : ISegmentObject, new()
    {
        var item = new TreeViewItem { Tag = $"category:{name}" };
        item.Header = CreateHeader(name, name, true);
        item.PreviewMouseRightButtonDown += SelectOnRightClick;

        foreach (var child in collection)
            item.Items.Add(CreateCategoryEntryNode(child, collection));

        var menu = new ContextMenu();
        var add = new MenuItem { Header = $"Add {name}" };
        add.Click += (s, e) => AddSegmentObject(collection, name);
        menu.Items.Add(add);
        item.ContextMenu = menu;

        return item;
    }

    private TreeViewItem CreateCategoryEntryNode(ISegmentObject obj, IList collection)
    {
        var item = new TreeViewItem { Tag = obj };
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

    private void AddSegmentObject<T>(NotifyingCollection<T> collection, string typeName) where T : ISegmentObject, new()
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

