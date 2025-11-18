using System;
using System.ComponentModel;
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
    private SpawnsTreeViewItem _spawnersNode;
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
        _spawnersNode?.Dispose();
        _spawnersNode = null;
        _brushesNode?.Dispose();
        _brushesNode = null;
        _templatesNode?.Dispose();
        _templatesNode = null;
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

    private void EnsureSpawnersNode()
    {
        if (_spawnersNode is not null)
            return;
        
        _spawnersNode = new SpawnsTreeViewItem(Segment, CreateHeader("Spawns", "Spawns.png"));
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
