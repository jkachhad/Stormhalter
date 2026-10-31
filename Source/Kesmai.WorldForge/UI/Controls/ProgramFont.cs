using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Kesmai.WorldForge.UI;

/// <summary>
/// View > Font: the font, size and color of all of WorldForge's text. Windows pass the font down to their text, and
/// text that names its own font or size (much of WorldForge uses Tahoma at set sizes) is switched too as it appears; the
/// size is a percentage, so headings stay larger than labels. Default puts every one back. Kept in .storage and applied
/// at startup.
/// </summary>
public static class ProgramFont
{
	private static readonly string _path = Path.Combine(".storage", "Font.cfg");

	// what each element had before, so Default can put it back
	private static readonly ConditionalWeakTable<DependencyObject, object> _familyOriginals = new();
	private static readonly ConditionalWeakTable<DependencyObject, object> _sizeOriginals = new();
	private static bool _hooked;

	// text that shows a font of its own on purpose (the font list, its preview) and is left alone
	private static readonly ConditionalWeakTable<DependencyObject, object> _keep = new();

	// the text color, over View > Program background's own
	private static ResourceDictionary _colorStyles;

	// a window's text size when it doesn't set one
	private const double WindowFontSize = 12;

	/// <summary>The chosen font, or null for WorldForge's own.</summary>
	public static FontFamily Font { get; private set; }

	/// <summary>How large text is drawn, 1 for its own size.</summary>
	public static double Scale { get; private set; } = 1;

	/// <summary>The chosen text color, or null for WorldForge's own (or the one Program background picks).</summary>
	public static Color? TextColor { get; private set; }

