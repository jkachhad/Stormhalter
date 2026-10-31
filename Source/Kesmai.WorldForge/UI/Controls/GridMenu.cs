using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Kesmai.WorldForge.UI;

/// <summary>
/// A pop-up of large tiles for a folder's contents. A tile opens what it stands for; a folder's tile (or the arrow on a
/// tile that is both) goes into the folder, and Back comes out again. A filter box narrows the tiles shown by name.
/// </summary>
public static class GridMenu
{
	/// <summary>One tile: its name and look, what clicking it opens (null for a plain folder), and its folder contents.</summary>
	public sealed class Entry
	{
		public string Name { get; init; } = String.Empty;
		public ImageSource Icon { get; init; }
		public Brush Swatch { get; init; }
		public Action Open { get; init; }
		public Func<IReadOnlyList<Entry>> Children { get; init; }

		/// <summary>A picture drawn on the tile instead of the icon (a region's map), made as the tile is shown.</summary>
		public Func<ImageSource> Preview { get; init; }

		/// <summary>The preview is a map (pixels are tiles, drawn sharp on black) rather than a picture.</summary>
		public bool PreviewIsMap { get; init; }

		/// <summary>A symbol drawn on the tile when it has no icon (a speaker for a sound).</summary>
		public string Glyph { get; init; }

		/// <summary>Clicking the tile leaves the menu open (playing a sound), instead of closing it.</summary>
		public bool KeepOpen { get; init; }

		/// <summary>The tile's tooltip, instead of "Open …".</summary>
		public string ToolTip { get; init; }

		public bool IsFolder => Children is not null && Children().Count > 0;
	}

	public static void Show(FrameworkElement anchor, string title, IReadOnlyList<Entry> entries)
	{
		var trail = new Stack<(string Title, IReadOnlyList<Entry> Entries)>();
		var popup = new Popup
		{
			PlacementTarget = anchor,
			Placement = PlacementMode.Bottom,
			StaysOpen = false,
			AllowsTransparency = true,
			PopupAnimation = PopupAnimation.Fade,
		};

		var titleText = new TextBlock { FontWeight = FontWeights.Bold, FontSize = 13, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 6, 0) };
		var back = new Button { Content = "◀ Back", Padding = new Thickness(8, 2, 8, 2), FontSize = 12 };
		var filter = new TextBox { Width = 160, FontSize = 12, VerticalContentAlignment = VerticalAlignment.Center, ToolTip = "Type to show only tiles whose name has this in it" };
		var tiles = new WrapPanel { Margin = new Thickness(4) };
		var empty = new TextBlock { Text = "Nothing here.", Foreground = Brushes.DimGray, Margin = new Thickness(8), Visibility = Visibility.Collapsed };

		var header = new DockPanel { Margin = new Thickness(6, 6, 6, 2), LastChildFill = true };
		DockPanel.SetDock(back, Dock.Left);
		DockPanel.SetDock(filter, Dock.Right);
		header.Children.Add(back);
		header.Children.Add(filter);
		header.Children.Add(titleText);

		var body = new DockPanel();
		DockPanel.SetDock(header, Dock.Top);
		DockPanel.SetDock(empty, Dock.Top);
		body.Children.Add(header);
		body.Children.Add(empty);
		body.Children.Add(new ScrollViewer { Content = tiles, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, MaxHeight = 520 });

		var frame = new Border
		{
			BorderBrush = Brushes.Gray,
			BorderThickness = new Thickness(1),
			Width = 660,
			Child = body,
		};

		// the bar color and its text, so View > Program background reaches the pop-up too
		frame.SetResourceReference(Border.BackgroundProperty, "WorldForgeBarBrush");
		frame.SetResourceReference(TextElement.ForegroundProperty, "WorldForgeBarTextBrush");
		popup.Child = frame;

		var current = (Title: title, Entries: entries);

