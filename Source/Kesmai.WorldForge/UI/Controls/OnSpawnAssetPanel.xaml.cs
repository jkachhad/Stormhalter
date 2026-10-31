#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Editor.Assets;

namespace Kesmai.WorldForge.UI;

/// <summary>
/// Side panel for the OnSpawn script: a builder form for the whole script, dropdowns of server types, a body
/// gallery, a hue picker with a tinted preview, and the custom sound library. Each choice is applied to the script
/// through <see cref="EditRequested"/>.
/// </summary>
public partial class OnSpawnAssetPanel : UserControl
{
	/// <summary>Raised with an edit to apply to the current script body.</summary>
	public event Action<Func<string, string>>? EditRequested;

	/// <summary>Returns the current OnSpawn body, or null when no creature is selected.</summary>
	public Func<string?>? ReadScript { get; set; }

	private AssetCatalog? _catalog;
	private OnSpawnBuilder _builder = new();
	private string? _pendingScript;
	private bool _unreadScript;
	private readonly MediaPlayer _player = new();
	private int _optionRow;

	private const string NoHue = "None";
	private const string CustomHue = "Custom ARGB";

	private static readonly HueSwatch[] _swatches = new[] { "White", "Red", "Lime", "Blue", "Gold", "Purple", "Cyan", "Orange", "Gray", "Black" }
		.Select(name => new HueSwatch(name, NamedColor(name)))
		.ToArray();

	private Dictionary<int, BodyAsset> _bodyById = new();
	private BodyAsset? _body;
	private Color _hue = Colors.White;
	private bool _syncing;

	public OnSpawnAssetPanel()
	{
		InitializeComponent();

		SetBuilder(new OnSpawnBuilder());

		Loaded += OnLoaded;
	}

	/// <summary>Fills the builder from a script, for example when another creature is selected.</summary>
	public OnSpawnParseResult? ShowScript(string? script)
	{
		if (_catalog is null)
		{
			_pendingScript = script;
			return null;
		}

		var builder = new OnSpawnBuilder();
		OnSpawnParseResult? result = null;

		builder.SetEnumChoices(_catalog.Immunities, _catalog.Weaknesses);

		if (!String.IsNullOrWhiteSpace(script))
			result = builder.Parse(script, _catalog.Shields, _catalog.Armor, _catalog.Helmets, _catalog.Gauntlets, _catalog.Boots);

		// a script the builder couldn't read starts the form empty; writing it would replace that script
		_unreadScript = result is { Succeeded: false };
		SetBuilder(_unreadScript ? NewBuilder() : builder);
		_status.Text = Describe(result);

		return result;
	}

	private OnSpawnBuilder NewBuilder()
	{
		var builder = new OnSpawnBuilder();

		if (_catalog is not null)
			builder.SetEnumChoices(_catalog.Immunities, _catalog.Weaknesses);

		return builder;
	}

	private static string Describe(OnSpawnParseResult? result)
	{
		if (result is null)
			return "OnSpawn is empty, so the builder starts blank.";

		if (!result.Succeeded)
			return result.Problem!;

		if (result.DtoOnly)
			return $"OnSpawn picks between creatures with if or switch, so the builder edits only its CreatureDTO values ({result.Filled}): Name, Body, Level, Alignment, Attack, Defense, stats and sounds. Write changes those values where they stand; the rest of the script stays as typed, and other builder fields aren't written.";

		var kept = result.KeptInitializers + result.KeptStatements;

		return kept == 0
			? $"Filled the builder from OnSpawn ({result.Filled} fields and lists)."
			: $"Filled the builder from OnSpawn ({result.Filled} fields and lists). {kept} line{(kept == 1 ? " has" : "s have")} no field and {(kept == 1 ? "is" : "are")} kept as typed under Kept code.";
	}

	private void SetBuilder(OnSpawnBuilder builder)
	{
		_builder.Changed -= OnBuilderChanged;
		_builder = builder;
		_builder.Changed += OnBuilderChanged;

		_builderRoot.DataContext = _builder;
		_dialogueRoot.DataContext = _builder;
		UpdateDialogueCheck();
		_builderPreview.Text = _builder.Generate();

		if (_catalog is not null)
			ShowBuilderBodyAndHue();
	}

