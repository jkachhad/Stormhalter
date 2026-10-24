#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Kesmai.WorldForge.Editor.Assets;

namespace Kesmai.WorldForge.UI;

/// <summary>
/// Side panel for the OnSpawn script: dropdowns of server types, a body gallery, a hue picker with a tinted
/// preview, and the custom sound library. Each choice is applied to the script through <see cref="EditRequested"/>.
/// </summary>
public partial class OnSpawnAssetPanel : UserControl
{
	/// <summary>Raised with an edit to apply to the current script body.</summary>
	public event Action<Func<string, string>>? EditRequested;

	private AssetCatalog? _catalog;
	private readonly MediaPlayer _player = new();
	private int _optionRow;

	public OnSpawnAssetPanel()
	{
		InitializeComponent();

		Loaded += OnLoaded;
	}

	private async void OnLoaded(object sender, RoutedEventArgs e)
	{
		Loaded -= OnLoaded;

		_status.Text = "Loading server types, bodies and sounds…";

		_catalog = await AssetCatalog.LoadAsync();

		BuildOptions(_catalog);

		_bodies.ItemsSource = _catalog.Bodies;
		_sounds.ItemsSource = _catalog.Sounds;
		_hueNames.ItemsSource = _catalog.NamedColors;
		_huePicker.SelectedColor = Colors.White;

		_status.Text = _catalog.Warnings.Count > 0
			? String.Join(Environment.NewLine, _catalog.Warnings)
			: $"{_catalog.Creatures.Count} creatures, {_catalog.Bodies.Count} bodies, {_catalog.Sounds.Count} sounds.";

		UpdateHuePreview();
	}

	private void BuildOptions(AssetCatalog catalog)
	{
		AddOption("Creature class", catalog.Creatures, (body, value) => OnSpawnEdits.SetCreatureType(body, value));

		AddOption("Brain (AI)", catalog.Brains, (body, value) =>
		{
			var variable = OnSpawnEdits.GetVariable(body);
			return OnSpawnEdits.SetStatement(body, @"\w+\.Brain\s*=", $"{variable}.Brain = new {value}({variable});");
		});

		AddOption("Shield penetration", catalog.ShieldPenetration,
			(body, value) => OnSpawnEdits.SetInitializer(body, "BasePenetration", $"ShieldPenetration.{value}"));

		AddOption("Alignment", catalog.Alignments,
			(body, value) => OnSpawnEdits.SetInitializer(body, "Alignment", $"Alignment.{value}"));

		AddOption("Immunity", catalog.Immunities,
			(body, value) => OnSpawnEdits.SetInitializer(body, "Immunity", $"CreatureImmunity.{value}"));

		AddOption("Weakness", catalog.Weaknesses,
			(body, value) => OnSpawnEdits.SetInitializer(body, "Weakness", $"CreatureWeakness.{value}"));

		AddOption("Target strategy", catalog.SearchStrategies, (body, value) =>
		{
			var variable = OnSpawnEdits.GetVariable(body);
			var argument = value is "ClosestSearchStrategy" or "FarthestSearchStrategy" ? $"{variable}.Location" : String.Empty;

			return OnSpawnEdits.SetStatement(body, @"\w+\.CombatantSearchStrategy\s*=",
				$"{variable}.CombatantSearchStrategy = new {value}({argument});");
		});

		AddOption("Wield weapon", catalog.Weapons, Wield);
		AddOption("Wield shield", catalog.Shields, Wield);
		AddOption("Armor / robe", catalog.Armor, Equip);
		AddOption("Helmet", catalog.Helmets, Equip);
		AddOption("Gauntlets", catalog.Gauntlets, Equip);
		AddOption("Boots", catalog.Boots, Equip);
		AddOption("Ring / amulet", catalog.Jewelry, Equip);

		AddOption("Spell", catalog.Spells, (body, value) =>
		{
			var variable = OnSpawnEdits.GetVariable(body);
			return OnSpawnEdits.SetStatement(body, @"\w+\.Spells\s*=",
				$"{variable}.Spells = new CreatureSpellCollection() {{ {{ new CreatureSpellEntry(new CreatureSpell<{value}>(10), 100, TimeSpan.FromSeconds(3)) }} }};");
		});
	}

	private static string Wield(string body, string value)
		=> OnSpawnEdits.AppendStatement(body, $"{OnSpawnEdits.GetVariable(body)}.Wield(new {value}());");

