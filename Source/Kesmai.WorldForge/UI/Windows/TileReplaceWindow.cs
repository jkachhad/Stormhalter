using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.UI.Windows;

/// <summary>
/// Picks which component to find among the tiles and what to put in its place: the palette's selected tile,
/// another component already on the tiles, or nothing.
/// </summary>
public class TileReplaceWindow : Window
{
	private readonly ListBox _find;
	private readonly ComboBox _with;
	private readonly List<(string Key, string Name, int Count, IComponentProvider Sample)> _census;

	public string FindKey { get; private set; }
	public IComponentProvider Replacement { get; private set; }

	public TileReplaceWindow(string scope, List<(string Key, string Name, int Count, IComponentProvider Sample)> census, IComponentProvider paletteSelection)
	{
		_census = census;

		Title = "Find & replace tiles";
		Width = 460;
		Height = 520;
		WindowStartupLocation = WindowStartupLocation.CenterOwner;
		ShowInTaskbar = false;
		ResizeMode = ResizeMode.CanResizeWithGrip;

		var layout = new DockPanel { Margin = new Thickness(10) };

		var header = new TextBlock { Text = $"Find in {scope}:", Margin = new Thickness(0, 0, 0, 4) };
		DockPanel.SetDock(header, Dock.Top);
		layout.Children.Add(header);

		var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
		var replace = new Button { Content = "Replace", Width = 80, IsDefault = true, Margin = new Thickness(0, 0, 6, 0) };
		var cancel = new Button { Content = "Cancel", Width = 80, IsCancel = true };
		buttons.Children.Add(replace);
		buttons.Children.Add(cancel);
		DockPanel.SetDock(buttons, Dock.Bottom);
		layout.Children.Add(buttons);

		var withPanel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };
		withPanel.Children.Add(new TextBlock { Text = "Replace with:", Margin = new Thickness(0, 0, 0, 4) });

		_with = new ComboBox();
		_with.Items.Add(new ComboBoxItem { Content = "Nothing (take it out)", Tag = null });

		if (paletteSelection is not null)
			_with.Items.Add(new ComboBoxItem { Content = $"Palette selection: {TileReplace.Describe(paletteSelection)}", Tag = paletteSelection });

		foreach (var entry in census)
			_with.Items.Add(new ComboBoxItem { Content = $"{entry.Name}  (on these tiles)", Tag = entry.Sample });

		_with.SelectedIndex = paletteSelection is not null ? 1 : 0;
		withPanel.Children.Add(_with);
		DockPanel.SetDock(withPanel, Dock.Bottom);
		layout.Children.Add(withPanel);

		_find = new ListBox();

		foreach (var entry in census)
			_find.Items.Add(new ListBoxItem { Content = $"{entry.Name}   ×{entry.Count}", ToolTip = entry.Key });

		if (census.Count > 0)
			_find.SelectedIndex = 0;

		layout.Children.Add(_find);

		Content = layout;

		replace.Click += (_, _) =>
		{
			if (_find.SelectedIndex < 0)
				return;

			FindKey = _census[_find.SelectedIndex].Key;
			Replacement = (_with.SelectedItem as ComboBoxItem)?.Tag as IComponentProvider;

			// swapping a component for itself would change nothing
			if (Replacement is not null && TileReplace.Key(Replacement) == FindKey)
			{
				MessageBox.Show(this, "That is the same component.", Title);
				return;
			}

			DialogResult = true;
		};
	}
}