	private void OnBuilderChanged(object? sender, EventArgs e)
	{
		UpdateDialogueCheck();
		_builderPreview.Text = _builder.Generate();
	}

	/// <summary>
	/// Only humanoids have Shift+Right Click options. When the Builder's class isn't one, the Dialogue tab says so and
	/// its options aren't written; when the class can't be followed, it warns but still writes them.
	/// </summary>
	private void UpdateDialogueCheck()
	{
		if (_catalog is null)
			return;

		var request = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
		var folder = request.HasReceivedResponse ? request.Response?.Directory : null;
		var humanoid = HumanoidCheck.IsHumanoid(_builder.CreatureType, folder, _catalog.BaseTypes);
		var type = String.IsNullOrWhiteSpace(_builder.CreatureType) ? "this class" : _builder.CreatureType.Trim();

		_builder.DialogueAllowed = humanoid != false;

		_dialogueWarning.Text = humanoid switch
		{
			false => $"Only humanoid creatures have Shift+Right Click options, and {type} isn't one. Pick a humanoid class in Builder, or remove these options. They won't be written to OnSpawn; options the script already has are kept as they are.",
			null => $"Couldn't tell whether {type} is a humanoid. Only humanoid creatures have Shift+Right Click options, so check the class if the segment fails to compile.",
			_ => String.Empty,
		};
		_dialogueWarningBox.Visibility = humanoid == true ? Visibility.Collapsed : Visibility.Visible;
		_dialogueOptionsList.IsEnabled = _dialogueOption.IsEnabled = humanoid != false;
	}

	private void OnBuilderRead(object sender, RoutedEventArgs e)
	{
		var script = ReadScript?.Invoke();

		if (script is null)
		{
			_status.Text = "Select a creature with an OnSpawn script first.";
			return;
		}

		_status.Text = Describe(ShowScript(script));
	}

