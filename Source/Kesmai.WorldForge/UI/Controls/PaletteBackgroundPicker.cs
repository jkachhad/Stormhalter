using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Kesmai.WorldForge.UI;

/// <summary>Sets the palette's or the whole program's background color from the View menu, with a color picker for Custom.</summary>
public static class PaletteBackgroundPicker
{
	/// <summary>View > Palette background.</summary>
	public static void Apply(string choice, Window owner)
	{
		var before = ComponentPaletteSettings.Background;
		var color = choice == "Custom"
			? Pick(owner, "Palette background", before, live => ComponentPaletteSettings.Background = live)
			: (Color)ColorConverter.ConvertFromString(choice);

		if (color is null)
		{
			ComponentPaletteSettings.Background = before;
			return;
		}

		ComponentPaletteSettings.Background = color.Value;
		ComponentPaletteSettings.Save();
	}

	/// <summary>View > Program background; Default puts WorldForge's own look back.</summary>
	public static void ApplyProgram(string choice, Window owner)
	{
		if (choice == "Default")
		{
			ProgramTheme.Apply(null);
			return;
		}

		var before = ProgramTheme.Background;
		var color = choice == "Custom"
			? Pick(owner, "Program background", before ?? Color.FromRgb(0xDD, 0xDD, 0xDD), live => ProgramTheme.Apply(live, save: false))
			: (Color)ColorConverter.ConvertFromString(choice);

		if (color is null)
		{
			ProgramTheme.Apply(before, save: false);
			return;
		}

		ProgramTheme.Apply(color.Value);
	}

	/// <summary>Picks any color, starting from <paramref name="start"/>; null when cancelled.</summary>
	public static Color? PickColor(Window owner, string title, Color start) => Pick(owner, title, start, _ => { });

	/// <summary>A small window with a color canvas; the color shows live while picked. Null when cancelled.</summary>
	private static Color? Pick(Window owner, string title, Color start, Action<Color> preview)
	{
		var canvas = new ColorCanvas
		{
			SelectedColor = start,
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
			Title = title,
			Content = layout,
			Owner = owner,
			SizeToContent = SizeToContent.WidthAndHeight,
			ResizeMode = ResizeMode.NoResize,
			WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
			ShowInTaskbar = false,
		};

		canvas.SelectedColorChanged += (_, _) =>
		{
			if (canvas.SelectedColor is { } live)
				preview(Color.FromRgb(live.R, live.G, live.B));
		};

		ok.Click += (_, _) => window.DialogResult = true;

		return window.ShowDialog() == true && canvas.SelectedColor is { } chosen
			? Color.FromRgb(chosen.R, chosen.G, chosen.B)
			: null;
	}
}