	public static void LoadSaved()
	{
		try
		{
			if (!File.Exists(_path))
				return;

			FontFamily font = null;
			double scale = 1;
			Color? color = null;

			foreach (var line in File.ReadAllLines(_path))
			{
				var parts = line.Split('=', 2);

				if (parts.Length != 2 || parts[1].Trim() is not { Length: > 0 } value)
					continue;

				switch (parts[0].Trim())
				{
					case "font":
						font = new FontFamily(value);
						break;
					case "scale" when Double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var s) && s is >= 0.5 and <= 3:
						scale = s;
						break;
					case "color":
						try { color = (Color)ColorConverter.ConvertFromString(value); } catch (FormatException) { }
						break;
				}
			}

			if (font is not null || scale != 1 || color is not null)
				Apply(font, scale, color, save: false);
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			// a font file that can't be read leaves the usual font
		}
	}

	/// <summary>Sets the font, size and color on everything open, and on whatever opens later.</summary>
	public static void Apply(FontFamily font, double scale, Color? color, bool save = true)
	{
		var colorChanged = color != TextColor;

		Font = font;
		Scale = scale;
		TextColor = color;

		if (!_hooked)
		{
			// text that appears later (a new tab, a dialog, a list's rows) gets the font as it loads
			foreach (var type in new[] { typeof(Window), typeof(Control), typeof(TextBlock) })
				EventManager.RegisterClassHandler(type, FrameworkElement.LoadedEvent, new RoutedEventHandler((sender, _) => Set(sender as DependencyObject)), true);

			_hooked = true;
		}

		foreach (var window in Application.Current.Windows.OfType<Window>())
			SetAll(window);

		// the color goes over Program background's; taking it away lets that one's come back
		if (colorChanged)
		{
			if (color is null)
			{
				ReapplyColor();
				UncolorWindows();
				ProgramTheme.Apply(ProgramTheme.Background, save: false);
			}
			else
			{
				ReapplyColor();
			}
		}

		if (save)
			Save();
	}

	/// <summary>
	/// Puts the text color's styles last among the application's resources, over Program background's; Program
	/// background calls this after it changes, so the text color stays on top.
	/// </summary>
	internal static void ReapplyColor()
	{
		var resources = Application.Current.Resources;

		if (_colorStyles is not null)
			resources.MergedDictionaries.Remove(_colorStyles);

		_colorStyles = null;

		if (TextColor is not { } color)
			return;

		var brush = new SolidColorBrush(color);
		brush.Freeze();

		_colorStyles = new ResourceDictionary();

		// each keeps the style it has now (Program background's, if one is chosen), with the text color on top
		foreach (var type in new[] { typeof(TreeView), typeof(ListBox), typeof(ListView), typeof(DataGrid), typeof(PropertyGrid), typeof(StatusBar),
			         typeof(ToolBar), typeof(Button), typeof(TabControl), typeof(TabItem), typeof(Expander), typeof(GroupBox), typeof(Menu),
			         typeof(ComboBoxItem), typeof(CheckBox), typeof(RadioButton), typeof(Label) })
		{
			var style = new Style(type, Application.Current.TryFindResource(type) as Style);
			style.Setters.Add(new Setter(Control.ForegroundProperty, brush));
			_colorStyles[type] = style;
		}

		_colorStyles[SystemColors.WindowTextBrushKey] = brush;
		_colorStyles[SystemColors.ControlTextBrushKey] = brush;
		_colorStyles[SystemColors.MenuTextBrushKey] = brush;
		_colorStyles["WorldForgeBarTextBrush"] = brush;

		resources.MergedDictionaries.Add(_colorStyles);
		resources["WorldForgeBarTextBrush"] = brush;

		foreach (var window in Application.Current.Windows.OfType<Window>())
			ColorWindow(window, brush);
	}

	// a window's own text color from before any was set here, so Default (and Program background) can put it back
	private static readonly ConditionalWeakTable<Window, object> _foregroundOriginals = new();

	/// <summary>The window's text color before View > Font changed it, when it did.</summary>
	internal static bool TryGetOriginalForeground(Window window, out object original) => _foregroundOriginals.TryGetValue(window, out original);

	private static void ColorWindow(Window window, Brush brush)
	{
		if (!_foregroundOriginals.TryGetValue(window, out _))
			_foregroundOriginals.Add(window, window.ReadLocalValue(Control.ForegroundProperty));

		window.Foreground = brush;
	}

	private static void UncolorWindows()
	{
		foreach (var window in Application.Current.Windows.OfType<Window>())
		{
			if (!_foregroundOriginals.TryGetValue(window, out var original))
				continue;

			if (original == DependencyProperty.UnsetValue)
				window.ClearValue(Control.ForegroundProperty);
			else
				window.Foreground = (Brush)original;

			_foregroundOriginals.Remove(window);
		}
	}

	private static void SetAll(DependencyObject root)
	{
		Set(root);

		for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
			SetAll(VisualTreeHelper.GetChild(root, i));
	}

	private static void Set(DependencyObject element)
	{
		if (element is null || _keep.TryGetValue(element, out _))
			return;

		var (family, size) = element switch
		{
			Control => (Control.FontFamilyProperty, Control.FontSizeProperty),
			TextBlock => (TextBlock.FontFamilyProperty, TextBlock.FontSizeProperty),
			_ => (null, null),
		};

		if (family is null)
			return;

		SetFamily(element, family);
		SetSize(element, size);

		// a window opened later takes the text color too
		if (element is Window window && TextColor is { } color)
			ColorWindow(window, new SolidColorBrush(color));
	}

	/// <summary>Windows always take the font; inside them, only text that named a font of its own.</summary>
	private static void SetFamily(DependencyObject element, DependencyProperty property)
	{
		var changedBefore = _familyOriginals.TryGetValue(element, out var original);

		if (Font is null)
		{
			if (changedBefore)
				Restore(element, property, original, _familyOriginals);

			return;
		}

		var local = element.ReadLocalValue(property);

		if (element is not Window && local == DependencyProperty.UnsetValue && !changedBefore)
			return;

		if (!changedBefore)
			_familyOriginals.Add(element, local);

		element.SetValue(property, Font);
	}

	/// <summary>Windows always take the size; inside them, only text that set a size of its own, scaled from it.</summary>
	private static void SetSize(DependencyObject element, DependencyProperty property)
	{
		var changedBefore = _sizeOriginals.TryGetValue(element, out var original);

		if (Scale == 1)
		{
			if (changedBefore)
				Restore(element, property, original, _sizeOriginals);

			return;
		}

		if (!changedBefore)
		{
			var local = element.ReadLocalValue(property);

			if (element is not Window && local == DependencyProperty.UnsetValue)
				return;

			original = local;
			_sizeOriginals.Add(element, local);
		}

		var baseSize = original is double own ? own : WindowFontSize;
		element.SetValue(property, Math.Round(baseSize * Scale, 1));
	}

	private static void Restore(DependencyObject element, DependencyProperty property, object original, ConditionalWeakTable<DependencyObject, object> table)
	{
		if (original == DependencyProperty.UnsetValue)
			element.ClearValue(property);
		else
			element.SetValue(property, original);

		table.Remove(element);
	}

	private static void Save()
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
			File.WriteAllLines(_path,
			[
				$"font={Font?.Source ?? String.Empty}",
				$"scale={Scale.ToString(CultureInfo.InvariantCulture)}",
				$"color={(TextColor is { } color ? color.ToString() : String.Empty)}",
			]);
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			// the font just isn't remembered
		}
	}

	/// <summary>
	/// A window listing the installed fonts, each in its own look, with a search box, the text size and color, and a
	/// live preview. OK keeps them, Default goes back to WorldForge's own, Cancel to what they were.
	/// </summary>
	public static void Pick(Window owner)
	{
		var (beforeFont, beforeScale, beforeColor) = (Font, Scale, TextColor);
		var names = Fonts.SystemFontFamilies.Select(f => f.Source).Distinct().OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();

		var search = new TextBox { Margin = new Thickness(0, 0, 0, 6), ToolTip = "Type part of a font's name" };
		var list = new ListBox { Height = 280 };
		var preview = new TextBlock
		{
			Text = "Region 26  Spawnerguy  Honeycomb Flagstone  0123456789",
			FontSize = 16,
			Margin = new Thickness(0, 8, 0, 8),
			TextWrapping = TextWrapping.Wrap,
		};
		_keep.Add(preview, true);

		var sizes = new ComboBox { Width = 90, ToolTip = "How large all text is drawn" };
		foreach (var percent in new[] { 80, 90, 100, 110, 125, 150, 175, 200 })
			sizes.Items.Add(new ComboBoxItem { Content = $"{percent}%", Tag = percent / 100.0 });
		sizes.SelectedItem = sizes.Items.OfType<ComboBoxItem>().FirstOrDefault(i => Math.Abs((double)i.Tag - Scale) < 0.001) ?? sizes.Items[2];

		var chosenColor = TextColor;
		var colors = new ComboBox { Width = 150, Margin = new Thickness(6, 0, 0, 0), ToolTip = "The color of all text" };
		foreach (var (label, value) in new (string, string)[] { ("Default", ""), ("Black", "#FF000000"), ("White", "#FFFFFFFF"), ("Light gray", "#FFD0D0D0"),
			         ("Gold", "#FFFFD700"), ("Light blue", "#FF9CCCFF"), ("Light green", "#FFA8E6A0"), ("Dark blue", "#FF1A2F6B"), ("Custom...", "Custom") })
			colors.Items.Add(new ComboBoxItem { Content = label, Tag = value });
		colors.SelectedItem = colors.Items.OfType<ComboBoxItem>().FirstOrDefault(i => TextColor is { } c ? (string)i.Tag == c.ToString() : (string)i.Tag == "")
		                      ?? colors.Items.OfType<ComboBoxItem>().Last();

		var sizeRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
		sizeRow.Children.Add(new TextBlock { Text = "Size", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) });
		sizeRow.Children.Add(sizes);
		sizeRow.Children.Add(new TextBlock { Text = "Color", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0) });
		sizeRow.Children.Add(colors);

		FontFamily SelectedFont() => list.SelectedItem is ListBoxItem { Tag: string name } ? new FontFamily(name) : Font;
		double SelectedScale() => sizes.SelectedItem is ComboBoxItem { Tag: double scale } ? scale : 1;

		void Preview()
		{
			preview.FontFamily = SelectedFont() ?? new FontFamily("Segoe UI");
			preview.FontSize = 16 * SelectedScale();
			Apply(SelectedFont(), SelectedScale(), chosenColor, save: false);
		}

		void Fill()
		{
			list.Items.Clear();

			foreach (var name in names.Where(n => search.Text.Length == 0 || n.Contains(search.Text, StringComparison.OrdinalIgnoreCase)))
			{
				// each name in its own font, so the list shows what it looks like
				var item = new ListBoxItem { Content = name, Tag = name };
				item.SetValue(Control.FontFamilyProperty, new FontFamily(name));
				_keep.Add(item, true);
				list.Items.Add(item);
			}
		}

		var window = new Window
		{
			Title = "Font",
			Owner = owner,
			Width = 400,
			SizeToContent = SizeToContent.Height,
			ResizeMode = ResizeMode.NoResize,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			ShowInTaskbar = false,
		};

		search.TextChanged += (_, _) => Fill();
		list.SelectionChanged += (_, _) => Preview();
		sizes.SelectionChanged += (_, _) => Preview();
		colors.SelectionChanged += (_, _) =>
		{
			if (colors.SelectedItem is not ComboBoxItem { Tag: string value })
				return;

			if (value == "Custom")
				chosenColor = PaletteBackgroundPicker.PickColor(window, "Text color", chosenColor ?? Colors.Black) ?? chosenColor;
			else
				chosenColor = value.Length == 0 ? null : (Color)ColorConverter.ConvertFromString(value);

			Preview();
		};

		var ok = new Button { Content = "OK", Width = 80, IsDefault = true, Margin = new Thickness(0, 0, 6, 0) };
		var useDefault = new Button { Content = "Default", Width = 80, Margin = new Thickness(0, 0, 6, 0), ToolTip = "WorldForge's own font, size and color" };
		var cancel = new Button { Content = "Cancel", Width = 80, IsCancel = true };
		var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
		buttons.Children.Add(ok);
		buttons.Children.Add(useDefault);
		buttons.Children.Add(cancel);

		var layout = new StackPanel { Margin = new Thickness(10) };
		layout.Children.Add(search);
		layout.Children.Add(list);
		layout.Children.Add(sizeRow);
		layout.Children.Add(preview);
		layout.Children.Add(buttons);
		window.Content = layout;

		var result = "cancel";
		ok.Click += (_, _) => { result = "ok"; window.Close(); };
		useDefault.Click += (_, _) => { result = "default"; window.Close(); };

		Fill();

		if (Font is not null)
			list.SelectedItem = list.Items.OfType<ListBoxItem>().FirstOrDefault(i => (string)i.Tag == Font.Source);

		search.Focus();
		window.ShowDialog();

		switch (result)
		{
			case "ok":
				Apply(SelectedFont(), SelectedScale(), chosenColor);
				break;
			case "default":
				Apply(null, 1, null);
				break;
			default:
				Apply(beforeFont, beforeScale, beforeColor, save: false);
				break;
		}
	}
}