	private void OnBuilderWrite(object sender, RoutedEventArgs e)
	{
		if (_unreadScript && !String.IsNullOrWhiteSpace(ReadScript?.Invoke())
		    && MessageBox.Show(Window.GetWindow(this)!,
			    "The builder couldn't read this creature's OnSpawn, so writing replaces the whole script with the builder's code. Replace it?",
			    "Write to OnSpawn", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
			return;

		var body = _builder.Generate();

		EditRequested?.Invoke(_ => body);
		_unreadScript = false;
		_status.Text = "Wrote the builder to OnSpawn.";
	}

	private void OnBuilderUseBody(object sender, RoutedEventArgs e)
	{
		var id = _bodyId.Text.Trim();

		if (id.Length > 0)
			_builder.Body = id;
		else
			_status.Text = "Pick a body on the Body & Hue tab first.";
	}

	private void OnBuilderUsePickerHue(object sender, RoutedEventArgs e) => HueToBuilder();

	private void OnDialogueAdd(object sender, RoutedEventArgs e)
	{
		var option = new DialogueOption { Title = "New option" };
		option.Steps.Add(new DialogueStep { Kind = DialogueStep.Say, Value = "Greetings." });

		_builder.Dialogue.Add(option);
		_dialogueList.SelectedItem = option;
	}

	private void OnDialogueRemove(object sender, RoutedEventArgs e)
	{
		if (_dialogueList.SelectedItem is DialogueOption option)
			_builder.Dialogue.Remove(option);
	}

	private void OnDialogueUp(object sender, RoutedEventArgs e) => MoveDialogue(-1);
	private void OnDialogueDown(object sender, RoutedEventArgs e) => MoveDialogue(1);

	private void MoveDialogue(int offset)
	{
		if (_dialogueList.SelectedItem is not DialogueOption option)
			return;

		var index = _builder.Dialogue.IndexOf(option);
		var target = index + offset;

		if (target < 0 || target >= _builder.Dialogue.Count)
			return;

		_builder.Dialogue.Move(index, target);
		_dialogueList.SelectedItem = option;
	}

	/// <summary>Every quest the dialogue mentions, for the quest dropdowns.</summary>
	private List<string> QuestNames() => _builder.Dialogue
		.SelectMany(o => o.Steps.Select(s => s.Quest).Append(o.RequiredQuest))
		.Where(q => !String.IsNullOrWhiteSpace(q))
		.Select(q => q.Trim())
		.Distinct()
		.OrderBy(q => q)
		.ToList();

	private void OnQuestNamesOpened(object? sender, EventArgs e) => _dRequiredQuest.ItemsSource = QuestNames();

	private void OnDialogueStepEdit(object? sender, DataGridBeginningEditEventArgs e) => _dStepQuest.ItemsSource = QuestNames();

	private void FillBuilderChoices(AssetCatalog catalog)
	{
		_bCreature.ItemsSource = catalog.Creatures;
		_bAlignment.ItemsSource = catalog.Alignments;
		_bHueMode.ItemsSource = OnSpawnBuilder.HueModes;
		_bHueName.ItemsSource = catalog.NamedColors;
		_bPenetration.ItemsSource = catalog.ShieldPenetration;
		_bSearch.ItemsSource = catalog.SearchStrategies;
		_bWeapon.ItemsSource = catalog.Weapons;
		_bShield.ItemsSource = catalog.Shields;
		_bArmor.ItemsSource = catalog.Armor;
		_bHelmet.ItemsSource = catalog.Helmets;
		_bGauntlets.ItemsSource = catalog.Gauntlets;
		_bBoots.ItemsSource = catalog.Boots;
		_bBrain.ItemsSource = catalog.Brains;
		_bAttackType.ItemsSource = OnSpawnBuilder.AttackTypes;
		_bAttackEffect.ItemsSource = new[] { String.Empty }.Concat(catalog.AttackComponents).ToList();
		_bSpell.ItemsSource = catalog.Spells;
		_bStatus.ItemsSource = catalog.Statuses;
		_bStat.ItemsSource = catalog.EntityStats;
		_bStatModifier.ItemsSource = StatRow.Modifiers;
		_dStepKind.ItemsSource = DialogueStep.Kinds;
		_dRequiredComparison.ItemsSource = OnSpawnBuilder.RequirementComparisons;
		_dRequiredState.ItemsSource = OnSpawnBuilder.QuestStates;
		_bSpellImmunity.ItemsSource = catalog.Spells;
		_bSpellWeakness.ItemsSource = catalog.Spells;

		foreach (var sound in new[] { _bNearbySound, _bAttackSound, _bDeathSound, _bWarmSound })
			sound.ItemsSource = catalog.Sounds;
	}

	private async void OnLoaded(object sender, RoutedEventArgs e)
	{
		Loaded -= OnLoaded;

		_status.Text = "Loading server types, bodies and sounds…";

		_catalog = await AssetCatalog.LoadAsync();

		BuildOptions(_catalog);
		FillBuilderChoices(_catalog);

		_bodyById = _catalog.Bodies.GroupBy(b => b.Id).ToDictionary(g => g.Key, g => g.First());
		_bodies.ItemsSource = _catalog.Bodies;
		_sounds.ItemsSource = _catalog.Sounds;
		_hueMode.ItemsSource = new[] { NoHue, CustomHue }.Concat(_catalog.NamedColors).ToList();
		_hueSwatches.ItemsSource = _swatches;

		var initial = ShowScript(_pendingScript ?? ReadScript?.Invoke());

		_status.Text = _catalog.Warnings.Count > 0
			? String.Join(Environment.NewLine, _catalog.Warnings)
			: $"{_catalog.Creatures.Count} creatures, {_catalog.Bodies.Count} bodies, {_catalog.Sounds.Count} sounds. {Describe(initial)}";
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

		// scripts set this as a factory: (attacker) => new RandomSearchStrategy()
		AddOption("Target strategy", catalog.SearchStrategies, (body, value) =>
			OnSpawnEdits.SetStatement(body, @"\w+\.CombatantSearchStrategy\s*=",
				$"{OnSpawnEdits.GetVariable(body)}.CombatantSearchStrategy = (attacker) => new {value}();"));

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
				$"{variable}.Spells = new CreatureSpellCollection() {{ {{ new CreatureSpell<{value}>(10), 100, TimeSpan.FromSeconds(3) }} }};");
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

	private void OnBrowseBodies(object sender, RoutedEventArgs e)
	{
		var show = _bodyBrowser.Visibility != Visibility.Visible;

		_bodyBrowser.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
		_browseBodies.Content = show ? "Hide bodies" : "Browse bodies…";

		if (show && _body is not null)
		{
			_syncing = true;
			_bodies.SelectedItem = _body;
			_syncing = false;

			if (_bodies.SelectedItem is not null)
				_bodies.ScrollIntoView(_bodies.SelectedItem);
		}
	}

	private void OnBodySelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!_syncing && _bodies.SelectedItem is BodyAsset body)
			_bodyId.Text = body.Id.ToString();
	}

