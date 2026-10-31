using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Kesmai.WorldForge.UI;

/// <summary>
/// View > Program background: one color for WorldForge's windows and the large white areas in them (lists, trees,
/// tables, the property grid). Text in those areas turns white on a dark color; boxes for typing keep their own look so
/// what's typed stays readable. With no color chosen WorldForge looks as it always has. The color is kept in .storage.
/// </summary>
public static class ProgramTheme
{
	private static readonly string _path = Path.Combine(".storage", "Theme.cfg");

	private static ResourceDictionary _styles;
	private static bool _windowsHooked;

	/// <summary>The chosen color, or null for WorldForge's own look.</summary>
	public static Color? Background { get; private set; }

	/// <summary>Applies the color saved last time, at startup.</summary>
	public static void LoadSaved()
	{
		try
		{
			if (File.Exists(_path) && File.ReadAllLines(_path).FirstOrDefault(l => l.StartsWith("background=")) is { } line
			    && line.Substring("background=".Length).Trim() is { Length: > 0 } value)
				Apply((Color)ColorConverter.ConvertFromString(value), save: false);
		}
		catch (Exception exception) when (exception is IOException or FormatException or UnauthorizedAccessException)
		{
			// a theme file that can't be read leaves the usual look
		}
	}

	/// <summary>Sets the color (null for the usual look) on every open window and every one opened later.</summary>
	public static void Apply(Color? color, bool save = true)
	{
		Background = color;

		var resources = Application.Current.Resources;

		if (_styles is not null)
			resources.MergedDictionaries.Remove(_styles);

		_styles = null;

		if (color is { } chosen)
		{
			var background = Frozen(chosen);
			var foreground = IsDark(chosen) ? Brushes.White : Brushes.Black;

			_styles = new ResourceDictionary();

			// the big areas: each keeps its own style, with the color on top
			foreach (var type in new[] { typeof(TreeView), typeof(ListBox), typeof(ListView), typeof(DataGrid), typeof(PropertyGrid), typeof(System.Windows.Controls.Primitives.StatusBar) })
			{
				var style = new Style(type, Application.Current.TryFindResource(type) as Style);
				style.Setters.Add(new Setter(Control.BackgroundProperty, background));
				style.Setters.Add(new Setter(Control.ForegroundProperty, foreground));

				if (type == typeof(DataGrid))
				{
					style.Setters.Add(new Setter(DataGrid.RowBackgroundProperty, background));
					style.Setters.Add(new Setter(DataGrid.AlternatingRowBackgroundProperty, background));
				}

				_styles[type] = style;
			}

			resources.MergedDictionaries.Add(_styles);
		}

		if (!_windowsHooked)
		{
			// windows opened later (a floating palette, a dialog) get the color as they load
			EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler((sender, _) =>
			{
				if (sender is Window window)
					Paint(window);
			}));

			_windowsHooked = true;
		}

		foreach (var window in Application.Current.Windows.OfType<Window>())
			Paint(window);

		if (save)
			Save();
	}

	private static void Paint(Window window)
	{
		// the window's own color is remembered the first time, so Default puts it back
		var original = _originals.GetValue(window, w => new ThemeOriginal(w.ReadLocalValue(Control.BackgroundProperty)));

		if (Background is { } chosen)
			window.Background = Frozen(chosen);
		else if (original.Background == DependencyProperty.UnsetValue)
			window.ClearValue(Control.BackgroundProperty);
		else
			window.Background = (Brush)original.Background;
	}

	private sealed record ThemeOriginal(object Background);

	private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Window, ThemeOriginal> _originals = new();

	private static SolidColorBrush Frozen(Color color)
	{
		var brush = new SolidColorBrush(color);
		brush.Freeze();
		return brush;
	}

	public static bool IsDark(Color color) => 0.299 * color.R + 0.587 * color.G + 0.114 * color.B < 128;

	private static void Save()
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
			File.WriteAllLines(_path, [$"background={(Background is { } color ? color.ToString() : String.Empty)}"]);
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			// the color just isn't remembered
		}
	}
}
