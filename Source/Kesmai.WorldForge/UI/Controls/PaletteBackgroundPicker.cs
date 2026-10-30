using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Kesmai.WorldForge.UI;

/// <summary>Sets the palette's background color from the View menu, with a color picker for Custom.</summary>
public static class PaletteBackgroundPicker
{
	public static void Apply(string choice, Window owner)
	{
		Color color;

		if (choice == "Custom")
		{
			if (Pick(owner) is not { } picked)
				return;

			color = picked;
		}
		else
		{
			color = (Color)ColorConverter.ConvertFromString(choice);
		}

		ComponentPaletteSettings.Background = color;
		ComponentPaletteSettings.Save();
	}

	/// <summary>A small window with a color canvas, starting at the current color; null when cancelled.</summary>
	private static Color? Pick(Window owner)
	{
		var canvas = new ColorCanvas
		{
			SelectedColor = ComponentPaletteSettings.Background,
			UsingAlphaChannel = false,
			Margin = new Thickness(8),
		};

		var ok = new Button { Content = "OK", Width = 80, IsDefault = true, Margin = new Thickness(0, 0, 6, 0) };
		var cancel = new Button { Content = "Cancel", Width = 80, IsCancel = true };
		var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(8, 0, 8, 8) };
		buttons.Children.Add(ok);
		buttons.Children.Add(cancel);

		var layout = new DockPanel();
		DockPanel.SetDock(buttons, Dock.Bottom);
		layout.Children.Add(buttons);
		layout.Children.Add(canvas);

		var window = new Window
		{
			Title = "Palette background",
			Content = layout,
			Owner = owner,
			SizeToContent = SizeToContent.WidthAndHeight,
			ResizeMode = ResizeMode.NoResize,
			WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
			ShowInTaskbar = false,
		};

		// the palette shows the color while it is being picked, and goes back if cancelled
		var before = ComponentPaletteSettings.Background;
		canvas.SelectedColorChanged += (_, _) =>
		{
			if (canvas.SelectedColor is { } live)
				ComponentPaletteSettings.Background = live;
		};

		ok.Click += (_, _) => window.DialogResult = true;

		if (window.ShowDialog() == true && canvas.SelectedColor is { } chosen)
			return Color.FromRgb(chosen.R, chosen.G, chosen.B);

		ComponentPaletteSettings.Background = before;
		return null;
	}
}
