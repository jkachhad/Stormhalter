using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.UI.Windows;

/// <summary>
/// The saved stamps, with a preview of the picked one; it can be pasted with its top left on the tile the window was
/// opened from, put on the clipboard, or deleted.
/// </summary>
public class StampsWindow : Window
{
	private readonly ListBox _list;
	private readonly Image _preview;
	private readonly TextBlock _size;
	private readonly Func<TileBlock, bool> _paste;

	private TileBlock Block { get; set; }

	public StampsWindow(string target, Func<TileBlock, bool> paste)
	{
		_paste = paste;

		Title = "Stamps";
		Width = 620;
		Height = 460;
		WindowStartupLocation = WindowStartupLocation.CenterOwner;
		ShowInTaskbar = false;

		var layout = new DockPanel { Margin = new Thickness(10) };

		var buttons = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
		DockPanel.SetDock(buttons, Dock.Bottom);
		layout.Children.Add(buttons);

		Button Add(string text, string tip, Action click)
		{
			var button = new Button { Content = text, ToolTip = tip, Padding = new Thickness(8, 2, 8, 2), Margin = new Thickness(0, 0, 6, 4) };
			button.Click += (_, _) => click();
			buttons.Children.Add(button);
			return button;
		}

		Add($"Paste at {target}", "Sets the stamp's tiles with its top left on this tile (one undo)", () =>
		{
			if (Block is not null && _paste(Block))
				Close();
		});
		Add("Copy", "Puts the stamp on the clipboard, for Ctrl+V", () => Block?.ToClipboard());
		Add("Delete", "Deletes the saved stamp", Delete);

		_list = new ListBox { Width = 200, Margin = new Thickness(0, 0, 10, 0) };
		_list.SelectionChanged += (_, _) => Pick();
		DockPanel.SetDock(_list, Dock.Left);
		layout.Children.Add(_list);

		_size = new TextBlock { Margin = new Thickness(0, 0, 0, 4) };
		DockPanel.SetDock(_size, Dock.Top);

		var right = new DockPanel();
		right.Children.Add(_size);

		_preview = new Image { Stretch = Stretch.Uniform, StretchDirection = StretchDirection.Both };
		RenderOptions.SetBitmapScalingMode(_preview, BitmapScalingMode.NearestNeighbor);
		right.Children.Add(new Border { Background = Brushes.Black, Child = _preview });

		layout.Children.Add(right);

		Content = layout;

		Refresh();
	}


	private void Display(TileBlock block)
	{
		Block = block;

		if (block is null)
		{
			_preview.Source = null;
			_size.Text = "";
			return;
		}

		_size.Text = $"{block.Width} x {block.Height} tiles";
		_preview.Source = RegionPreview.Render(block.ToRegion());
	}

	private void Refresh()
	{
		_list.Items.Clear();

		foreach (var (name, path) in Stamps.List())
			_list.Items.Add(new ListBoxItem { Content = name, Tag = path });

		if (_list.Items.Count > 0)
			_list.SelectedIndex = 0;
		else
			Display(null);
	}

	private void Pick()
	{
		if ((_list.SelectedItem as ListBoxItem)?.Tag is not string path)
			return;

		try
		{
			Display(Stamps.Load(path));
		}
		catch (Exception exception)
		{
			Display(null);
			_size.Text = $"This stamp couldn't be read: {exception.Message}";
		}
	}

	private void Delete()
	{
		if ((_list.SelectedItem as ListBoxItem) is not { Tag: string path } item)
			return;

		if (MessageBox.Show(this, $"Delete the stamp \"{item.Content}\"?", Title, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
			return;

		File.Delete(path);
		Refresh();
	}
}
