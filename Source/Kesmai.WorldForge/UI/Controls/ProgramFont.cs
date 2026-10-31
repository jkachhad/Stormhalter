using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Kesmai.WorldForge.UI;

/// <summary>
/// View > Font: one font for all of WorldForge's text. Windows pass it down to their text, and text that names its own
/// font (much of WorldForge uses Tahoma) is switched too, as it appears; Default puts every one back. The font is kept
/// in .storage and applied at startup.
/// </summary>
public static class ProgramFont
{
	private static readonly string _path = Path.Combine(".storage", "Font.cfg");

	// what each element had before, so Default can put it back
	private static readonly ConditionalWeakTable<DependencyObject, object> _originals = new();
	private static bool _hooked;

	// text that shows a font of its own on purpose (the font list, its preview) and is left alone
	private static readonly ConditionalWeakTable<DependencyObject, object> _keep = new();

	/// <summary>The chosen font, or null for WorldForge's own.</summary>
	public static FontFamily Font { get; private set; }

	public static void LoadSaved()
	{
		try
		{
			if (File.Exists(_path) && File.ReadAllLines(_path).FirstOrDefault(l => l.StartsWith("font=")) is { } line
			    && line.Substring("font=".Length).Trim() is { Length: > 0 } name)
				Apply(new FontFamily(name), save: false);
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			// a font file that can't be read leaves the usual font
		}
	}

	/// <summary>Sets the font (null for WorldForge's own) on everything open, and on whatever opens later.</summary>
	public static void Apply(FontFamily font, bool save = true)
	{
		Font = font;

		if (!_hooked)
		{
			// text that appears later (a new tab, a dialog, a list's rows) gets the font as it loads
			foreach (var type in new[] { typeof(Window), typeof(Control), typeof(TextBlock) })
				EventManager.RegisterClassHandler(type, FrameworkElement.LoadedEvent, new RoutedEventHandler((sender, _) => Set(sender as DependencyObject)), true);

			_hooked = true;
		}

		foreach (var window in Application.Current.Windows.OfType<Window>())
			SetAll(window);

		if (save)
			Save();
	}

	private static void SetAll(DependencyObject root)
	{
		Set(root);

		for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
			SetAll(VisualTreeHelper.GetChild(root, i));
	}

	/// <summary>
	/// Gives a window the font, and any element that names its own font too; others take it from their window. With
	/// no font chosen, an element changed earlier gets its own back.
	/// </summary>
	private static void Set(DependencyObject element)
	{
		var property = element switch
		{
			Control => Control.FontFamilyProperty,
			TextBlock => TextBlock.FontFamilyProperty,
			_ => null,
		};

		if (property is null || _keep.TryGetValue(element, out _))
			return;

		var local = element.ReadLocalValue(property);
		var changedBefore = _originals.TryGetValue(element, out var original);

		if (Font is null)
		{
			if (!changedBefore)
				return;

			if (original == DependencyProperty.UnsetValue)
				element.ClearValue(property);
			else
				element.SetValue(property, original);

			_originals.Remove(element);
			return;
		}

		// windows always take it; inside them, only text that named a font of its own
		if (element is not Window && local == DependencyProperty.UnsetValue && !changedBefore)
			return;

		if (!changedBefore)
			_originals.Add(element, local);

		element.SetValue(property, Font);
	}

	private static void Save()
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
			File.WriteAllLines(_path, [$"font={Font?.Source ?? String.Empty}"]);
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			// the font just isn't remembered
		}
	}

	/// <summary>A window listing the installed fonts, each in its own look, with a search box and a live preview.</summary>
	public static void Pick(Window owner)
	{
		var before = Font;
		var names = Fonts.SystemFontFamilies.Select(f => f.Source).Distinct().OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();

		var search = new TextBox { Margin = new Thickness(0, 0, 0, 6), ToolTip = "Type part of a font's name" };
		var list = new ListBox { Height = 320 };
		var preview = new TextBlock
		{
			Text = "Region 26  Spawnerguy  Honeycomb Flagstone  0123456789",
			FontSize = 16,
			Margin = new Thickness(0, 8, 0, 8),
			TextWrapping = TextWrapping.Wrap,
		};
		_keep.Add(preview, true);

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

		search.TextChanged += (_, _) => Fill();
		list.SelectionChanged += (_, _) =>
		{
			if (list.SelectedItem is ListBoxItem { Tag: string name })
			{
				preview.FontFamily = new FontFamily(name);
				Apply(new FontFamily(name), save: false);
			}
		};

		var ok = new Button { Content = "OK", Width = 80, IsDefault = true, Margin = new Thickness(0, 0, 6, 0) };
		var useDefault = new Button { Content = "Default", Width = 80, Margin = new Thickness(0, 0, 6, 0), ToolTip = "WorldForge's own fonts" };
		var cancel = new Button { Content = "Cancel", Width = 80, IsCancel = true };
		var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
		buttons.Children.Add(ok);
		buttons.Children.Add(useDefault);
		buttons.Children.Add(cancel);

		var layout = new StackPanel { Margin = new Thickness(10) };
		layout.Children.Add(search);
		layout.Children.Add(list);
		layout.Children.Add(preview);
		layout.Children.Add(buttons);

		var window = new Window
		{
			Title = "Font",
			Content = layout,
			Owner = owner,
			Width = 380,
			SizeToContent = SizeToContent.Height,
			ResizeMode = ResizeMode.NoResize,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			ShowInTaskbar = false,
		};

		var result = "cancel";
		ok.Click += (_, _) => { result = "ok"; window.Close(); };
		useDefault.Click += (_, _) => { result = "default"; window.Close(); };

		Fill();
		search.Focus();
		window.ShowDialog();

		switch (result)
		{
			case "ok" when list.SelectedItem is ListBoxItem { Tag: string name }:
				Apply(new FontFamily(name));
				break;
			case "default":
				Apply(null);
				break;
			default:
				Apply(before, save: false);
				break;
		}
	}
}
