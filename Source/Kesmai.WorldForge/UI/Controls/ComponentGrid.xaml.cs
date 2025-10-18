using System;
using System.Windows;
using System.Windows.Controls;

namespace Kesmai.WorldForge.UI;

public partial class ComponentGrid : ListBox
{
	private UniformGridPanel _uniformGridPanel;
	
	public ComponentGrid()
	{
		InitializeComponent();
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