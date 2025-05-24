using System.Collections.ObjectModel;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.UI;

public partial class ComponentGrid : ListBox
{
    public ComponentGrid ( )
    {
        InitializeComponent ( );
    }
    private void menuItemDeletePrefabLoaded ( object sender, System.Windows.RoutedEventArgs e )
    {
        if ( sender is MenuItem menuItem )
        {
            menuItem.Command = Kesmai.WorldForge.UI.ComponentsPanel.Instance?.DeletePrefabCommand;
        }
    }
}

public class ComponentsCategory : ObservableObject
{
    public string Name { get; set; }

    private ObservableCollection<TerrainComponent> _components;

    public ObservableCollection<TerrainComponent> Components
    {
        get => _components;
        set => SetProperty ( ref _components, value );
    }

    public ComponentsCategory ( )
    {
        _components = new ObservableCollection<TerrainComponent> ( );
    }
}