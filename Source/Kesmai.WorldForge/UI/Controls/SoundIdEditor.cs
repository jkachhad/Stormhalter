#nullable enable

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Kesmai.WorldForge.Editor.Assets;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Kesmai.WorldForge.UI;

/// <summary>
/// Property grid editor for a sound id: a dropdown of the repo's sounds (id and name) that also takes a typed id, and a
/// ▶ button that plays the sound's file when the repo has it.
/// </summary>
public class SoundIdEditor : ITypeEditor
{
	private static readonly MediaPlayer _player = new();

	public FrameworkElement ResolveEditor(PropertyItem propertyItem)
	{
		var panel = new DockPanel();
		var play = new Button { Content = "▶", Width = 24, Margin = new Thickness(2, 0, 0, 0), ToolTip = "Play this sound" };
		var combo = new ComboBox { IsEditable = true, IsTextSearchEnabled = true };

		TextSearch.SetTextPath(combo, nameof(SoundAsset.Id));

		// each choice shows its id and name; picking one puts its id in the box
		var template = new DataTemplate();
		var text = new FrameworkElementFactory(typeof(TextBlock));
		text.SetBinding(TextBlock.TextProperty, new Binding(nameof(SoundAsset.Label)));
		template.VisualTree = text;
		combo.ItemTemplate = template;

		combo.SetBinding(ComboBox.TextProperty, new Binding(nameof(PropertyItem.Value))
		{
			Source = propertyItem,
			Mode = BindingMode.TwoWay,
			UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
		});

		combo.SelectionChanged += (_, _) =>
		{
			if (combo.SelectedItem is SoundAsset sound)
				propertyItem.Value = sound.Id;
		};

		var loading = AssetCatalog.LoadAsync();

		void Fill(AssetCatalog catalog) => combo.ItemsSource = catalog.Sounds;

		if (loading.IsCompletedSuccessfully)
			Fill(loading.Result);
		else
			loading.ContinueWith(t => { if (t.IsCompletedSuccessfully) combo.Dispatcher.Invoke(() => Fill(t.Result)); });

		play.Click += (_, _) =>
		{
			var id = propertyItem.Value is int value ? value : Int32.TryParse(combo.Text, out var typed) ? typed : -1;
			var sound = loading.IsCompletedSuccessfully ? loading.Result.Sounds.FirstOrDefault(s => s.Id == id) : null;

			if (sound?.FilePath is null)
			{
				play.ToolTip = $"No audio file for sound {id} in the repo, so it can't be played here.";
				return;
			}

			_player.Open(new Uri(sound.FilePath));
			_player.Play();
		};

		DockPanel.SetDock(play, Dock.Right);
		panel.Children.Add(play);
		panel.Children.Add(combo);

		return panel;
	}
}
