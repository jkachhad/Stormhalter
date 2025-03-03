using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Kesmai.WorldForge.UI;

public class DropDownButton : ToggleButton
{
	public static readonly DependencyProperty MenuProperty = DependencyProperty.Register(nameof(Menu),
		typeof(ContextMenu), typeof(DropDownButton), new UIPropertyMetadata(null, OnMenuChanged));

	private static void OnMenuChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
	{
		if (sender is not DropDownButton dropDownButton || args.NewValue is not ContextMenu contextMenu)
			return;

		contextMenu.DataContext = dropDownButton.DataContext;
	}
	
	public ContextMenu Menu
	{
		get { return (ContextMenu)GetValue(MenuProperty); }
		set { SetValue(MenuProperty, value); }
	}
	
	public DropDownButton()
	{
		var binding = new Binding("Menu.IsOpen")
		{
			Source = this
		};
		SetBinding(IsCheckedProperty, binding);
		
		DataContextChanged += (sender, args) =>
		{
			if (Menu != null)
				Menu.DataContext = DataContext;
		};
	}
	
	protected override void OnClick()
	{
		if (Menu != null)
		{
			Menu.PlacementTarget = this;
			Menu.Placement = PlacementMode.Bottom;
			Menu.IsOpen = true;
		}
	}
}