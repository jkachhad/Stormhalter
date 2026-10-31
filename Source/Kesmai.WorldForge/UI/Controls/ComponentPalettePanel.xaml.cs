using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.UI;

/// <summary>
/// The region editor's component palette: the categories, their tiles laid out as a grid or as rows that scroll left to
/// right, at a chosen size, and a button that floats it in its own window or docks it again.
/// </summary>
public partial class ComponentPalettePanel : UserControl
{
	/// <summary>Raised by the Float / Dock button; the region editor moves the panel.</summary>
	public event EventHandler FloatToggled;

	private bool _loading;

	public ComponentPalettePanel()
	{
		InitializeComponent();

		_loading = true;
		_layout.SelectedItem = _layout.Items.OfType<ComboBoxItem>().FirstOrDefault(i => (string)i.Tag == ComponentPaletteSettings.Rows.ToString(CultureInfo.InvariantCulture)) ?? _layout.Items[0];
		_size.SelectedItem = _size.Items.OfType<ComboBoxItem>().FirstOrDefault(i => Math.Abs(Double.Parse((string)i.Tag, CultureInfo.InvariantCulture) - ComponentPaletteSettings.Scale) < 0.01) ?? _size.Items[^1];
		_loading = false;

		_grid.Rows = ComponentPaletteSettings.Rows;
		_grid.TileScale = ComponentPaletteSettings.Scale;

		ShowFloating(false);
		ShowCategoryMode();

		// the top categories arrive as the palette loads, and change with the segment's own components
		var palette = ServiceLocator.Current.GetInstance<ComponentPalette>();
		palette.Categories.CollectionChanged += (_, _) => Dispatcher.BeginInvoke(BuildCategoryButtons);
		BuildCategoryButtons();
	}

	private void BuildCategoryButtons()
	{
		var palette = ServiceLocator.Current.GetInstance<ComponentPalette>();

		_categoryButtons.Children.Clear();

		foreach (var category in palette.Categories.ToList())
		{
			var button = new Button
			{
				Content = (category.Subcategories?.Count ?? 0) > 0 ? $"{category.Name} ▾" : category.Name,
				FontWeight = FontWeights.Bold,
				FontSize = 12,
				Padding = new Thickness(10, 3, 10, 3),
				Margin = new Thickness(0, 0, 4, 4),
				ToolTip = (category.Subcategories?.Count ?? 0) > 0 ? $"Pick a folder in {category.Name}" : $"Show the {category.Name} tiles",
			};

			button.Click += (_, _) =>
			{
				if ((category.Subcategories?.Count ?? 0) == 0)
					Select(category);
				else
					GridMenu.Show(button, category.Name, Entries(category));
			};

			_categoryButtons.Children.Add(button);
		}
	}

	/// <summary>A category's tiles first (when it has its own), then its folders.</summary>
	private IReadOnlyList<GridMenu.Entry> Entries(ComponentsCategory category)
	{
		var entries = new List<GridMenu.Entry>();

		if ((category.Components?.Count ?? 0) > 0)
			entries.Add(new GridMenu.Entry { Name = $"All of {category.Name}", Open = () => Select(category) });

		foreach (var subcategory in category.Subcategories ?? [])
		{
			var hasFolders = (subcategory.Subcategories?.Count ?? 0) > 0;

			entries.Add(new GridMenu.Entry
			{
				Name = subcategory.Name,
				// a folder of folders opens into them; one with only tiles shows them
				Open = hasFolders && (subcategory.Components?.Count ?? 0) == 0 ? null : () => Select(subcategory),
				Children = hasFolders ? () => Entries(subcategory) : null,
			});
		}

		return entries;
	}

	/// <summary>Shows a category's tiles, and picks it in the list too.</summary>
	private static void Select(ComponentsCategory category)
	{
		var palette = ServiceLocator.Current.GetInstance<ComponentPalette>();

		palette.SelectedCategory = category;
		category.IsSelected = true;
	}

	/// <summary>Updates the button for where the panel is: floating in its own window, or docked beside the map.</summary>
	public void ShowFloating(bool floating)
	{
		_float.Content = floating ? "Dock" : "Float";
		_float.ToolTip = floating
			? "Put the palette back beside the map"
			: "Pop the palette out into its own window you can drag anywhere and resize";
	}

	private void OnLayoutChanged(object sender, SelectionChangedEventArgs args)
	{
		if (_loading || _layout.SelectedItem is not ComboBoxItem { Tag: string tag })
			return;

		_grid.Rows = Int32.Parse(tag, CultureInfo.InvariantCulture);
		ComponentPaletteSettings.Rows = _grid.Rows;
		ComponentPaletteSettings.Save();
	}

