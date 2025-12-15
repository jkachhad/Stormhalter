using NuGet.Protocol.Plugins;

namespace System.Windows.Controls;

public static class ContextMenuExtensions
{
	public static void AddItem(this ContextMenu contextMenu, string header, string icon, RoutedEventHandler clickHandler)
	{
		var menuItem = new MenuItem
		{
			Header = header
		};
		menuItem.Click += clickHandler;
		
		if (!String.IsNullOrEmpty(icon))
		{
			var image = new Image
			{
				Source = new Media.Imaging.BitmapImage(
					new Uri($"pack://application:,,,/Kesmai.WorldForge;component/Resources/{icon}")),
				
				Width = 16,
				Height = 16
			};
			menuItem.Icon = image;
		}
		
		contextMenu.Items.Add(menuItem);
	}
}