		void Fill()
		{
			titleText.Text = current.Title;
			back.Visibility = trail.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
			tiles.Children.Clear();

			var shown = current.Entries
				.Where(e => filter.Text.Length == 0 || e.Name.Contains(filter.Text, StringComparison.OrdinalIgnoreCase))
				.ToList();

			foreach (var entry in shown)
				tiles.Children.Add(Tile(entry));

			empty.Visibility = shown.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
		}

		void Enter(Entry folder)
		{
			trail.Push(current);
			current = ($"{current.Title} › {folder.Name}", folder.Children!());
			filter.Text = String.Empty;
			Fill();
		}

		UIElement Tile(Entry entry)
		{
			var look = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
			var preview = entry.Preview?.Invoke();

			if (preview is not null)
			{
				var picture = new Image { Source = preview, Width = 104, Height = 64, Stretch = Stretch.Uniform, Margin = new Thickness(0, 2, 0, 2) };

				// a map's pixels are tiles, kept sharp on black; a creature's picture is drawn as it is, no larger than itself
				if (entry.PreviewIsMap)
				{
					RenderOptions.SetBitmapScalingMode(picture, BitmapScalingMode.NearestNeighbor);
					look.Children.Add(new Border { Background = Brushes.Black, Child = picture });
				}
				else
				{
					picture.Stretch = Stretch.Uniform;
					picture.StretchDirection = StretchDirection.DownOnly;
					look.Children.Add(picture);
				}
			}
			else if (entry.Icon is not null)
				look.Children.Add(new Image { Source = entry.Icon, Width = 28, Height = 28, Margin = new Thickness(0, 2, 0, 2) });
			else if (entry.Swatch is not null)
				look.Children.Add(new Rectangle { Fill = entry.Swatch, Width = 16, Height = 16, Margin = new Thickness(0, 6, 0, 6) });
			else if (entry.Glyph is not null)
				look.Children.Add(new TextBlock { Text = entry.Glyph, FontSize = 20, HorizontalAlignment = HorizontalAlignment.Center });
			else if (entry.IsFolder)
				look.Children.Add(new TextBlock { Text = "📁", FontSize = 20, HorizontalAlignment = HorizontalAlignment.Center });

			look.Children.Add(new TextBlock
			{
				Text = entry.Name,
				FontSize = 12,
				TextWrapping = TextWrapping.Wrap,
				TextAlignment = TextAlignment.Center,
				MaxHeight = 34,
				TextTrimming = TextTrimming.CharacterEllipsis,
			});

			var main = new Button
			{
				Content = look,
				Width = 118,
				Height = preview is not null ? 104 : 74,
				Margin = new Thickness(3),
				ToolTip = entry.ToolTip ?? (entry.Open is not null ? $"Open {entry.Name}" : $"Show what's in {entry.Name}"),
			};

			main.Click += (_, _) =>
			{
				if (entry.Open is not null)
				{
					if (!entry.KeepOpen)
						popup.IsOpen = false;

					entry.Open();
				}
				else if (entry.IsFolder)
				{
					Enter(entry);
				}
			};

			// a tile that opens something and also holds more (a region with subregions) gets an arrow into it
			if (entry.Open is null || !entry.IsFolder)
				return main;

			var into = new Button
			{
				Content = "›",
				FontWeight = FontWeights.Bold,
				Width = 20,
				Height = 20,
				Padding = new Thickness(0),
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Top,
				Margin = new Thickness(0, 5, 5, 0),
				ToolTip = $"Show what's in {entry.Name}",
			};
			into.Click += (_, _) => Enter(entry);

			var both = new Grid();
			both.Children.Add(main);
			both.Children.Add(into);
			return both;
		}

		back.Click += (_, _) =>
		{
			if (trail.Count == 0)
				return;

			current = trail.Pop();
			filter.Text = String.Empty;
			Fill();
		};

		filter.TextChanged += (_, _) => Fill();

		popup.KeyDown += (_, args) =>
		{
			if (args.Key == Key.Escape)
			{
				popup.IsOpen = false;
				args.Handled = true;
			}
		};

		Fill();
		popup.IsOpen = true;
		filter.Focus();
	}
}