	private static string Equip(string body, string value)
		=> OnSpawnEdits.AppendStatement(body, $"{OnSpawnEdits.GetVariable(body)}.Equip(new {value}());");

	private void AddOption(string label, IReadOnlyList<string> values, Func<string, string, string> apply)
	{
		_optionsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

		var text = new TextBlock { Text = label, Style = (Style)FindResource("FieldLabel") };
		var combo = new ComboBox { ItemsSource = values, IsEnabled = values.Count > 0 };
		var button = new Button { Content = "Set", IsEnabled = values.Count > 0 };

		button.Click += (_, _) =>
		{
			var value = (combo.Text ?? String.Empty).Trim();

			if (value.Length > 0)
				EditRequested?.Invoke(body => apply(body, value));
		};

		Grid.SetRow(text, _optionRow);
		Grid.SetRow(combo, _optionRow);
		Grid.SetRow(button, _optionRow);
		Grid.SetColumn(combo, 1);
		Grid.SetColumn(button, 2);

		_optionsGrid.Children.Add(text);
		_optionsGrid.Children.Add(combo);
		_optionsGrid.Children.Add(button);

		_optionRow++;
	}

	private void OnBodySearchChanged(object sender, TextChangedEventArgs e)
	{
		if (_catalog is null)
			return;

		var query = _bodySearch.Text.Trim();

		_bodies.ItemsSource = query.Length == 0
			? _catalog.Bodies
			: _catalog.Bodies.Where(b => b.Id.ToString().StartsWith(query)
			                             || b.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
			                             || b.Texture.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
	}

	private void OnBodyDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (_bodies.SelectedItem is BodyAsset body)
			EditRequested?.Invoke(script => OnSpawnEdits.SetInitializer(script, "Body", body.Id.ToString()));
	}

	private void OnBodySelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateHuePreview();

	private void OnHueNameChanged(object sender, SelectionChangedEventArgs e)
	{
		if (_hueNames.SelectedItem is not string name)
			return;

		var color = System.Drawing.Color.FromName(name);
		_huePicker.SelectedColor = Color.FromArgb(color.A, color.R, color.G, color.B);
	}

	private void OnHueColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) => UpdateHuePreview();

	private void OnSetNamedHue(object sender, RoutedEventArgs e)
	{
		var name = (_hueNames.Text ?? String.Empty).Trim();

		if (name.Length > 0)
			EditRequested?.Invoke(script => OnSpawnEdits.SetInitializer(script, "Hue", $"Color.{name}"));
	}

	private void OnSetCustomHue(object sender, RoutedEventArgs e)
	{
		var color = _huePicker.SelectedColor ?? Colors.White;
		EditRequested?.Invoke(script => OnSpawnEdits.SetInitializer(script, "Hue", $"Color.FromArgb({color.A}, {color.R}, {color.G}, {color.B})"));
	}

	private void UpdateHuePreview()
	{
		var color = _huePicker.SelectedColor ?? Colors.White;

		_hueArgb.Text = $"A {color.A}  R {color.R}  G {color.G}  B {color.B}   #{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

		var body = _bodies.SelectedItem as BodyAsset ?? _catalog?.Bodies.FirstOrDefault();

		_huePreview.Source = body?.Tint(color);
		_huePreviewLabel.Text = body is null
			? "Pick a body on the Body tab to preview the hue."
			: $"Previewing body {body.Label}. Pick another on the Body tab.";
	}

	private void OnPlaySound(object sender, RoutedEventArgs e)
	{
		if (sender is not FrameworkElement { DataContext: SoundAsset sound })
			return;

		if (sound.FilePath is null)
		{
			_status.Text = $"No audio file found for sound {sound.Id}.";
			return;
		}

		_player.Open(new Uri(sound.FilePath));
		_player.Play();
	}

	private void OnCopySoundId(object sender, RoutedEventArgs e)
	{
		if (sender is not FrameworkElement { DataContext: SoundAsset sound })
			return;

		Clipboard.SetText(sound.Id.ToString());
		_status.Text = $"Copied sound id {sound.Id}.";
	}

	private void OnSoundSearchChanged(object sender, TextChangedEventArgs e)
	{
		if (_catalog is null)
			return;

		var query = _soundSearch.Text.Trim();

		_sounds.ItemsSource = query.Length == 0
			? _catalog.Sounds
			: _catalog.Sounds.Where(s => s.Id.ToString().Contains(query)
			                             || s.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
			                             || s.Category.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
	}
}