	private void OnBodyDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (_bodies.SelectedItem is not BodyAsset body)
			return;

		_bodyId.Text = body.Id.ToString();
		OnBrowseBodies(sender, e);
	}

	private void OnBodyIdChanged(object sender, TextChangedEventArgs e)
	{
		_body = Int32.TryParse(_bodyId.Text.Trim(), out var id) && _bodyById.TryGetValue(id, out var body) ? body : null;
		UpdateHuePreview();
	}

	private void OnHueModeChanged(object sender, SelectionChangedEventArgs e)
	{
		if (_syncing || _hueMode.SelectedItem is not string mode)
			return;

		switch (mode)
		{
			case NoHue: SetHue(Colors.White, NoHue); break;
			case CustomHue: SetHue(_hue, CustomHue); break;
			default: SetHue(NamedColor(mode), mode); break;
		}
	}

	private void OnHueColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
	{
		if (!_syncing && e.NewValue is Color color)
			SetHue(color, CustomHue);
	}

	private void OnHueSwatch(object sender, RoutedEventArgs e)
	{
		if (sender is FrameworkElement { DataContext: HueSwatch swatch })
			SetHue(swatch.Color, swatch.Name);
	}

	private void OnHueComponentChanged(object sender, TextChangedEventArgs e)
	{
		if (_syncing)
			return;

		if (Byte.TryParse(_hueA.Text.Trim(), out var a) && Byte.TryParse(_hueR.Text.Trim(), out var r)
		    && Byte.TryParse(_hueG.Text.Trim(), out var g) && Byte.TryParse(_hueB.Text.Trim(), out var b))
			SetHue(Color.FromArgb(a, r, g, b), CustomHue, updateComponents: false);
	}

	/// <summary>Shows a hue in every control of the Body &amp; Hue tab without the controls feeding back into each other.</summary>
	private void SetHue(Color color, string mode, bool updateComponents = true)
	{
		_syncing = true;

		_hue = color;
		_hueMode.SelectedItem = mode;
		_huePicker.SelectedColor = color;

		if (updateComponents)
		{
			_hueA.Text = color.A.ToString();
			_hueR.Text = color.R.ToString();
			_hueG.Text = color.G.ToString();
			_hueB.Text = color.B.ToString();
		}

		_syncing = false;

		UpdateHuePreview();
	}

	/// <summary>Fills the Body &amp; Hue tab from the builder, so it shows the selected creature's body and hue.</summary>
	private void ShowBuilderBodyAndHue()
	{
		_bodyId.Text = _builder.Body;

		switch (_builder.HueMode)
		{
			case "Named" when !String.IsNullOrWhiteSpace(_builder.HueName):
				SetHue(NamedColor(_builder.HueName.Trim()), _builder.HueName.Trim());
				break;
			case "Custom":
				SetHue(Color.FromArgb(_builder.HueA, _builder.HueR, _builder.HueG, _builder.HueB), CustomHue);
				break;
			default:
				SetHue(Colors.White, NoHue);
				break;
		}
	}

	private void HueToBuilder()
	{
		var mode = _hueMode.SelectedItem as string ?? NoHue;

		if (mode == NoHue)
		{
			_builder.HueMode = "None";
		}
		else if (mode == CustomHue)
		{
			_builder.HueMode = "Custom";
			(_builder.HueA, _builder.HueR, _builder.HueG, _builder.HueB) = (_hue.A, _hue.R, _hue.G, _hue.B);
		}
		else
		{
			_builder.HueMode = "Named";
			_builder.HueName = mode;
		}
	}

	private string? HueExpression() => (_hueMode.SelectedItem as string ?? NoHue) switch
	{
		NoHue => null,
		CustomHue => $"Color.FromArgb({_hue.A}, {_hue.R}, {_hue.G}, {_hue.B})",
		var name => $"Color.{name}",
	};

	private void OnSetBody(object sender, RoutedEventArgs e) => ApplyBodyAndHue(body: true, hue: false);
	private void OnSetHue(object sender, RoutedEventArgs e) => ApplyBodyAndHue(body: false, hue: true);
	private void OnSetBodyAndHue(object sender, RoutedEventArgs e) => ApplyBodyAndHue(body: true, hue: true);

	/// <summary>Writes the body and/or hue into the OnSpawn script, and into the builder so the two stay in step.</summary>
	private void ApplyBodyAndHue(bool body, bool hue)
	{
		var id = _bodyId.Text.Trim();
		var expression = HueExpression();
		var messages = new List<string>();

		if (body && !Int32.TryParse(id, out _))
		{
			messages.Add("Enter a body id or pick one with Browse bodies.");
			body = false;
		}

		if (hue && expression is null)
		{
			messages.Add("Hue is None, so the script's hue was left alone.");
			hue = false;
		}

		if (!body && !hue)
		{
			if (messages.Count > 0)
				_status.Text = String.Join(" ", messages);
			return;
		}

		EditRequested?.Invoke(script =>
		{
			if (body)
				script = OnSpawnEdits.SetInitializer(script, "Body", id);

			if (hue)
				script = OnSpawnEdits.SetInitializer(script, "Hue", expression!);

			return script;
		});

		if (body)
			_builder.Body = id;

		if (hue)
			HueToBuilder();

		messages.Insert(0, body && hue ? $"Set Body = {id} and Hue = {expression}." : body ? $"Set Body = {id}." : $"Set Hue = {expression}.");
		_status.Text = String.Join(" ", messages);
	}

	private static Color NamedColor(string name)
	{
		var color = System.Drawing.Color.FromName(name);
		return Color.FromArgb(color.A, color.R, color.G, color.B);
	}

	private void UpdateHuePreview()
	{
		_hueArgb.Text = $"ARGB #{_hue.A:X2}{_hue.R:X2}{_hue.G:X2}{_hue.B:X2}";
		_huePreview.Source = _body?.Tint(_hue);

		var id = _bodyId.Text.Trim();

		if (_body is null)
		{
			_bodyTitle.Text = id.Length == 0 ? "No body" : $"Body {id}";
			_bodyInfo.Text = id.Length == 0
				? "Enter a body id or pick one with Browse bodies."
				: "This id isn't in Body-External.xml, so there is nothing to preview.";
			return;
		}

		_bodyTitle.Text = String.IsNullOrEmpty(_body.Name) ? $"Body {_body.Id}" : $"Body {_body.Id} · {_body.Name}";

		var image = _body.Image;
		var size = _body.Source is Int32Rect source ? $"{source.Width}×{source.Height}px"
			: image is not null ? $"{image.PixelWidth}×{image.PixelHeight}px"
			: "texture not found";

		_bodyInfo.Text = $"{_body.Texture} · {size}";
	}

	/// <summary>A one-click hue on the Body &amp; Hue tab.</summary>
	public sealed record HueSwatch(string Name, Color Color)
	{
		public Brush Brush { get; } = new SolidColorBrush(Color);
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
