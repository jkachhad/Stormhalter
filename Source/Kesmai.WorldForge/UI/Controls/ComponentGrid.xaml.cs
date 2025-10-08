using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.UI;

public partial class ComponentGrid : ListBox
{
	private UniformGridPanel _uniformGridPanel;
	
	public ComponentGrid ( )
	{
		InitializeComponent ( );
	}

	private void menuItemDeletePrefabLoaded(object sender, System.Windows.RoutedEventArgs e)
	{
		if (sender is MenuItem menuItem)
		{
			menuItem.Command = Kesmai.WorldForge.UI.ComponentsPanel.Instance?.DeletePrefabCommand;
		}
	}

	private void OnSizeChanged(object sender, SizeChangedEventArgs args)
	{
		if (_uniformGridPanel is null)
			return;
		
		if (args.WidthChanged)
			_uniformGridPanel.Columns = (int)(args.NewSize.Width / 100);
        
		if (args.HeightChanged)
			_uniformGridPanel.Rows = (int)(args.NewSize.Height / 120);
	}

	private void uniformGridPanel_OnLoaded(object sender, RoutedEventArgs e)
	{
		if (sender is UniformGridPanel uniformGridPanel)
			_uniformGridPanel = uniformGridPanel;
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