	private void OnSizeChanged(object sender, SelectionChangedEventArgs args)
	{
		if (_loading || _size.SelectedItem is not ComboBoxItem { Tag: string tag })
			return;

		_grid.TileScale = Double.Parse(tag, CultureInfo.InvariantCulture);
		ComponentPaletteSettings.Scale = _grid.TileScale;
		ComponentPaletteSettings.Save();
	}

	private void OnFloatClicked(object sender, RoutedEventArgs args) => FloatToggled?.Invoke(this, EventArgs.Empty);

	// the categories as buttons on one line (with grid menus), or as the list; one or the other, to save room
	private GridLength _listHeight = new GridLength(250);

	private void OnCategoryModeClicked(object sender, RoutedEventArgs args)
	{
		ComponentPaletteSettings.CategoryButtons = !ComponentPaletteSettings.CategoryButtons;
		ComponentPaletteSettings.Save();
		ShowCategoryMode();
	}

	private void ShowCategoryMode()
	{
		var buttons = ComponentPaletteSettings.CategoryButtons;

		if (buttons && _categoriesRow.Height.Value > 0)
			_listHeight = _categoriesRow.Height;

		_categoryButtons.Visibility = buttons ? Visibility.Visible : Visibility.Collapsed;
		_categoryList.Visibility = buttons ? Visibility.Collapsed : Visibility.Visible;
		_categorySplitter.Visibility = buttons ? Visibility.Collapsed : Visibility.Visible;
		_categoriesRow.MinHeight = buttons ? 0 : 60;
		_categoriesRow.Height = buttons ? new GridLength(0) : _listHeight;

		_categoryMode.Content = buttons ? "List" : "Buttons";
		_categoryMode.ToolTip = buttons
			? "Show the categories as a list instead of buttons"
			: "Show the categories as buttons on one line instead of a list, to save room";
	}

	private void OnCategorySelected(object sender, RoutedPropertyChangedEventArgs<object> args)
	{
		var componentPalette = ServiceLocator.Current.GetInstance<ComponentPalette>();

		if (args.NewValue is ComponentsCategory category)
			componentPalette.SelectedCategory = category;
	}
}

/// <summary>The palette's layout, size and floating window, kept in .storage so they last between sessions.</summary>
public static class ComponentPaletteSettings
{
	private static readonly string _path = Path.Combine(".storage", "ComponentPalette.cfg");

	public static int Rows { get; set; }
	public static double Scale { get; set; } = 1.0;
	public static bool Floating { get; set; }
	public static double DockWidth { get; set; } = 375;

	/// <summary>The categories show as buttons (true) or as the list (false).</summary>
	public static bool CategoryButtons { get; set; } = true;
	public static Rect FloatBounds { get; set; } = Rect.Empty;

	static ComponentPaletteSettings()
	{
		try
		{
			if (!File.Exists(_path))
				return;

			foreach (var line in File.ReadAllLines(_path))
			{
				var parts = line.Split('=', 2);

				if (parts.Length != 2)
					continue;

				var value = parts[1].Trim();

				switch (parts[0].Trim())
				{
					case "rows" when Int32.TryParse(value, out var rows) && rows is >= 0 and <= 4:
						Rows = rows;
						break;
					case "scale" when Double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var scale) && scale is >= 0.3 and <= 2:
						Scale = scale;
						break;
					case "dockWidth" when Double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var width) && width is >= 150 and <= 3000:
						DockWidth = width;
						break;
					case "categories":
						CategoryButtons = value != "list";
						break;
					case "floating":
						Floating = value == "true";
						break;
					case "float":
						var numbers = value.Split(',').Select(n => Double.TryParse(n, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : Double.NaN).ToArray();

						if (numbers.Length == 4 && numbers.All(n => !Double.IsNaN(n)) && numbers[2] > 100 && numbers[3] > 100)
							FloatBounds = new Rect(numbers[0], numbers[1], numbers[2], numbers[3]);
						break;
				}
			}
		}
		catch (IOException)
		{
			// a settings file that can't be read leaves the defaults
		}
	}

	public static void Save()
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

			var bounds = FloatBounds.IsEmpty
				? String.Empty
				: String.Join(",", new[] { FloatBounds.X, FloatBounds.Y, FloatBounds.Width, FloatBounds.Height }.Select(n => n.ToString(CultureInfo.InvariantCulture)));

			File.WriteAllLines(_path,
			[
				$"rows={Rows}",
				$"scale={Scale.ToString(CultureInfo.InvariantCulture)}",
				$"dockWidth={DockWidth.ToString(CultureInfo.InvariantCulture)}",
				$"categories={(CategoryButtons ? "buttons" : "list")}",
				$"floating={(Floating ? "true" : "false")}",
				$"float={bounds}",
			]);
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			// the layout just isn't remembered
		}
	}
}
