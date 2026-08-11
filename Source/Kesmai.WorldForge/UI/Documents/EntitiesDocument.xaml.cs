using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CommonServiceLocator;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;
using Kesmai.WorldForge.Roslyn;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Data;

namespace Kesmai.WorldForge.UI.Documents;

public sealed class SpellTypeDisplayConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		var type = value as string ?? "";
		var match = Regex.Match(type, @"^CreatureSpell<(?<spell>.+)>$");
		return match.Success ? match.Groups["spell"].Value : type;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}

public partial class EntitiesDocument : UserControl
{
	private EntitiesViewModel? _viewModel;
	private IReadOnlyList<RoslynConstructorDescriptor> _constructorDescriptors = Array.Empty<RoslynConstructorDescriptor>();
	private IReadOnlyList<string> _weaponTypeNames = Array.Empty<string>();
	private IReadOnlyList<string> _equipmentTypeNames = Array.Empty<string>();
	private IReadOnlyList<string> _creatureDefenseSuggestions = Array.Empty<string>();
	private IReadOnlyList<string> _spellStatusSuggestions = Array.Empty<string>();
	private IReadOnlyList<string> _spellTypeNames = Array.Empty<string>();
	private IReadOnlyList<string> _entityStatNames = Array.Empty<string>();
	private string? _counterEntityType;
	private bool _canUseCounters;
	private bool _isRefreshingStructuredCatalog;
	private bool _structuredCatalogLoaded;
	private readonly Dictionary<string, bool> _counterCapabilities = new Dictionary<string, bool>(StringComparer.Ordinal);
	
	public ObservableCollection<SegmentSpawner> Spawns { get; } = new ObservableCollection<SegmentSpawner>();
	
	public EntitiesDocument()
	{
		InitializeComponent();

		var collectionDetailsTemplate = (DataTemplate)Resources["CollectionDetailsTemplate"];
		var collectionConstructorTemplate = (DataTemplate)Resources["CollectionConstructorTemplate"];
		var cooldownEditorTemplate = (DataTemplate)Resources["CooldownEditorTemplate"];
		foreach (var grid in new[] { _attacksGrid, _blocksGrid, _spellsGrid })
		{
			grid.RowDetailsTemplate = collectionDetailsTemplate;
			grid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
			grid.PreviewMouseLeftButtonDown += SelectCollectionRowOnFirstClick;
			grid.AddHandler(Selector.SelectionChangedEvent,
				new SelectionChangedEventHandler(CollectionTypeSelectionChanged));
			var argumentsColumn = grid.Columns.FirstOrDefault(column => Equals(column.Header, "Arguments"));
			if (argumentsColumn is DataGridTemplateColumn constructorColumn)
			{
				constructorColumn.Header = "Constructor / options";
				constructorColumn.CellTemplate = collectionConstructorTemplate;
			}
			var cooldownColumn = grid.Columns.FirstOrDefault(column => Equals(column.Header, "Cooldown"));
			if (cooldownColumn is DataGridTemplateColumn cooldownTemplateColumn)
			{
				cooldownTemplateColumn.Width = 180;
				cooldownTemplateColumn.CellTemplate = cooldownEditorTemplate;
			}
		}
		foreach (var grid in new[] { _propertiesGrid, _defenseMethodsGrid, _methodsGrid, _lootGrid, _statsGrid })
			grid.PreviewMouseLeftButtonDown += SelectCollectionRowOnFirstClick;
		_methodsGrid.RowDetailsTemplate = collectionDetailsTemplate;
		_methodsGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
		_lootGrid.RowDetailsTemplate = collectionDetailsTemplate;
		_lootGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
		
		DataContextChanged += OnDataContextChanged;
	}

	private static void CollectionTypeSelectionChanged(object sender, SelectionChangedEventArgs args)
	{
		if (args.OriginalSource is not ComboBox combo ||
			combo.DataContext is not StructuredCollectionItem item ||
			!ReferenceEquals(combo.ItemsSource, item.KnownTypes) ||
			combo.SelectedItem is not string selectedType)
			return;

		item.Type = selectedType;
	}

	private static void SelectCollectionRowOnFirstClick(object sender, MouseButtonEventArgs args)
	{
		if (sender is not DataGrid grid || args.OriginalSource is not DependencyObject source)
			return;

		for (DependencyObject current = source; current != null; current = VisualTreeHelper.GetParent(current))
		{
			if (current is DataGridRow row)
			{
				if (!ReferenceEquals(grid.SelectedItem, row.Item))
					grid.SelectedItem = row.Item;

				return;
			}

			if (ReferenceEquals(current, grid))
				return;
		}
	}

	private void CooldownComboBoxLoaded(object sender, RoutedEventArgs args)
	{
		if (sender is not ComboBox combo) return;
		combo.Dispatcher.BeginInvoke(new Action(() =>
		{
			NormalizeCooldownSelection(combo);
			combo.ApplyTemplate();
			if (combo.Template.FindName("PART_EditableTextBox", combo) is not TextBox textBox) return;
			textBox.TextAlignment = TextAlignment.Right;
			textBox.HorizontalContentAlignment = HorizontalAlignment.Right;
			textBox.CaretIndex = textBox.Text.Length;
		}));
	}

	private static void NormalizeCooldownSelection(ComboBox combo)
	{
		if (combo.SelectedItem is not ComboBoxItem item) return;
		var value = item.Content?.ToString() ?? combo.Text;
		combo.SelectedIndex = -1;
		combo.Text = value;
		combo.ApplyTemplate();
		if (combo.Template.FindName("PART_EditableTextBox", combo) is TextBox textBox)
		{
			textBox.TextAlignment = TextAlignment.Right;
			textBox.HorizontalContentAlignment = HorizontalAlignment.Right;
		}
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
	{
		if (_viewModel != null)
			_viewModel.PropertyChanged -= OnViewModelPropertyChanged;

		_viewModel = args.NewValue as EntitiesViewModel;

		if (_viewModel != null)
			_viewModel.PropertyChanged += OnViewModelPropertyChanged;

		UpdateSpawns();
		UpdateStructuredEditor();
	}
	
	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
	{
		if (_viewModel == null)
			return;

		if (args.PropertyName == nameof(EntitiesViewModel.Entity))
			UpdateSpawns();

		if (args.PropertyName == nameof(EntitiesViewModel.OnSpawn))
		{
			_viewModel.OnSpawn.SetConstructorDescriptors(_constructorDescriptors);
			_viewModel.OnSpawn.SetWeaponTypes(_weaponTypeNames);
			_viewModel.OnSpawn.SetEquipmentTypes(_equipmentTypeNames);
			_viewModel.OnSpawn.SetCreatureDefenseSuggestions(_creatureDefenseSuggestions);
			_viewModel.OnSpawn.SetSpellStatusSuggestions(_spellStatusSuggestions);
			_viewModel.OnSpawn.SetSpellTypes(_spellTypeNames);
			_viewModel.OnSpawn.SetEntityStatNames(_entityStatNames);
			ApplyCounterCapability();
		}

		if (args.PropertyName is nameof(EntitiesViewModel.Entity) or nameof(EntitiesViewModel.SelectedScript) or nameof(EntitiesViewModel.OnSpawn))
			UpdateStructuredEditor();
	}

	private void UpdateStructuredEditor()
	{
		var visible = _viewModel?.SelectedScript?.Name == "OnSpawn";
		_structuredOnSpawn.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
		_structuredColumn.Width = visible ? new GridLength(666) : new GridLength(0);
		if (visible)
		{
			_viewModel?.OnSpawn.SetConstructorDescriptors(_constructorDescriptors);
			_viewModel?.OnSpawn.SetWeaponTypes(_weaponTypeNames);
			_viewModel?.OnSpawn.SetEquipmentTypes(_equipmentTypeNames);
			_viewModel?.OnSpawn.SetSpellTypes(_spellTypeNames);
			_viewModel?.OnSpawn.SetEntityStatNames(_entityStatNames);
			ApplyCounterCapability();
			RefreshAttackComponentSuggestions();
		}
	}

	private async void RefreshAttackComponentSuggestions()
	{
		if (_isRefreshingStructuredCatalog) return;
		var host = ServiceLocator.Current.GetInstance<SegmentWorkspace>()?.Host;
		if (host == null) return;
		if (_structuredCatalogLoaded)
		{
			await RefreshCounterCapabilityAsync(host);
			return;
		}
		_isRefreshingStructuredCatalog = true;
		try
		{
			var types = await host.GetAttackComponentTypeNamesAsync();
			StructuredCollectionArgument.SetAttackComponentTypes(types);
			_constructorDescriptors = await host.GetCreatureConstructorsAsync();
			_viewModel?.OnSpawn.SetConstructorDescriptors(_constructorDescriptors);
			_weaponTypeNames = await host.GetWieldableTypeNamesAsync();
			_viewModel?.OnSpawn.SetWeaponTypes(_weaponTypeNames);
			_equipmentTypeNames = await host.GetEquipmentTypeNamesAsync();
			_viewModel?.OnSpawn.SetEquipmentTypes(_equipmentTypeNames);
			_creatureDefenseSuggestions = await host.GetCreatureDefenseSuggestionsAsync();
			_viewModel?.OnSpawn.SetCreatureDefenseSuggestions(_creatureDefenseSuggestions);
			_spellStatusSuggestions = await host.GetSpellStatusSuggestionsAsync();
			_viewModel?.OnSpawn.SetSpellStatusSuggestions(_spellStatusSuggestions);
		_spellTypeNames = await host.GetCreatureSpellTypeNamesAsync();
		_viewModel?.OnSpawn.SetSpellTypes(_spellTypeNames);
		_entityStatNames = await host.GetEntityStatNamesAsync();
		_viewModel?.OnSpawn.SetEntityStatNames(_entityStatNames);
			_structuredCatalogLoaded = true;
			await RefreshCounterCapabilityAsync(host);
		}
		finally { _isRefreshingStructuredCatalog = false; }
	}

	private async Task RefreshCounterCapabilityAsync(CustomRoslynHost host)
	{
		var entityType = _viewModel?.OnSpawn.EntityType;
		if (String.IsNullOrWhiteSpace(entityType)) return;
		if (!_counterCapabilities.TryGetValue(entityType, out var canUseCounters))
		{
			canUseCounters = await host.TypeHasMethodAsync(entityType, "AddCounter");
			_counterCapabilities[entityType] = canUseCounters;
		}
		if (_viewModel?.OnSpawn.EntityType != entityType) return;
		_counterEntityType = entityType;
		_canUseCounters = canUseCounters;
		ApplyCounterCapability();
	}

	private void ApplyCounterCapability()
	{
		if (_viewModel == null) return;
		_viewModel.OnSpawn.SetCanUseCounters(_canUseCounters &&
			String.Equals(_counterEntityType, _viewModel.OnSpawn.EntityType, StringComparison.Ordinal));
	}

	private void ApplyStructuredOnSpawnClick(object sender, RoutedEventArgs e)
	{
		if (_viewModel?.SelectedScript is not Script script)
			return;

		var attackConstructors = _viewModel.OnSpawn.AttackItems.Select(item => item.ConstructorOption).ToArray();
		var blockConstructors = _viewModel.OnSpawn.BlockItems.Select(item => item.ConstructorOption).ToArray();
		var spellConstructors = _viewModel.OnSpawn.SpellItems.Select(item => item.ConstructorOption).ToArray();
		script.Body = _viewModel.OnSpawn.Apply(script.Body);
		_viewModel.InvalidateCatalog();
		_viewModel.RefreshOnSpawn();
		_viewModel.OnSpawn.SetConstructorDescriptors(_constructorDescriptors);
		_viewModel.OnSpawn.SetWeaponTypes(_weaponTypeNames);
		_viewModel.OnSpawn.SetEquipmentTypes(_equipmentTypeNames);
		_viewModel.OnSpawn.SetCreatureDefenseSuggestions(_creatureDefenseSuggestions);
		_viewModel.OnSpawn.SetSpellStatusSuggestions(_spellStatusSuggestions);
		_viewModel.OnSpawn.SetSpellTypes(_spellTypeNames);
		_viewModel.OnSpawn.SetEntityStatNames(_entityStatNames);
		ApplyCounterCapability();
		RestoreConstructorSelections(_viewModel.OnSpawn.AttackItems, attackConstructors);
		RestoreConstructorSelections(_viewModel.OnSpawn.BlockItems, blockConstructors);
		RestoreConstructorSelections(_viewModel.OnSpawn.SpellItems, spellConstructors);
		_scriptsTabControl.Items.Refresh();
	}

	private static void RestoreConstructorSelections(IReadOnlyList<StructuredCollectionItem> items,
		IReadOnlyList<string?> selections)
	{
		for (var index = 0; index < Math.Min(items.Count, selections.Count); index++)
			if (!String.IsNullOrWhiteSpace(selections[index]))
				items[index].ConstructorOption = selections[index];
	}

	private void CreatePatternSkeletonClick(object sender, RoutedEventArgs e)
	{
		if (_viewModel?.SelectedScript is not Script script || _viewModel.OnSpawn.CreationPattern == "Custom/unsupported")
			return;

		if (!String.IsNullOrWhiteSpace(script.Body) &&
			MessageBox.Show("Replace the current OnSpawn body with a new structured skeleton?",
				"Create OnSpawn skeleton", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
			return;

		script.Body = _viewModel.OnSpawn.CreateSkeleton();
		_viewModel.InvalidateCatalog();
		_viewModel.RefreshOnSpawn();
		_viewModel.OnSpawn.SetConstructorDescriptors(_constructorDescriptors);
		_viewModel.OnSpawn.SetWeaponTypes(_weaponTypeNames);
		_viewModel.OnSpawn.SetEquipmentTypes(_equipmentTypeNames);
		_viewModel.OnSpawn.SetCreatureDefenseSuggestions(_creatureDefenseSuggestions);
		_viewModel.OnSpawn.SetSpellStatusSuggestions(_spellStatusSuggestions);
		_viewModel.OnSpawn.SetSpellTypes(_spellTypeNames);
		_viewModel.OnSpawn.SetEntityStatNames(_entityStatNames);
		ApplyCounterCapability();
		_scriptsTabControl.Items.Refresh();
	}

	private void AddStructuredPropertyClick(object sender, RoutedEventArgs e)
	{
		var name = (sender as FrameworkElement)?.Tag as string ?? _newPropertyName.Text;
		if (!String.IsNullOrWhiteSpace(name))
		{
			_viewModel?.OnSpawn.AddProperty(name);
			if ((sender as FrameworkElement)?.Tag == null)
				_newPropertyName.Text = String.Empty;
		}
	}

	private void AddStructuredMethodClick(object sender, RoutedEventArgs e)
	{
		var name = (sender as FrameworkElement)?.Tag as string ?? _newMethodName.SelectedItem as string;
		if (!String.IsNullOrWhiteSpace(name))
			_viewModel?.OnSpawn.AddMethod(name);
	}

	private void AddLootItemClick(object sender, RoutedEventArgs e)
	{
		_viewModel?.OnSpawn.AddLootItem();
	}

	private void AddStatClick(object sender, RoutedEventArgs e) => _viewModel?.OnSpawn.AddStat();

	private void DeleteStatClick(object sender, RoutedEventArgs e)
	{
		if (_statsGrid.SelectedItem is StructuredStatItem item)
			_viewModel?.OnSpawn.StatItems.Remove(item);
	}

	private void AddCollectionItemClick(object sender, RoutedEventArgs e)
	{
		if ((sender as FrameworkElement)?.Tag is not string kind) return;
		var type = kind == "Attacks" ? _newAttackType.Text : kind == "Blocks" ? _newBlockType.Text : _newSpellType.Text;
		_viewModel?.OnSpawn.AddCollectionItem(kind, type);
	}

	private void DeleteCollectionItemClick(object sender, RoutedEventArgs e)
	{
		if ((sender as Button)?.CommandParameter is DataGrid grid && grid.SelectedItem is StructuredCollectionItem item)
		{
			if (grid == _attacksGrid) _viewModel?.OnSpawn.AttackItems.Remove(item);
			else if (grid == _blocksGrid) _viewModel?.OnSpawn.BlockItems.Remove(item);
			else if (grid == _spellsGrid) _viewModel?.OnSpawn.SpellItems.Remove(item);
		}
	}

	private void DeleteStructuredPropertyClick(object sender, RoutedEventArgs e)
	{
		var grid = (sender as Button)?.CommandParameter as DataGrid ?? _propertiesGrid;
		if (grid.SelectedItem is StructuredOnSpawnProperty property)
			_viewModel?.OnSpawn.RemoveProperty(property);
	}

	private void DeleteStructuredMethodClick(object sender, RoutedEventArgs e)
	{
		var grid = (sender as Button)?.CommandParameter as DataGrid ?? _methodsGrid;
		if (grid.SelectedItem is StructuredOnSpawnMethod method)
			_viewModel?.OnSpawn.Methods.Remove(method);
	}

	private void DeleteLootItemClick(object sender, RoutedEventArgs e)
	{
		if (_lootGrid.SelectedItem is StructuredLootItem item)
			_viewModel?.OnSpawn.LootItems.Remove(item);
	}

	private void GridEditorPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (sender is FrameworkElement { DataContext: StructuredOnSpawnMethod method })
		{
			_methodsGrid.SelectedItem = method;
			_methodsGrid.CurrentItem = method;
		}
	}

	private void UpdateSpawns()
	{
		Spawns.Clear();

		if (_viewModel.Entity != null)
		{
			var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
			var segment = presenter.Segment;

			if (segment is null)
				return;

			var spawns = segment.Spawns.GetSpawns(_viewModel.Entity);
				
			foreach (var spawn in spawns)
				Spawns.Add(spawn);
		}
	}
	
	private void SpawnerButtonClick(object sender, RoutedEventArgs e)
	{
		if (sender is not Button { DataContext: SegmentSpawner spawner })
			return;
		
		var applicationPresenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();

		if (applicationPresenter != null)
			spawner.Present(applicationPresenter);
	}
}

public class EntitiesViewModel : ObservableRecipient
{
	public string Name => "(Entities)";

	private SegmentEntity? _entity;
	private Script? _selectedScript;
	private StructuredOnSpawnModel _onSpawn = new StructuredOnSpawnModel();
	private Segment? _catalogSegment;
	private StructuredOnSpawnCatalog? _catalog;

	public StructuredOnSpawnModel OnSpawn
	{
		get => _onSpawn;
		private set => SetProperty(ref _onSpawn, value);
	}

	public SegmentEntity? Entity
	{
		get => _entity;
		set
		{
			if (!SetProperty(ref _entity, value))
				return;

			if (_entity != null)
			{
				if (_selectedScript is null || !_entity.Scripts.Contains(_selectedScript))
					SelectedScript = _entity.Scripts.FirstOrDefault(s => s.IsEnabled) ?? _entity.Scripts.FirstOrDefault();
			}
			else
			{
				SelectedScript = null;
			}
		}
	}

	public Script? SelectedScript
	{
		get => _selectedScript;
		set
		{
			if (SetProperty(ref _selectedScript, value))
				RefreshOnSpawn();
		}
	}

	public void RefreshOnSpawn()
	{
		Segment? segment = null;
		try
		{
			segment = ServiceLocator.Current.GetInstance<ApplicationPresenter>()?.Segment;
		}
		catch
		{
			// The designer and early startup do not always have a presenter available.
		}

		if (_catalog == null || !ReferenceEquals(_catalogSegment, segment))
		{
			_catalogSegment = segment;
			_catalog = StructuredOnSpawnCatalog.FromSegment(segment);
		}
		var catalog = _catalog;
		OnSpawn = StructuredOnSpawnModel.Parse(_selectedScript?.Name == "OnSpawn" ? _selectedScript.Body : null, catalog);
	}

	public void InvalidateCatalog() => _catalog = null;
}

public class StructuredOnSpawnCatalog
{
	private static readonly string[] ActionNames = { "AddStatus", "AddWeakness", "AddImmunity", "AddGold", "AddLoot", "Wield", "Equip", "AddCounter" };
	private static readonly Regex Declaration = new Regex(
		@"(?:var|[A-Za-z_][A-Za-z0-9_.<>]*)\s+([A-Za-z_][A-Za-z0-9_]*)\s*=\s*new\s+([A-Za-z_][A-Za-z0-9_.<>]*)",
		RegexOptions.Compiled);
	private static readonly Regex Return = new Regex(@"return\s+([A-Za-z_][A-Za-z0-9_]*)\s*;", RegexOptions.Compiled);

	public SortedSet<string> EntityVariables { get; } = new SortedSet<string>(StringComparer.Ordinal) { "creature", "mob", "crit" };
	public SortedSet<string> EntityTypes { get; } = new SortedSet<string>(StringComparer.Ordinal) { "CreatureEntity", "MobileEntity", "CreatureBloodDefaultEntity", "MuCreatureEntity", "MuUndeadEntity" };
	public SortedSet<string> ConfigurationVariables { get; } = new SortedSet<string>(StringComparer.Ordinal) { "configuration", "dto", "critDTO", "mob.Mv" };
	public SortedSet<string> ConfigurationTypes { get; } = new SortedSet<string>(StringComparer.Ordinal) { "CreatureDTO", "CreatureBloodDTO", "MuVar.MuMobileEntity" };
	public SortedSet<string> BaseMobVariables { get; } = new SortedSet<string>(StringComparer.Ordinal) { "baseMob" };
	public SortedSet<string> BaseMobTypes { get; } = new SortedSet<string>(StringComparer.Ordinal);
	public Dictionary<string, SortedSet<string>> ActionArguments { get; } = ActionNames.ToDictionary(
		name => name, _ => new SortedSet<string>(StringComparer.Ordinal), StringComparer.Ordinal);
	public Dictionary<string, List<string>> IntegerValues { get; } =
		new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
	public SortedSet<string> TreasureNames { get; } = new SortedSet<string>(StringComparer.Ordinal);
	public List<string> LootCreateExpressions { get; } = new List<string>();
	private void AddLootCreateExpression(string value)
	{
		if (!value.Contains("=>", StringComparison.Ordinal)) return;
		LootCreateExpressions.Remove(value);
		LootCreateExpressions.Add(value);
		while (LootCreateExpressions.Count > 10) LootCreateExpressions.RemoveAt(0);
	}
	public IReadOnlyList<string> GetIntegerValues(string name) =>
		IntegerValues.TryGetValue(NormalizeFieldName(name), out var values)
			? values.AsEnumerable().Reverse().ToArray() : Array.Empty<string>();
	private void AddInteger(string name, string value)
	{
		if (!Regex.IsMatch(value, @"^[+-]?[0-9][0-9_]*$")) return;
		var key = NormalizeFieldName(name);
		if (!IntegerValues.TryGetValue(key, out var values))
			IntegerValues[key] = values = new List<string>();
		values.Remove(value);
		values.Add(value);
		while (values.Count > 10) values.RemoveAt(0);
	}
	private static string NormalizeFieldName(string name) =>
		Regex.Replace(name, @"\s*\([^)]*\)\s*$", String.Empty).Replace(" ", String.Empty);

	public static StructuredOnSpawnCatalog FromSegment(Segment? segment)
	{
		var catalog = new StructuredOnSpawnCatalog();
		if (segment == null)
			return catalog;

		foreach (var treasure in segment.Treasures)
			if (!String.IsNullOrWhiteSpace(treasure.Name)) catalog.TreasureNames.Add(treasure.Name);

		foreach (var entity in segment.Entities)
		{
			var source = entity.Scripts.FirstOrDefault(script => script.Name == "OnSpawn")?.Body;
			if (String.IsNullOrWhiteSpace(source))
				continue;

			var declarations = Declaration.Matches(source).Cast<Match>().ToList();
			var returned = Return.Matches(source).Cast<Match>().LastOrDefault()?.Groups[1].Value;
			foreach (var declaration in declarations)
			{
				var variable = declaration.Groups[1].Value;
				var type = declaration.Groups[2].Value;
				if (variable == returned)
				{
					catalog.EntityVariables.Add(variable);
					catalog.EntityTypes.Add(type);
				}
				else if (type.Contains("DTO", StringComparison.Ordinal))
				{
					catalog.ConfigurationVariables.Add(variable);
					catalog.ConfigurationTypes.Add(type);
				}
				else
				{
					catalog.BaseMobVariables.Add(variable);
					catalog.BaseMobTypes.Add(type);
				}
			}

			foreach (var actionName in ActionNames.Where(name => name != "AddLoot"))
			{
				var actionRegex = new Regex($@"(?m)^\s*[A-Za-z_][A-Za-z0-9_]*\.{Regex.Escape(actionName)}\s*\(");
				foreach (Match action in actionRegex.Matches(source))
				{
					var open = action.Index + action.Length - 1;
					var close = FindClosingParenthesis(source, open);
					if (close > open)
					{
						var arguments = source.Substring(open + 1, close - open - 1).Trim();
						catalog.ActionArguments[actionName].Add(arguments);
						if (actionName == "AddCounter")
						{
							var counterArguments = arguments;
							var pointMatch = Regex.Match(counterArguments,
								@"^new\s+Point2D\s*\((.*)\)$", RegexOptions.Singleline);
							if (pointMatch.Success)
								counterArguments = pointMatch.Groups[1].Value;
							var values = SplitArguments(counterArguments).Select(value => value.Trim()).ToArray();
							var names = new[] { "X", "Y", "Region" };
							for (var index = 0; index < Math.Min(values.Length, names.Length); index++)
								catalog.AddInteger(names[index], values[index]);
						}
					}
				}
			}

			foreach (Match value in Regex.Matches(source,
			         @"(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*(?:=|:)\s*(?<value>[+-]?[0-9][0-9_]*)\b"))
				catalog.AddInteger(value.Groups["name"].Value, value.Groups["value"].Value);

			foreach (Match lootEntry in Regex.Matches(source, @"new\s+LootPackEntry\s*\("))
			{
				var open = lootEntry.Index + lootEntry.Length - 1;
				var close = FindClosingParenthesis(source, open);
				if (close <= open) continue;
				foreach (var argument in SplitArguments(source.Substring(open + 1, close - open - 1)))
					catalog.AddLootCreateExpression(argument.Trim());
			}

			var constructorNames = new Dictionary<string, string[]>(StringComparer.Ordinal)
			{
				["CreatureAttack"] = new[] { "AttackLevel", "MinimumDamage", "MaximumDamage", "Message" },
				["CreatureBasicAttack"] = new[] { "SkillLevel", "MinimumDamage", "MaximumDamage" },
				["CreatureBlock"] = new[] { "Chance", "Description" },
				["CreatureSpell"] = new[] { "SkillLevel", "Cost", "InstantCast", "Mantra" }
			};
			foreach (Match constructor in Regex.Matches(source,
			         @"new\s+(?<type>CreatureAttack|CreatureBasicAttack|CreatureBlock|CreatureSpell(?:<[^>]+>)?)\s*\("))
			{
				var open = constructor.Index + constructor.Length - 1;
				var close = FindClosingParenthesis(source, open);
				if (close <= open) continue;
				var rawType = constructor.Groups["type"].Value;
				var type = rawType.StartsWith("CreatureSpell<", StringComparison.Ordinal) ? "CreatureSpell" : rawType;
				var fields = constructorNames[type];
				var arguments = SplitArguments(source.Substring(open + 1, close - open - 1)).ToArray();
				for (var index = 0; index < Math.Min(fields.Length, arguments.Length); index++)
				{
					var argument = arguments[index].Trim();
					var colon = argument.IndexOf(':');
					catalog.AddInteger(colon > 0 ? argument.Substring(0, colon).Trim() : fields[index],
						colon > 0 ? argument.Substring(colon + 1).Trim() : argument);
				}
			}
		}

		return catalog;
	}

	private static IEnumerable<string> SplitArguments(string source)
	{
		var start = 0; var depth = 0; var quoted = false;
		for (var index = 0; index < source.Length; index++)
		{
			var current = source[index];
			if (current == '"' && (index == 0 || source[index - 1] != '\\')) quoted = !quoted;
			if (quoted) continue;
			if (current is '(' or '{' or '[') depth++;
			else if (current is ')' or '}' or ']') depth--;
			else if (current == ',' && depth == 0)
			{
				yield return source.Substring(start, index - start); start = index + 1;
			}
		}
		if (start < source.Length) yield return source.Substring(start);
	}

	private static int FindClosingParenthesis(string source, int open)
	{
		var depth = 0;
		var inString = false;
		var quote = '\0';
		for (var index = open; index < source.Length; index++)
		{
			var current = source[index];
			if (inString)
			{
				if (current == quote && (index == 0 || source[index - 1] != '\\')) inString = false;
				continue;
			}
			if (current is '\"' or '\'') { inString = true; quote = current; continue; }
			if (current == '(') depth++;
			if (current == ')' && --depth == 0) return index;
		}
		return -1;
	}
}

public class StructuredOnSpawnProperty : ObservableObject
{
	private string _value;
	private bool _isEnabled;

	public string Name { get; }
	public string Value { get => _value; set => SetProperty(ref _value, value); }
	public bool IsEnabled { get => _isEnabled; set => SetProperty(ref _isEnabled, value); }
	public IReadOnlyList<string> SuggestedValues { get; }
	internal int ValueStart { get; }
	internal int ValueLength { get; }
	internal string? TargetVariable { get; }
	internal int FullStart { get; }
	internal int FullLength { get; }
	internal int CommentStart { get; }
	internal int CommentLength { get; }

	public StructuredOnSpawnProperty(string name, string value, IReadOnlyList<string> suggestedValues,
		int valueStart, int valueLength, string? targetVariable = null, int fullStart = -1, int fullLength = 0,
		bool isEnabled = true, int commentStart = -1, int commentLength = 0)
	{
		Name = name;
		_value = value;
		SuggestedValues = suggestedValues;
		ValueStart = valueStart;
		ValueLength = valueLength;
		TargetVariable = targetVariable;
		FullStart = fullStart;
		FullLength = fullLength;
		_isEnabled = isEnabled;
		CommentStart = commentStart;
		CommentLength = commentLength;
	}
}

public class StructuredOnSpawnMethod : ObservableObject
{
	private string _name;
	private string _arguments;
	private bool _isEnabled;

	public string Name
	{
		get => _name;
		set
		{
			if (SetProperty(ref _name, value))
			{
				OnPropertyChanged(nameof(IsLootPack));
				BuildFields();
			}
		}
	}
	public string Arguments { get => _arguments; set => SetProperty(ref _arguments, value); }
	public bool IsEnabled { get => _isEnabled; set => SetProperty(ref _isEnabled, value); }
	public IReadOnlyList<string> SuggestedArguments { get; private set; }
	public bool IsLootPack => Name == "AddLoot";
	public ObservableCollection<StructuredCollectionArgument> Fields { get; } =
		new ObservableCollection<StructuredCollectionArgument>();
	internal int ArgumentsStart { get; }
	internal int ArgumentsLength { get; }
	internal int CommentStart { get; }
	internal int CommentLength { get; }
	internal int FullStart { get; }
	internal int FullLength { get; }

	public StructuredOnSpawnMethod(string name, string arguments, IReadOnlyList<string>? suggestedArguments = null,
		int argumentsStart = -1, int argumentsLength = 0, bool isEnabled = true,
		int commentStart = -1, int commentLength = 0, int fullStart = -1, int fullLength = 0)
	{
		_name = name;
		_arguments = arguments;
		SuggestedArguments = suggestedArguments ?? Array.Empty<string>();
		ArgumentsStart = argumentsStart;
		ArgumentsLength = argumentsLength;
		_isEnabled = isEnabled;
		CommentStart = commentStart;
		CommentLength = commentLength;
		FullStart = fullStart;
		FullLength = fullLength;
		BuildFields();
	}
	private void BuildFields()
	{
		Fields.Clear();
		if (Name != "AddCounter") return;
		var counterArguments = Arguments.Trim();
		var pointMatch = Regex.Match(counterArguments,
			@"^new\s+Point2D\s*\((.*)\)$", RegexOptions.Singleline);
		if (pointMatch.Success)
			counterArguments = pointMatch.Groups[1].Value;
		var values = SplitTopLevelArguments(counterArguments).ToList();
		while (values.Count < 3) values.Add("0");
		var names = new[] { "X (int)", "Y (int)", "Region (int)" };
		for (var index = 0; index < 3; index++)
		{
			var field = new StructuredCollectionArgument(names[index], values[index], null);
			field.PropertyChanged += (_, args) =>
			{
				if (args.PropertyName == nameof(StructuredCollectionArgument.Value))
					Arguments = $"new Point2D({String.Join(", ", Fields.Select(item => item.Value))})";
			};
			Fields.Add(field);
		}
	}
	private static IEnumerable<string> SplitTopLevelArguments(string arguments)
	{
		var start = 0;
		var depth = 0;
		for (var index = 0; index < arguments.Length; index++)
		{
			switch (arguments[index])
			{
				case '(' or '{' or '[': depth++; break;
				case ')' or '}' or ']': depth--; break;
				case ',' when depth == 0:
					yield return arguments.Substring(start, index - start).Trim();
					start = index + 1;
					break;
			}
		}
		if (start <= arguments.Length)
			yield return arguments.Substring(start).Trim();
	}
	public void SetSuggestedArguments(IReadOnlyList<string> suggestions)
	{
		SuggestedArguments = suggestions;
		OnPropertyChanged(nameof(SuggestedArguments));
	}
}

public class StructuredLootItem : StructuredCollectionItem
{
	public int PackNumber { get; }

	public StructuredLootItem(int packNumber, string expression, bool isEnabled = true)
		: base("LootPackEntry", expression, isEnabled, new[] { "LootPackEntry" })
	{
		PackNumber = packNumber;
	}
}

public class StructuredStatItem : ObservableObject
{
	private string _stat;
	private string _operation;
	private string _value;
	private bool _isEnabled;
	public string Stat { get => _stat; set => SetProperty(ref _stat, value); }
	public string Operation { get => _operation; set => SetProperty(ref _operation, value); }
	public string Value { get => _value; set => SetProperty(ref _value, value); }
	public bool IsEnabled { get => _isEnabled; set => SetProperty(ref _isEnabled, value); }
	public IReadOnlyList<string> KnownStats { get; private set; }
	public IReadOnlyList<string> Operations { get; } =
		new[] { "Base", "MaximumValue", "Add", "Remove" };
	public StructuredStatItem(string stat, string operation, string value, bool enabled,
		IReadOnlyList<string> knownStats)
	{
		_stat = stat; _operation = operation; _value = value; _isEnabled = enabled; KnownStats = knownStats;
	}
	public void SetKnownStats(IReadOnlyList<string> names)
	{
		KnownStats = names;
		OnPropertyChanged(nameof(KnownStats));
	}
}

public class StructuredCollectionItem : ObservableObject
{
	private string _type;
	private string _expression;
	private bool _isEnabled;
	private IReadOnlyList<RoslynConstructorDescriptor> _constructorCatalog = Array.Empty<RoslynConstructorDescriptor>();
	private IReadOnlyList<RoslynConstructorDescriptor> _roslynConstructors = Array.Empty<RoslynConstructorDescriptor>();
	private string? _selectedConstructorSignature;
	private readonly Dictionary<string, string> _parameterValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
	private IReadOnlyList<string> _lootTreasureNames = Array.Empty<string>();
	private IReadOnlyList<string> _lootCreateSuggestions = Array.Empty<string>();
	private bool _updatingComponentFields;
	private string _pendingCooldown = "";
	public string Type
	{
		get => _type;
		set
		{
			var previousType = _type;
			var previousArguments = SplitArguments(Arguments).Select(argument => argument.Trim()).ToList();
			if (!SetProperty(ref _type, value)) return;
			_roslynConstructors = FilterConstructors(_constructorCatalog, value);
			Expression = new Regex(@"(?<=new\s)[A-Za-z_][A-Za-z0-9_]*(?:<[^>]+>)?").Replace(Expression, value, 1);
			OnPropertyChanged(nameof(KnownArguments));
			OnPropertyChanged(nameof(KnownConstructors));
			OnPropertyChanged(nameof(ConstructorOption));
			var translatedArguments = TranslateAttackArguments(previousType, value, previousArguments);
			var newArguments = translatedArguments ?? GetKnownArguments(value).FirstOrDefault();
			if (newArguments != null) Arguments = newArguments;
			BuildFields();
		}
	}
	private static string? TranslateAttackArguments(string previousType, string newType, IReadOnlyList<string> arguments)
	{
		if (previousType == "CreatureAttack" && newType == "CreatureBasicAttack")
		{
			var retained = arguments.Take(3).ToList();
			var component = arguments.Skip(3).FirstOrDefault(IsAttackComponentArgument);
			if (component != null) retained.Add(component);
			return String.Join(", ", retained);
		}

		if (previousType == "CreatureBasicAttack" && newType == "CreatureAttack")
		{
			var retained = arguments.Take(3).ToList();
			retained.Add("\"The creature strikes you.\"");
			var component = arguments.Skip(3).FirstOrDefault(IsAttackComponentArgument);
			if (component != null) retained.Add(component);
			return String.Join(", ", retained);
		}

		return null;
	}
	private static bool IsAttackComponentArgument(string argument) =>
		Regex.IsMatch(argument, @"new\s+Attack[A-Za-z0-9_]*Component\s*\(", RegexOptions.Singleline);
	public string FormatForSource()
	{
		var arguments = SplitArguments(Arguments).Select(argument => argument.Trim()).ToList();
		if (Type == "CreatureBlock")
		{
			var blockConstructor = $"new {Type}({String.Join(", ", arguments)})";
			var cooldown = GetBlockCooldown();
			if (String.IsNullOrWhiteSpace(cooldown))
				return $"{{\r\n    {blockConstructor}\r\n}}";
			cooldown = Regex.Replace(cooldown, @"\s*//\s*cooldown\s*$", "").Trim();
			return $"{{\r\n    {blockConstructor},\r\n    {cooldown} // cooldown\r\n}}";
		}
		var components = arguments.Where(IsAttackComponentArgument).ToList();
		var attackCooldown = GetWeightedItemCooldown();
		var cooldownSuffix = String.IsNullOrWhiteSpace(attackCooldown) ? "" : $", {attackCooldown}";
		if (components.Count > 0)
		{
			arguments.RemoveAll(argument => IsAttackComponentArgument(argument));
			var leadingArguments = String.Join(", ", arguments);
			if (leadingArguments.Length > 0) leadingArguments += ",";
			var weight = String.IsNullOrWhiteSpace(Weight) ? "" : $", {Weight}";
			var formattedComponents = String.Join(",\r\n", components.Select(component => $"        {component}"));
			return $"{{\r\n    new {Type}( {leadingArguments}\r\n{formattedComponents}\r\n    ){weight}{cooldownSuffix}\r\n}}";
		}

		var constructor = $"new {Type}({String.Join(", ", arguments)})";
		if (String.IsNullOrWhiteSpace(Weight)) return constructor;
		if (Type.StartsWith("CreatureSpell<", StringComparison.Ordinal))
			return $"{{\r\n    {constructor},\r\n    {Weight}{cooldownSuffix}\r\n}}";
		return $"{{\r\n    {constructor}, {Weight}{cooldownSuffix}\r\n}}";
	}
	public string Expression { get => _expression; set { if (SetProperty(ref _expression, value)) { OnPropertyChanged(nameof(Arguments)); OnPropertyChanged(nameof(ConstructorOption)); OnPropertyChanged(nameof(Cooldown)); OnPropertyChanged(nameof(CooldownDisplay)); OnPropertyChanged(nameof(Weight)); } } }
	public bool IsEnabled { get => _isEnabled; set => SetProperty(ref _isEnabled, value); }
	public IReadOnlyList<string> KnownTypes { get; private set; }
	public void SetKnownTypes(IReadOnlyList<string> knownTypes)
	{
		KnownTypes = knownTypes;
		OnPropertyChanged(nameof(KnownTypes));
	}
	public void SetLootTreasureNames(IEnumerable<string> treasureNames)
	{
		_lootTreasureNames = treasureNames.Distinct(StringComparer.Ordinal)
			.OrderBy(name => name, StringComparer.Ordinal).ToArray();
		if (Type == "LootPackEntry") BuildFields();
	}
	public void SetLootCreateSuggestions(IEnumerable<string> expressions)
	{
		_lootCreateSuggestions = expressions.Reverse().Distinct(StringComparer.Ordinal).ToArray();
		if (Type == "LootPackEntry") BuildFields();
	}
	public ObservableCollection<StructuredCollectionArgument> Fields { get; } = new ObservableCollection<StructuredCollectionArgument>();
	public string CurrentValuesSummary
	{
		get
		{
			var values = new List<string>();
			for (var index = 0; index < Fields.Count; index++)
			{
				var field = Fields[index];
				if (!field.IsAvailable || field.Name == "Percentage chance") continue;
				if (field.Name == "Component type")
				{
					if (String.IsNullOrWhiteSpace(field.Value)) continue;
					values.Add(field.Value);
					if (index + 1 < Fields.Count && Fields[index + 1].Name == "Percentage chance")
						values.Add(Fields[index + 1].Value);
					continue;
				}
				values.Add(field.Value);
			}
			return String.Join(", ", values);
		}
	}
	public IReadOnlyList<string> KnownArguments => GetKnownArguments(Type);
	public IReadOnlyList<StructuredConstructorOption> KnownConstructors => _roslynConstructors.Count > 0
		? _roslynConstructors.Select(constructor =>
			new StructuredConstructorOption(constructor.Signature, constructor.Signature, constructor)).ToArray()
		: Type switch
	{
		"CreatureAttack" => new[]
		{
			new StructuredConstructorOption("Attack", KnownArguments[0]),
			new StructuredConstructorOption("Attack with component",
				"10, 5, 15, \"The creature strikes you.\", new AttackProneComponent(0)")
		},
		"CreatureBasicAttack" => new[]
		{
			new StructuredConstructorOption("Basic attack", KnownArguments[0]),
			new StructuredConstructorOption("Basic attack with component", KnownArguments[1])
		},
		"CreatureBlock" => new[]
		{
			new StructuredConstructorOption("Block", KnownArguments[0])
		},
		_ when Type.StartsWith("CreatureSpell<", StringComparison.Ordinal) => new[]
		{
			new StructuredConstructorOption("Spell", KnownArguments[0])
		},
		_ => Array.Empty<StructuredConstructorOption>()
	};
	public string? ConstructorOption
	{
		get
		{
			if (KnownConstructors.Count == 0) return null;
			if (_roslynConstructors.Count > 0)
				return SelectRoslynConstructor(SplitArguments(Arguments).Select(value => value.Trim()).ToList())?.Signature;
			if (Type is "CreatureAttack" or "CreatureBasicAttack")
				return KnownConstructors[IsAttackComponentArgument(Arguments) ? 1 : 0].Arguments;
			return KnownConstructors[0].Arguments;
		}
		set
		{
			if (String.IsNullOrWhiteSpace(value)) return;
			var constructor = _roslynConstructors.FirstOrDefault(item => item.Signature == value);
			if (constructor != null)
			{
				CacheCurrentParameterValues();
				_selectedConstructorSignature = constructor.Signature;
				var existing = SplitArguments(Arguments).Select(argument => argument.Trim()).ToList();
				var rebuilt = new List<string>();
				for (var index = 0; index < constructor.Parameters.Count; index++)
				{
					var parameter = constructor.Parameters[index];
					if (parameter.IsParams)
					{
						if (existing.Count > index) rebuilt.AddRange(existing.Skip(index));
						else if (_parameterValues.TryGetValue(parameter.Name, out var retainedParams) &&
						         !String.IsNullOrWhiteSpace(retainedParams)) rebuilt.Add(retainedParams);
						else if (Type == "LootPackEntry" &&
						         parameter.TypeName.Contains("LootPackItem", StringComparison.Ordinal))
							rebuilt.Add(_lootTreasureNames.FirstOrDefault() ?? "lootItem");
						continue;
					}
					rebuilt.Add(_parameterValues.TryGetValue(parameter.Name, out var retained) ? retained :
						index < existing.Count ? existing[index] : GetInitialParameterValue(parameter));
				}
				Arguments = String.Join(", ", rebuilt);
			}
			else
			{
				Arguments = value;
			}
			BuildFields();
			OnPropertyChanged();
		}
	}
	private void CacheCurrentParameterValues()
	{
		var values = SplitArguments(Arguments).Select(argument => argument.Trim()).ToList();
		var constructor = SelectRoslynConstructor(values);
		if (constructor == null) return;
		for (var index = 0; index < Math.Min(values.Count, constructor.Parameters.Count); index++)
		{
			var raw = values[index];
			var colon = FindTopLevelColon(raw);
			var explicitName = colon > 0 ? raw.Substring(0, colon).Trim() : null;
			var parameter = explicitName == null
				? constructor.Parameters[index]
				: constructor.Parameters.FirstOrDefault(item =>
					String.Equals(item.Name, explicitName, StringComparison.OrdinalIgnoreCase)) ?? constructor.Parameters[index];
			if (parameter.IsParams)
			{
				_parameterValues[parameter.Name] = String.Join(", ", values.Skip(index));
				break;
			}
			_parameterValues[parameter.Name] = colon > 0
				? raw.Substring(colon + 1).Trim() : raw;
		}
	}
	private RoslynConstructorDescriptor? SelectRoslynConstructor(IReadOnlyList<string> arguments)
	{
		var selected = _roslynConstructors.FirstOrDefault(item => item.Signature == _selectedConstructorSignature);
		if (selected != null && ConstructorAcceptsCount(selected, arguments.Count)) return selected;
		return _roslynConstructors.Where(item => ConstructorAcceptsCount(item, arguments.Count))
			.OrderByDescending(item => ScoreConstructor(item, arguments)).FirstOrDefault()
		       ?? _roslynConstructors.LastOrDefault();
	}
	private static bool ConstructorAcceptsCount(RoslynConstructorDescriptor constructor, int count)
	{
		var required = constructor.Parameters.Count(parameter => !parameter.IsOptional && !parameter.IsParams);
		var unlimited = constructor.Parameters.LastOrDefault()?.IsParams == true;
		return count >= required && (unlimited || count <= constructor.Parameters.Count);
	}
	private int ScoreConstructor(RoslynConstructorDescriptor constructor, IReadOnlyList<string> arguments)
	{
		var score = arguments.Count == constructor.Parameters.Count ? 5 : 0;
		for (var index = 0; index < arguments.Count; index++)
		{
			var parameterIndex = Math.Min(index, constructor.Parameters.Count - 1);
			if (parameterIndex < 0) return Int32.MinValue;
			var parameter = constructor.Parameters[parameterIndex];
			var raw = arguments[index].Trim();
			var colon = FindTopLevelColon(raw);
			if (colon > 0)
			{
				var argumentName = raw.Substring(0, colon).Trim();
				var namedParameter = constructor.Parameters.FirstOrDefault(item =>
					String.Equals(item.Name, argumentName, StringComparison.OrdinalIgnoreCase));
				if (namedParameter != null)
				{
					parameter = namedParameter;
					score += 20;
				}
				else score -= 10;
				raw = raw.Substring(colon + 1).Trim();
			}
			if (Type == "LootPackEntry")
			{
				if (_lootTreasureNames.Contains(raw, StringComparer.Ordinal))
					score += parameter.TypeName.Contains("LootPackItem", StringComparison.Ordinal) ? 40 :
						parameter.TypeName.Contains("Func<", StringComparison.Ordinal) ? -20 : 0;
				else if (raw.Contains("=>", StringComparison.Ordinal))
					score += parameter.TypeName.Contains("Func<", StringComparison.Ordinal) ? 40 :
						parameter.TypeName.Contains("LootPackItem", StringComparison.Ordinal) ? -20 : 0;
			}
			score += ScoreArgumentType(raw, parameter.TypeName);
		}
		return score;
	}
	private static int ScoreArgumentType(string value, string typeName)
	{
		var type = typeName.TrimEnd('?');
		if (value == "null") return typeName.EndsWith("?", StringComparison.Ordinal) ? 6 : 1;
		if (value.StartsWith("\"", StringComparison.Ordinal) || value.StartsWith("$\"", StringComparison.Ordinal) ||
			value.StartsWith("@\"", StringComparison.Ordinal))
			return type is "string" or "String" ? 12 : -8;
		if (value is "true" or "false") return type is "bool" or "Boolean" ? 12 : -8;
		if (Regex.IsMatch(value, @"^[+-]?[0-9][0-9_]*(?:[uUlLfFdDmM])?$"))
			return Regex.IsMatch(type, @"^(?:s?byte|u?short|u?int|u?long|Int16|Int32|Int64|UInt16|UInt32|UInt64|float|double|decimal)$") ? 10 : -5;
		if (IsAttackComponentArgument(value)) return type.Contains("Component", StringComparison.OrdinalIgnoreCase) ? 12 : -8;
		var member = Regex.Match(value, @"^(?:new\s+)?(?<type>[A-Za-z_][A-Za-z0-9_]*)[\.<(]");
		if (member.Success && type.Contains(member.Groups["type"].Value, StringComparison.OrdinalIgnoreCase)) return 10;
		if (value.Contains("Local", StringComparison.OrdinalIgnoreCase) && type.Contains("Local", StringComparison.OrdinalIgnoreCase)) return 10;
		return 0;
	}
	private string GetInitialParameterValue(RoslynConstructorParameter parameter)
	{
		if (parameter.IsOptional && !String.IsNullOrWhiteSpace(parameter.DefaultValue)) return parameter.DefaultValue;
		if (parameter.Suggestions.Count > 0) return parameter.Suggestions[0];
		if (Type == "LootPackEntry" && parameter.TypeName.Contains("Func<", StringComparison.Ordinal))
			return "(from, container) => new ClearBalm()";
		if (parameter.TypeName is "string" or "String") return "\"\"";
		if (parameter.TypeName.EndsWith("Component", StringComparison.Ordinal)) return "null";
		return "0";
	}
	public void SetConstructorDescriptors(IEnumerable<RoslynConstructorDescriptor> constructors)
	{
		_constructorCatalog = constructors.ToArray();
		_roslynConstructors = FilterConstructors(_constructorCatalog, Type);
		_selectedConstructorSignature = SelectRoslynConstructor(SplitArguments(Arguments).Select(value => value.Trim()).ToList())?.Signature;
		OnPropertyChanged(nameof(KnownConstructors));
		OnPropertyChanged(nameof(ConstructorOption));
		BuildFields();
	}
	public void InitializeFromRoslynDefaults()
	{
		if (_roslynConstructors.Count == 0) return;
		var constructor = _roslynConstructors
			.OrderBy(item => item.Parameters.Count(parameter => !parameter.IsOptional && !parameter.IsParams))
			.ThenBy(item => item.Parameters.Count).First();
		_selectedConstructorSignature = constructor.Signature;
		Arguments = String.Join(", ", constructor.Parameters.Where(parameter => !parameter.IsParams)
			.Select(parameter => $"{parameter.Name}: {GetInitialParameterValue(parameter)}"));
		BuildFields();
		OnPropertyChanged(nameof(ConstructorOption));
	}
	private static IReadOnlyList<RoslynConstructorDescriptor> FilterConstructors(
		IEnumerable<RoslynConstructorDescriptor> constructors, string type)
	{
		var key = type.StartsWith("CreatureSpell<", StringComparison.Ordinal) ? "CreatureSpell<>" : type;
		return constructors.Where(item => item.TypeName == key).ToArray();
	}
	private static IReadOnlyList<string> GetKnownArguments(string type) => type switch
	{
		"CreatureAttack" => new[] { "10, 5, 15, \"The creature strikes you.\"", "40, 20, 40, $\"The {mob.Name} strikes you.\"" },
		"CreatureBasicAttack" => new[]
		{
			"skillLevel: 12, minimumDamage: 10, maximumDamage: 20",
			"skillLevel: 12, minimumDamage: 10, maximumDamage: 20, new AttackProneComponent(0)"
		},
		"CreatureBlock" => new[] { "6, \"tough skin.\"", "3, \"an inexplicable miss\"", "1, \"a fearful aura\"" },
		_ when type.StartsWith("CreatureSpell<", StringComparison.Ordinal) => new[] { "skillLevel: 60, cost: 20, instantCast: false, mantra: SpellHelper.GenerateMantra()", "skillLevel: 40, cost: 10, instantCast: false, mantra: SpellHelper.GenerateMantra()" },
		_ => Array.Empty<string>()
	};
	public string Arguments
	{
		get
		{
			var open = Expression.IndexOf('('); if (open < 0) return "";
			var close = FindClose(Expression, open); return close > open ? Expression.Substring(open + 1, close - open - 1) : "";
		}
		set
		{
			var open = Expression.IndexOf('('); if (open < 0) return;
			var close = FindClose(Expression, open); if (close > open) Expression = Expression.Remove(open + 1, close - open - 1).Insert(open + 1, value);
			OnPropertyChanged();
		}
	}
	public string Weight
	{
		get => HasWeightAndCooldown ? GetTailArguments().FirstOrDefault() ?? "" : "";
		set
		{
			if (!HasWeightAndCooldown) return;
			SetWeightedItemTail(value, Cooldown);
			OnPropertyChanged();
		}
	}
	public string Cooldown
	{
		get => HasWeightAndCooldown ? GetWeightedItemCooldown() : "";
		set
		{
			if (!HasWeightAndCooldown) return;
			_pendingCooldown = value ?? "";
			if (!String.IsNullOrWhiteSpace(Weight)) SetWeightedItemTail(Weight, value);
			OnPropertyChanged();
			OnPropertyChanged(nameof(CooldownDisplay));
		}
	}
	public string CooldownDisplay
	{
		get
		{
			var value = Cooldown;
			if (value == "TimeSpan.Zero") return "0s";
			var match = Regex.Match(value,
				@"TimeSpan\.From(?<unit>Milliseconds|Seconds|Minutes|Hours|Days)\s*\((?<value>[^)]*)\)");
			if (!match.Success) return value;
			var unit = match.Groups["unit"].Value switch
			{
				"Milliseconds" => "ms",
				"Seconds" => "s",
				"Minutes" => "m",
				"Hours" => "h",
				"Days" => "d",
				_ => ""
			};
			return $"{match.Groups["value"].Value}{unit}";
		}
		set
		{
			var display = value?.Trim() ?? "";
			if (display is "Zero" or "0s") Cooldown = "TimeSpan.Zero";
			else
			{
				var compact = Regex.Match(display,
					@"^\(?(?<value>.*?)\)?(?<unit>ms|s|m|h|d)?$", RegexOptions.IgnoreCase);
				if (!compact.Success) Cooldown = display;
				else
				{
					var factory = compact.Groups["unit"].Value.ToLowerInvariant() switch
					{
						"ms" => "FromMilliseconds",
						"m" => "FromMinutes",
						"h" => "FromHours",
						"d" => "FromDays",
						_ => "FromSeconds"
					};
					Cooldown = $"TimeSpan.{factory}({compact.Groups["value"].Value})";
				}
			}
			OnPropertyChanged();
		}
	}
	private bool HasWeightAndCooldown => Type is "CreatureAttack" or "CreatureBasicAttack" ||
		Type.StartsWith("CreatureSpell<", StringComparison.Ordinal);
	public StructuredCollectionItem(string type, string expression, bool enabled, IReadOnlyList<string> knownTypes)
	{
		_type = type; _expression = expression; _isEnabled = enabled; KnownTypes = knownTypes;
		if (String.IsNullOrWhiteSpace(Arguments))
		{
			var defaultArguments = GetKnownArguments(type).FirstOrDefault();
			if (defaultArguments != null)
				Arguments = defaultArguments;
		}
		BuildFields();
	}
	private void BuildFields()
	{
		Fields.Clear();
		var removedBasicAttackMessage = false;
		var values = SplitArguments(Arguments).ToList();
		var constructor = SelectRoslynConstructor(values);
		if (constructor != null)
		{
			var boundParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var nextPositionalParameter = 0;
			foreach (var argument in values)
			{
				var raw = argument.Trim();
				var colon = FindTopLevelColon(raw);
				if (colon > 0)
				{
					var name = raw.Substring(0, colon).Trim();
					if (constructor.Parameters.Any(parameter =>
						    String.Equals(parameter.Name, name, StringComparison.OrdinalIgnoreCase)))
						boundParameters.Add(name);
					continue;
				}

				while (nextPositionalParameter < constructor.Parameters.Count &&
				       boundParameters.Contains(constructor.Parameters[nextPositionalParameter].Name))
					nextPositionalParameter++;
				if (nextPositionalParameter < constructor.Parameters.Count)
					boundParameters.Add(constructor.Parameters[nextPositionalParameter++].Name);
			}

			foreach (var parameter in constructor.Parameters.Where(parameter =>
				         !parameter.IsParams && !boundParameters.Contains(parameter.Name)))
				values.Add($"{parameter.Name}: {GetInitialParameterValue(parameter)}");
			values = OrderArgumentsByConstructor(values, constructor);
		}
		for (var index = 0; index < values.Count; index++)
		{
			var raw = values[index].Trim();
			var colon = FindTopLevelColon(raw);
			var explicitName = colon > 0 ? raw.Substring(0, colon).Trim() : null;
			var value = colon > 0 ? raw.Substring(colon + 1).Trim() : raw;
			var parameter = explicitName != null && constructor != null
				? constructor.Parameters.FirstOrDefault(item =>
					String.Equals(item.Name, explicitName, StringComparison.OrdinalIgnoreCase)) ??
				  (index < constructor.Parameters.Count ? constructor.Parameters[index] :
					  constructor.Parameters.LastOrDefault()?.IsParams == true ? constructor.Parameters.Last() : null)
				: constructor != null && index < constructor.Parameters.Count
					? constructor.Parameters[index]
					: constructor?.Parameters.LastOrDefault()?.IsParams == true ? constructor.Parameters.Last() : null;
			var fieldName = parameter != null
				? $"{FormatParameterName(parameter.Name)} ({parameter.TypeName})"
				: explicitName ?? GetFieldName(Type, index);
			if (Type == "CreatureBasicAttack" && index >= 3 &&
				(explicitName?.Equals("message", StringComparison.OrdinalIgnoreCase) == true ||
				 value.StartsWith("\"", StringComparison.Ordinal) || value.StartsWith("$\"", StringComparison.Ordinal)))
			{
				removedBasicAttackMessage = true;
				continue;
			}
			var isComponentParameter = parameter?.Name.Contains("component", StringComparison.OrdinalIgnoreCase) == true ||
				(parameter?.TypeName.Contains("component", StringComparison.OrdinalIgnoreCase) == true &&
				 parameter.TypeName.Contains("attack", StringComparison.OrdinalIgnoreCase)) ||
				(parameter == null && fieldName.Equals("Component", StringComparison.OrdinalIgnoreCase)) ||
				IsAttackComponentArgument(value);
			var component = isComponentParameter
				? Regex.Match(value, @"^new\s+(?<type>[A-Za-z_][A-Za-z0-9_]*)\s*\((?<chance>.*)\)$", RegexOptions.Singleline)
				: Match.Empty;
			if (component.Success)
			{
				AddField(new StructuredCollectionArgument("Component type", component.Groups["type"].Value, explicitName));
				AddField(new StructuredCollectionArgument("Percentage chance", component.Groups["chance"].Value.Trim(), null));
			}
			else if (isComponentParameter && (String.IsNullOrWhiteSpace(value) || value == "null"))
			{
				AddField(new StructuredCollectionArgument("Component type", "", explicitName));
				AddField(new StructuredCollectionArgument("Percentage chance", "0", null));
			}
			else
			{
				var suggestions = parameter?.Suggestions ?? Array.Empty<string>();
				if (Type == "LootPackEntry" && parameter?.TypeName.Contains("Func<", StringComparison.Ordinal) == true)
					suggestions = suggestions.Concat(_lootCreateSuggestions).Distinct(StringComparer.Ordinal).ToArray();
				var isNamedTreasureProvider = parameter != null &&
					parameter.TypeName.Contains("Func<", StringComparison.Ordinal) &&
					Regex.IsMatch(value, @"^[A-Za-z_][A-Za-z0-9_.]*$");
				if (Type == "LootPackEntry" && parameter != null &&
					(parameter.TypeName.Contains("LootPackItem", StringComparison.Ordinal) ||
					 parameter.Name.Equals("items", StringComparison.OrdinalIgnoreCase) ||
					 isNamedTreasureProvider))
					suggestions = suggestions.Concat(_lootTreasureNames).Distinct(StringComparer.Ordinal).ToArray();
				AddField(new StructuredCollectionArgument(fieldName, value, explicitName, suggestions));
			}
		}
		if ((Type is "CreatureAttack" or "CreatureBasicAttack") &&
			!Fields.Any(field => field.Name == "Component type" && String.IsNullOrWhiteSpace(field.Value)))
		{
			AddField(new StructuredCollectionArgument("Component type", "", null));
			AddField(new StructuredCollectionArgument("Percentage chance", "0", null));
		}
		if (Type == "CreatureBlock")
			AddField(new StructuredCollectionArgument("Cooldown", GetBlockCooldown(), null,
				new[] { "", "TimeSpan.Zero", "TimeSpan.FromSeconds(6)", "TimeSpan.FromSeconds(12)" }));
		if (removedBasicAttackMessage)
			RebuildArguments();
		UpdateMantraAvailability();
		OnPropertyChanged(nameof(CurrentValuesSummary));
	}
	private static List<string> OrderArgumentsByConstructor(IReadOnlyList<string> arguments,
		RoslynConstructorDescriptor constructor)
	{
		var namedParameterIndexes = new HashSet<int>();
		foreach (var argument in arguments)
		{
			var colon = FindTopLevelColon(argument);
			if (colon <= 0) continue;
			var name = argument.Substring(0, colon).Trim();
			var index = constructor.Parameters.ToList().FindIndex(parameter =>
				String.Equals(parameter.Name, name, StringComparison.OrdinalIgnoreCase));
			if (index >= 0) namedParameterIndexes.Add(index);
		}

		var nextPositionalIndex = 0;
		return arguments.Select((argument, originalIndex) =>
		{
			var colon = FindTopLevelColon(argument);
			var parameterIndex = -1;
			if (colon > 0)
			{
				var name = argument.Substring(0, colon).Trim();
				parameterIndex = constructor.Parameters.ToList().FindIndex(parameter =>
					String.Equals(parameter.Name, name, StringComparison.OrdinalIgnoreCase));
			}
			else
			{
				while (nextPositionalIndex < constructor.Parameters.Count &&
				       namedParameterIndexes.Contains(nextPositionalIndex))
					nextPositionalIndex++;
				parameterIndex = nextPositionalIndex++;
			}
			return (Argument: argument, ParameterIndex: parameterIndex < 0 ? Int32.MaxValue : parameterIndex,
				OriginalIndex: originalIndex);
		}).OrderBy(item => item.ParameterIndex).ThenBy(item => item.OriginalIndex)
			.Select(item => item.Argument).ToList();
	}
	private static string FormatParameterName(string name) => Regex.Replace(name, "([a-z0-9])([A-Z])", "$1 $2") switch
	{
		var text when text.Length > 0 => Char.ToUpperInvariant(text[0]) + text.Substring(1),
		_ => name
	};
	private void AddField(StructuredCollectionArgument field)
	{
		field.PropertyChanged += (_, args) =>
		{
			OnPropertyChanged(nameof(CurrentValuesSummary));
			if (args.PropertyName != nameof(StructuredCollectionArgument.Value) || _updatingComponentFields) return;
			if (field.Name == "Component type" && !String.IsNullOrWhiteSpace(field.Value))
			{
				var duplicate = Fields.Any(other => !ReferenceEquals(other, field) && other.Name == "Component type" &&
					String.Equals(other.Value, field.Value, StringComparison.Ordinal));
				if (duplicate)
				{
					_updatingComponentFields = true;
					field.Value = "";
					_updatingComponentFields = false;
					return;
				}
			}
			if (field.Name.Contains("Instant", StringComparison.OrdinalIgnoreCase))
				UpdateMantraAvailability();
			RebuildArguments();
			if (field.Name == "Component type" &&
				!Fields.Any(item => item.Name == "Component type" && String.IsNullOrWhiteSpace(item.Value)))
			{
				AddField(new StructuredCollectionArgument("Component type", "", null));
				AddField(new StructuredCollectionArgument("Percentage chance", "0", null));
			}
		};
		Fields.Add(field);
	}
	private void RebuildArguments()
	{
		var arguments = new List<string>();
		var cooldown = Fields.FirstOrDefault(field => field.Name == "Cooldown")?.Value;
		for (var index = 0; index < Fields.Count; index++)
		{
			var field = Fields[index];
			if (!field.IsAvailable) continue;
			if (field.Name == "Cooldown") continue;
			if (field.Name == "Percentage chance") continue;
			if (field.Name == "Component type")
			{
				if (String.IsNullOrWhiteSpace(field.Value)) continue;
				var chance = index + 1 < Fields.Count && Fields[index + 1].Name == "Percentage chance"
					? Fields[index + 1].Value : "0";
				var expression = $"new {field.Value}({chance})";
				arguments.Add(field.SourceName == null ? expression : $"{field.SourceName}: {expression}");
				continue;
			}
			arguments.Add(field.SourceName == null ? field.Value : $"{field.SourceName}: {field.Value}");
		}
		Arguments = String.Join(", ", arguments);
		if (Type == "CreatureBlock") SetBlockCooldown(cooldown);
	}
	private IReadOnlyList<string> GetTailArguments()
	{
		var open = Expression.IndexOf('(');
		if (open < 0) return Array.Empty<string>();
		var close = FindClose(Expression, open);
		if (close < 0) return Array.Empty<string>();
		var tail = Expression.Substring(close + 1).Trim().TrimEnd('}').Trim();
		if (!tail.StartsWith(",", StringComparison.Ordinal)) return Array.Empty<string>();
		return SplitArguments(tail.Substring(1)).Select(value => value.Trim()).
			Where(value => !String.IsNullOrWhiteSpace(value)).ToArray();
	}
	private string GetWeightedItemCooldown()
	{
		if (!HasWeightAndCooldown) return "";
		return GetTailArguments().Skip(1).FirstOrDefault() ?? _pendingCooldown;
	}
	private void SetWeightedItemTail(string? weight, string? cooldown)
	{
		var open = Expression.IndexOf('(');
		if (open < 0) return;
		var close = FindClose(Expression, open);
		if (close < 0) return;
		var newStart = Expression.LastIndexOf("new ", open, StringComparison.Ordinal);
		if (newStart < 0) return;
		var constructor = Expression.Substring(newStart, close - newStart + 1);
		var tail = new List<string>();
		if (!String.IsNullOrWhiteSpace(weight)) tail.Add(weight.Trim());
		if (!String.IsNullOrWhiteSpace(cooldown)) tail.Add(cooldown.Trim());
		Expression = tail.Count == 0 ? constructor : $"{{ {constructor}, {String.Join(", ", tail)} }}";
	}
	private string GetBlockCooldown()
	{
		if (Type != "CreatureBlock") return "";
		var open = Expression.IndexOf('(');
		if (open < 0) return "";
		var close = FindClose(Expression, open);
		if (close < 0) return "";
		var tail = Expression.Substring(close + 1).Trim();
		if (!tail.StartsWith(",", StringComparison.Ordinal)) return "";
		return tail.Substring(1).Trim().TrimEnd('}').Trim().TrimEnd(',').Trim();
	}
	private void SetBlockCooldown(string? cooldown)
	{
		var open = Expression.IndexOf('(');
		if (open < 0) return;
		var close = FindClose(Expression, open);
		if (close < 0) return;
		var newStart = Expression.LastIndexOf("new ", open, StringComparison.Ordinal);
		if (newStart < 0) return;
		var constructor = Expression.Substring(newStart, close - newStart + 1);
		Expression = String.IsNullOrWhiteSpace(cooldown)
			? $"{{ {constructor} }}"
			: $"{{ {constructor}, {cooldown!.Trim()} }}";
	}
	private void UpdateMantraAvailability()
	{
		var instantCast = Fields.FirstOrDefault(field =>
			field.Name.Contains("Instant", StringComparison.OrdinalIgnoreCase));
		var mantraAvailable = instantCast == null ||
			!String.Equals(instantCast.Value.Trim(), "true", StringComparison.OrdinalIgnoreCase);
		foreach (var mantra in Fields.Where(field =>
			         field.Name.StartsWith("Mantra", StringComparison.OrdinalIgnoreCase)))
			mantra.IsAvailable = mantraAvailable;
	}
	private static string GetFieldName(string type, int index)
	{
		var names = type switch
		{
			"CreatureAttack" => new[] { "Attack level", "Minimum damage", "Maximum damage", "Message", "Component" },
			"CreatureBasicAttack" => new[] { "Skill level", "Minimum damage", "Maximum damage", "Component" },
			"CreatureBlock" => new[] { "Chance", "Description" },
			_ when type.StartsWith("CreatureSpell<", StringComparison.Ordinal) => new[] { "Skill level", "Cost", "Instant cast", "Mantra" },
			_ => Array.Empty<string>()
		};
		return index < names.Length ? names[index] : $"Argument {index + 1}";
	}
	private static IEnumerable<string> SplitArguments(string source)
	{
		var start = 0; var depth = 0; var quoted = false;
		for (var i = 0; i < source.Length; i++) { var c = source[i]; if (c == '"' && (i == 0 || source[i - 1] != '\\')) quoted = !quoted; if (quoted) continue; if (c is '(' or '{' or '[') depth++; else if (c is ')' or '}' or ']') depth--; else if (c == ',' && depth == 0) { yield return source.Substring(start, i - start); start = i + 1; } }
		if (start < source.Length) yield return source.Substring(start);
	}
	private static int FindTopLevelColon(string source) { var depth = 0; for (var i = 0; i < source.Length; i++) { if (source[i] is '(' or '{' or '[') depth++; else if (source[i] is ')' or '}' or ']') depth--; else if (source[i] == ':' && depth == 0) return i; } return -1; }
	private static int FindClose(string text, int open) { var depth = 0; var quoted = false; for (var i = open; i < text.Length; i++) { if (text[i] == '"' && (i == 0 || text[i - 1] != '\\')) quoted = !quoted; if (quoted) continue; if (text[i] == '(') depth++; else if (text[i] == ')' && --depth == 0) return i; } return -1; }
}

public sealed record StructuredConstructorOption(string Name, string Arguments,
	RoslynConstructorDescriptor? Descriptor = null);

public class StructuredCollectionArgument : ObservableObject
{
	private static readonly string[] DefaultAttackComponentTypes =
	{
		"AttackAgeComponent", "AttackDazeComponent", "AttackPoisonComponent", "AttackProneComponent"
	};
	private static readonly ObservableCollection<string> AttackComponentSuggestions =
		new ObservableCollection<string>(DefaultAttackComponentTypes);
	private static IReadOnlyDictionary<string, IReadOnlyList<string>> IntegerSuggestions =
		new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
	private string _value;
	private bool _isAvailable = true;
	public string Name { get; }
	public string? SourceName { get; }
	public string Value { get => _value; set => SetProperty(ref _value, value); }
	public bool IsAvailable { get => _isAvailable; set => SetProperty(ref _isAvailable, value); }
	public IReadOnlyList<string> Suggestions { get; }
	public StructuredCollectionArgument(string name, string value, string? sourceName,
		IReadOnlyList<string>? roslynSuggestions = null)
	{
		Name = name; _value = value; SourceName = sourceName;
		IEnumerable<string> builtInSuggestions = name switch
		{
			"Component" => Array.Empty<string>(),
			"Component type" => AttackComponentSuggestions,
			"Percentage chance" => new[] { "0", "10", "20", "25", "40", "50", "60", "75", "80", "90", "100" },
			"Instant cast" => new[] { "true", "false" },
			_ when name.StartsWith("Mantra", StringComparison.OrdinalIgnoreCase) =>
				new[] { "SpellHelper.GenerateMantra()" },
			_ => Array.Empty<string>()
		};
		var integerSuggestions = IntegerSuggestions.TryGetValue(NormalizeFieldName(name), out var recentIntegers)
			? recentIntegers : Array.Empty<string>();
		Suggestions = (roslynSuggestions ?? Array.Empty<string>()).Concat(builtInSuggestions).Concat(integerSuggestions)
			.Distinct(StringComparer.Ordinal).ToArray();
	}
	private static string NormalizeFieldName(string name) =>
		Regex.Replace(name, @"\s*\([^)]*\)\s*$", String.Empty).Replace(" ", String.Empty);
	public static void SetIntegerSuggestions(IReadOnlyDictionary<string, List<string>> values)
	{
		if (values.Count == 0) return;
		IntegerSuggestions = values.ToDictionary(entry => NormalizeFieldName(entry.Key),
			entry => (IReadOnlyList<string>)entry.Value.AsEnumerable().Reverse().Take(10).ToArray(),
			StringComparer.OrdinalIgnoreCase);
	}

	public static void SetAttackComponentTypes(IEnumerable<string> types)
	{
		var discoveredTypes = types.Concat(DefaultAttackComponentTypes)
			.Distinct(StringComparer.Ordinal).OrderBy(type => type, StringComparer.Ordinal).ToArray();
		AttackComponentSuggestions.Clear();
		foreach (var type in discoveredTypes)
			AttackComponentSuggestions.Add(type);
	}
}

internal record StructuredLootPackSpan(int PackNumber, int ArgumentsStart, int ArgumentsLength,
	int CommentStart = -1, int CommentLength = 0, int FullStart = -1, int FullLength = 0);

public class StructuredOnSpawnModel : ObservableObject
{
	private IReadOnlyList<RoslynConstructorDescriptor> _constructorDescriptors = Array.Empty<RoslynConstructorDescriptor>();
	private string? _spellCollectionConstructorSignature;
	private string _sourceSnapshot = String.Empty;
	private string _creationPattern = "Custom/unsupported";
	private string _configurationVariable = "configuration";
	private string _configurationType = "CreatureDTO";
	private string _entityVariable = "creature";
	private string _entityType = "CreatureEntity";
	private string _entityConstructorArguments = "";
	private int _entityConstructorArgumentsStart = -1;
	private int _entityConstructorArgumentsLength;
	private int _configurationInitializerStart = -1;
	private int _configurationInitializerEnd = -1;
	private string _baseMobVariable = "baseMob";
	private string _baseMobType = "";
	private IReadOnlyDictionary<string, IReadOnlyList<string>> _knownActionArguments =
		new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
	private IReadOnlyList<string> _weaponSuggestions = Array.Empty<string>();
	private IReadOnlyList<string> _equipmentSuggestions = Array.Empty<string>();
	private IReadOnlyList<string> _creatureDefenseSuggestions = Array.Empty<string>();
	private IReadOnlyList<string> _spellStatusSuggestions = Array.Empty<string>();
	private IReadOnlyList<string> _spellStatusSuggestionTemplates = Array.Empty<string>();
	private bool _canUseCounters;
	private static readonly Regex DeclarationRegex = new Regex(
		@"(?:var|[A-Za-z_][A-Za-z0-9_.<>]*)\s+([A-Za-z_][A-Za-z0-9_]*)\s*=\s*new\s+([A-Za-z_][A-Za-z0-9_.<>]*)",
		RegexOptions.Compiled);
	private static readonly Regex PropertyRegex = new Regex(
		@"(?m)^(?<indent>[ \t]*)(?<comment>//\s*)?(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*(?<value>[^,;\r\n]+?)\s*(?:[,;]|(?=\r?$))",
		RegexOptions.Compiled);
	private static readonly Regex CollectionPropertyRegex = new Regex(
		@"(?m)^(?<indent>[ \t]*)(?<comment>//\s*)?(?<target>[A-Za-z_][A-Za-z0-9_]*)\.(?<name>Attacks|Spells|Blocks)\s*=\s*",
		RegexOptions.Compiled);
	private static readonly Regex ReturnRegex = new Regex(@"return\s+([A-Za-z_][A-Za-z0-9_]*)\s*;",
		RegexOptions.Compiled);

	private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Suggestions =
		new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
		{
			["Alignment"] = new[] { "Alignment.Lawful", "Alignment.Neutral", "Alignment.Chaotic", "Alignment.Evil" },
			["BasePenetration"] = new[] { "ShieldPenetration.None", "ShieldPenetration.VeryLight", "ShieldPenetration.Light", "ShieldPenetration.Medium", "ShieldPenetration.Heavy", "ShieldPenetration.VeryHeavy" },
			["Movement"] = new[] { "0", "1", "2", "3" },
			["CanLoot"] = new[] { "true", "false" }, ["CanFlee"] = new[] { "true", "false" },
			["CanSwim"] = new[] { "true", "false" }, ["CanFly"] = new[] { "true", "false" },
			["CanCharge"] = new[] { "true", "false" }, ["CanWalk"] = new[] { "true", "false" },
			["IsTethered"] = new[] { "true", "false" }, ["IsInvulnerable"] = new[] { "true", "false" },
			["CanStrikeCritically"] = new[] { "true", "false" }, ["CanRegenerateMana"] = new[] { "true", "false" },
			["CanRegenerateHealth"] = new[] { "true", "false" }, ["IsInvisible"] = new[] { "true", "false" },
			["NoCorpse"] = new[] { "true", "false" }, ["DropLeftHand"] = new[] { "true", "false" },
			["DropRightHand"] = new[] { "true", "false" }, ["DropPaperdollArmor"] = new[] { "true", "false" },
			["CanOrderFollow"] = new[] { "true", "false" }, ["CanOrderAttack"] = new[] { "true", "false" },
			["CanOrderCarry"] = new[] { "true", "false" }, ["CanRegenerateStamina"] = new[] { "true", "false" },
			["IsBoss"] = new[] { "true", "false" }, ["IsRare"] = new[] { "true", "false" },
			["IsKillCount"] = new[] { "true", "false" }, ["LootingOn"] = new[] { "true", "false" },
			["OnHear"] = new[] { "true", "false" },
			["CombatantSearchStrategy"] = new[] { "(attacker) => new WeakestSearchStrategy()", "(attacker) => new RandomSearchStrategy()" }
			,["Attacks"] = new[] { "new CreatureAttackCollection()" }
			,["Spells"] = new[] { "new CreatureSpellCollection()" }
			,["Blocks"] = new[] { "new CreatureBlockCollection()" }
			,["Weakness"] = new[] { "CreatureWeakness.None", "CreatureWeakness.Silver", "CreatureWeakness.BlueGlowing", "CreatureWeakness.DeathSpell", "CreatureWeakness.IceSpearSpell" }
			,["Immunity"] = new[] { "CreatureImmunity.None", "CreatureImmunity.Poison", "CreatureImmunity.Magic", "CreatureImmunity.Web", "CreatureImmunity.Bow", "CreatureImmunity.Piercing", "CreatureImmunity.Slashing", "CreatureImmunity.Bashing", "CreatureImmunity.Projectile" }
		};
	private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> MethodSuggestions =
		new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
		{
			["Wield"] = new[] { "new Longsword()", "new Greatsword()", "new BlackStaff()", "new WoodenStaff()", "new Longbow()", "new Spear()", "new FineCrossbow()", "new WoodenShield()" },
			["Equip"] = new[] { "new LeatherArmor()", "new ChainmailArmor()", "new PlatemailArmor()", "new DragonScaleArmor()", "new DrakeScaleArmor()" },
			["AddWeakness"] = new[] { "CreatureWeakness.Silver", "CreatureWeakness.BlueGlowing", "CreatureWeakness.DeathSpell", "CreatureWeakness.IceSpearSpell" },
			["AddImmunity"] = new[] { "CreatureImmunity.Poison", "CreatureImmunity.Magic", "CreatureImmunity.Web", "CreatureImmunity.Bow", "CreatureImmunity.Piercing", "CreatureImmunity.Slashing", "CreatureImmunity.Bashing", "CreatureImmunity.Projectile" },
			["AddGold"] = new[] { "0", "10", "20", "30", "50", "100" }
		};

	public IReadOnlyList<string> CreationPatterns { get; } =
		new[] { "Direct creature", "CreatureDTO", "CreatureBloodDTO", "Mu mobile entity", "Custom/unsupported" };
	public string CreationPattern
	{
		get => _creationPattern;
		set
		{
			if (SetProperty(ref _creationPattern, value))
			{
				OnPropertyChanged(nameof(CommonPropertyNames));
				OnPropertyChanged(nameof(AvailablePropertyNames));
			}
		}
	}
	public string ConfigurationVariable { get => _configurationVariable; set => SetProperty(ref _configurationVariable, value); }
	public string ConfigurationType { get => _configurationType; set => SetProperty(ref _configurationType, value); }
	public string EntityVariable
	{
		get => _entityVariable;
		set
		{
			if (!SetProperty(ref _entityVariable, value)) return;
			RefreshSpellStatusSuggestions();
		}
	}
	public string EntityType { get => _entityType; set => SetProperty(ref _entityType, value); }
	public string EntityConstructorArguments
	{
		get => _entityConstructorArguments;
		set => SetProperty(ref _entityConstructorArguments, value);
	}
	public string BaseMobVariable { get => _baseMobVariable; set => SetProperty(ref _baseMobVariable, value); }
	public string BaseMobType { get => _baseMobType; set => SetProperty(ref _baseMobType, value); }
	public IReadOnlyList<string> KnownEntityVariables { get; private set; } = Array.Empty<string>();
	public IReadOnlyList<string> KnownEntityTypes { get; private set; } = Array.Empty<string>();
	public IReadOnlyList<string> KnownConfigurationVariables { get; private set; } = Array.Empty<string>();
	public IReadOnlyList<string> KnownConfigurationTypes { get; private set; } = Array.Empty<string>();
	public IReadOnlyList<string> KnownBaseMobVariables { get; private set; } = Array.Empty<string>();
	public IReadOnlyList<string> KnownBaseMobTypes { get; private set; } = Array.Empty<string>();
	public string ConfigurationDescription { get; private set; } = "None detected";
	public string EntityDescription { get; private set; } = "No returned entity detected";
	public ObservableCollection<StructuredOnSpawnProperty> Properties { get; } = new ObservableCollection<StructuredOnSpawnProperty>();
	public ObservableCollection<StructuredOnSpawnMethod> Methods { get; } = new ObservableCollection<StructuredOnSpawnMethod>();
	public ObservableCollection<StructuredOnSpawnProperty> OrdinaryProperties { get; } = new ObservableCollection<StructuredOnSpawnProperty>();
	public ObservableCollection<StructuredOnSpawnProperty> AttackProperties { get; } = new ObservableCollection<StructuredOnSpawnProperty>();
	public ObservableCollection<StructuredOnSpawnProperty> SpellProperties { get; } = new ObservableCollection<StructuredOnSpawnProperty>();
	public ObservableCollection<StructuredOnSpawnProperty> BlockProperties { get; } = new ObservableCollection<StructuredOnSpawnProperty>();
	public ObservableCollection<StructuredOnSpawnMethod> DefenseMethods { get; } = new ObservableCollection<StructuredOnSpawnMethod>();
	public ObservableCollection<StructuredOnSpawnMethod> OtherMethods { get; } = new ObservableCollection<StructuredOnSpawnMethod>();
	public ObservableCollection<StructuredCollectionItem> AttackItems { get; } = new ObservableCollection<StructuredCollectionItem>();
	public ObservableCollection<StructuredCollectionItem> BlockItems { get; } = new ObservableCollection<StructuredCollectionItem>();
	public ObservableCollection<StructuredCollectionItem> SpellItems { get; } = new ObservableCollection<StructuredCollectionItem>();
	public ObservableCollection<StructuredCollectionArgument> SpellCollectionFields { get; } =
		new ObservableCollection<StructuredCollectionArgument>();
	public IReadOnlyList<StructuredConstructorOption> SpellCollectionConstructors => _constructorDescriptors
		.Where(item => item.TypeName == "CreatureSpellCollection")
		.Select(item => new StructuredConstructorOption(item.Signature, item.Signature, item)).ToArray();
	public string? SpellCollectionConstructor
	{
		get => _spellCollectionConstructorSignature;
		set
		{
			var option = SpellCollectionConstructors.FirstOrDefault(item => item.Arguments == value);
			if (option?.Descriptor == null) return;
			_spellCollectionConstructorSignature = option.Arguments;
			var existing = GetSpellCollectionArguments();
			var values = option.Descriptor.Parameters.Where(parameter => !parameter.IsParams)
				.Select((parameter, index) => index < existing.Count ? existing[index] :
					GetSpellCollectionDefault(parameter)).ToArray();
			SetSpellCollectionArguments(values);
			BuildSpellCollectionFields(option.Descriptor, values);
			OnPropertyChanged();
		}
	}
	private void RefreshSpellCollectionConstructor()
	{
		var arguments = GetSpellCollectionArguments();
		var descriptor = _constructorDescriptors.Where(item => item.TypeName == "CreatureSpellCollection")
			.FirstOrDefault(item => item.Parameters.Count(parameter => !parameter.IsParams) == arguments.Count)
			?? _constructorDescriptors.FirstOrDefault(item => item.TypeName == "CreatureSpellCollection");
		_spellCollectionConstructorSignature = descriptor?.Signature;
		SpellCollectionFields.Clear();
		if (descriptor != null) BuildSpellCollectionFields(descriptor, arguments);
		OnPropertyChanged(nameof(SpellCollectionConstructor));
	}
	private void BuildSpellCollectionFields(RoslynConstructorDescriptor descriptor, IReadOnlyList<string> arguments)
	{
		SpellCollectionFields.Clear();
		var parameters = descriptor.Parameters.Where(parameter => !parameter.IsParams).ToArray();
		for (var index = 0; index < parameters.Length; index++)
		{
			var parameter = parameters[index];
			var value = index < arguments.Count ? arguments[index] : GetSpellCollectionDefault(parameter);
			var field = new StructuredCollectionArgument(
				$"{Regex.Replace(parameter.Name, "([a-z0-9])([A-Z])", "$1 $2")} ({parameter.TypeName})",
				value, null, parameter.Suggestions);
			field.PropertyChanged += (_, args) =>
			{
				if (args.PropertyName == nameof(StructuredCollectionArgument.Value))
					SetSpellCollectionArguments(SpellCollectionFields.Select(item => item.Value));
			};
			SpellCollectionFields.Add(field);
		}
	}
	private static string GetSpellCollectionDefault(RoslynConstructorParameter parameter)
	{
		if (parameter.IsOptional && !String.IsNullOrWhiteSpace(parameter.DefaultValue)) return parameter.DefaultValue;
		if (parameter.Suggestions.Count > 0) return parameter.Suggestions[0];
		if (parameter.TypeName is "string" or "String") return "\"\"";
		if (parameter.TypeName is "bool" or "Boolean") return "false";
		return "0";
	}
	private List<string> GetSpellCollectionArguments()
	{
		var property = Properties.FirstOrDefault(item => item.Name == "Spells");
		if (property == null) return new List<string>();
		var match = Regex.Match(property.Value, @"new\s+CreatureSpellCollection\s*\((?<args>[^)]*)\)", RegexOptions.Singleline);
		return match.Success ? SplitTopLevel(match.Groups["args"].Value).Select(value => value.Trim())
			.Where(value => value.Length > 0).ToList() : new List<string>();
	}
	private void SetSpellCollectionArguments(IEnumerable<string> arguments)
	{
		var property = Properties.FirstOrDefault(item => item.Name == "Spells");
		if (property == null) return;
		property.Value = Regex.Replace(property.Value,
			@"new\s+CreatureSpellCollection\s*\([^)]*\)",
			$"new CreatureSpellCollection({String.Join(", ", arguments)})", RegexOptions.Singleline);
	}
	public ObservableCollection<StructuredLootItem> LootItems { get; } = new ObservableCollection<StructuredLootItem>();
	public ObservableCollection<StructuredStatItem> StatItems { get; } = new ObservableCollection<StructuredStatItem>();
	private IReadOnlyList<string> _entityStatNames = Array.Empty<string>();
	private readonly List<(int Start, int Length)> _statSpans = new List<(int Start, int Length)>();
	public void SetEntityStatNames(IEnumerable<string> names)
	{
		_entityStatNames = names.Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToArray();
		foreach (var item in StatItems) item.SetKnownStats(_entityStatNames);
	}
	public void AddStat()
	{
		StatItems.Add(new StructuredStatItem(_entityStatNames.FirstOrDefault() ?? "Health", "Base", "0", true,
			_entityStatNames));
	}
	private readonly List<StructuredLootPackSpan> _lootPackSpans = new List<StructuredLootPackSpan>();
	private readonly List<StructuredOnSpawnProperty> _parsedProperties = new List<StructuredOnSpawnProperty>();
	private readonly List<StructuredOnSpawnMethod> _parsedMethods = new List<StructuredOnSpawnMethod>();
	private IReadOnlyList<string> _treasureNames = Array.Empty<string>();
	private IReadOnlyList<string> _lootCreateSuggestions = Array.Empty<string>();
	private string _lootAmount = "";
	public string LootAmount { get => _lootAmount; set => SetProperty(ref _lootAmount, value ?? ""); }
	public IReadOnlyList<string> AttackTypes { get; } = new[] { "CreatureAttack", "CreatureBasicAttack" };
	public IReadOnlyList<string> BlockTypes { get; } = new[] { "CreatureBlock" };
	public IReadOnlyList<string> SpellTypes { get; private set; } = new[]
	{
		"CreatureSpell<DeathSpell>", "CreatureSpell<IceStormSpell>", "CreatureSpell<MagicMissileSpell>",
		"CreatureSpell<LightningBoltSpell>", "CreatureSpell<CurseSpell>", "CreatureSpell<IceSpearSpell>",
		"CreatureSpell<ConcussionSpell>", "CreatureSpell<DarknessSpell>", "CreatureSpell<StunSpell>",
		"CreatureSpell<DragonBreathIceSpell>", "CreatureSpell<PoisonCloudSpell>", "CreatureSpell<FireStormSpell>",
		"CreatureSpell<FireballSpell>", "CreatureSpell<WhirlwindSpell>", "CreatureSpell<FirewallSpell>",
		"CreatureSpell<BlindSpell>", "CreatureSpell<FearSpell>", "CreatureSpell<CreateWebSpell>",
		"CreatureSpell<BonfireSpell>", "CreatureSpell<ShadowstepSpell>", "CreatureSpell<FireBoltSpell>",
		"CreatureSpell<DragonBreathFireSpell>", "CreatureSpell<CreateLavaSpell>", "CreatureSpell<AcidRainSpell>"
	};
	public void SetSpellTypes(IEnumerable<string> spellTypeNames)
	{
		var discovered = spellTypeNames
			.Where(name => !String.IsNullOrWhiteSpace(name))
			.Select(name => $"CreatureSpell<{name}>");
		SpellTypes = SpellTypes.Concat(discovered)
			.Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToArray();
		foreach (var item in SpellItems) item.SetKnownTypes(SpellTypes);
		OnPropertyChanged(nameof(SpellTypes));
	}

	public StructuredOnSpawnModel()
	{
		Properties.CollectionChanged += (_, _) => RefreshPropertyCategories();
		Methods.CollectionChanged += (_, _) => RefreshMethodCategories();
	}

	private void RefreshPropertyCategories()
	{
		Synchronize(OrdinaryProperties, Properties.Where(property => property.Name is not ("Attacks" or "Spells" or "Blocks")));
		Synchronize(AttackProperties, Properties.Where(property => property.Name == "Attacks"));
		Synchronize(SpellProperties, Properties.Where(property => property.Name == "Spells"));
		Synchronize(BlockProperties, Properties.Where(property => property.Name == "Blocks"));
	}

	private void RefreshMethodCategories()
	{
		Synchronize(DefenseMethods, Methods.Where(method => method.Name is "AddImmunity" or "AddWeakness" or "AddStatus"));
		Synchronize(OtherMethods, Methods.Where(method => method.Name is not ("AddImmunity" or "AddWeakness" or "AddStatus")));
	}

	private static void Synchronize<T>(ObservableCollection<T> target, IEnumerable<T> values)
	{
		target.Clear();
		foreach (var value in values) target.Add(value);
	}

	private void ParseCollectionItems()
	{
		ParseCollection(Properties.FirstOrDefault(p => p.Name == "Attacks"), AttackItems, AttackTypes);
		ParseCollection(Properties.FirstOrDefault(p => p.Name == "Blocks"), BlockItems, BlockTypes);
		ParseCollection(Properties.FirstOrDefault(p => p.Name == "Spells"), SpellItems, SpellTypes);
	}

	private static void ParseCollection(StructuredOnSpawnProperty? property, ObservableCollection<StructuredCollectionItem> target, IReadOnlyList<string> types)
	{
		target.Clear();
		if (property == null) return;
		var open = property.Value.IndexOf('{');
		var close = property.Value.LastIndexOf('}');
		if (open < 0 || close <= open) return;
		foreach (var raw in SplitTopLevel(property.Value.Substring(open + 1, close - open - 1)))
		{
			var expression = raw.Trim().TrimEnd(',').TrimEnd();
			if (String.IsNullOrWhiteSpace(expression)) continue;
			var enabled = !expression.StartsWith("//", StringComparison.Ordinal);
			if (!enabled) expression = expression.Substring(2).TrimStart();
			var typeMatch = Regex.Match(expression, @"new\s+(?<type>[A-Za-z_][A-Za-z0-9_]*(?:<[^>]+>)?)\s*\(");
			if (typeMatch.Success) target.Add(new StructuredCollectionItem(typeMatch.Groups["type"].Value, expression, enabled, types));
		}
	}

	public void AddCollectionItem(string kind, string? selectedType = null)
	{
		var target = kind == "Attacks" ? AttackItems : kind == "Blocks" ? BlockItems : SpellItems;
		var types = kind == "Attacks" ? AttackTypes : kind == "Blocks" ? BlockTypes : SpellTypes;
		if (!Properties.Any(p => p.Name == kind)) AddProperty(kind);
		var type = String.IsNullOrWhiteSpace(selectedType) ? types.First() : selectedType;
		var item = new StructuredCollectionItem(type, $"new {type}()", true, types);
		if (_constructorDescriptors.Count > 0)
			item.Arguments = "";
		item.SetConstructorDescriptors(_constructorDescriptors);
		item.InitializeFromRoslynDefaults();
		target.Add(item);
	}
	public void SetConstructorDescriptors(IEnumerable<RoslynConstructorDescriptor> constructors)
	{
		var catalog = constructors.ToArray();
		_constructorDescriptors = catalog;
		OnPropertyChanged(nameof(SpellCollectionConstructors));
		RefreshSpellCollectionConstructor();
		foreach (var item in AttackItems.Concat(BlockItems).Concat(SpellItems).Concat<StructuredCollectionItem>(LootItems))
			item.SetConstructorDescriptors(catalog);
	}
	private static readonly IReadOnlyList<string> DirectPropertyNames = new[]
	{
		"Name", "Body", "Hue", "Level", "MaxHealth", "Health", "MaxMana", "Mana", "Experience",
		"BaseDodge", "BasePenetration", "Movement", "VisibilityDistance", "HideDetection", "RangePerception",
		"Alignment", "Weakness", "Immunity", "CanLoot", "CanFlee", "CanSwim", "CanFly", "CanWalk",
		"CanCharge", "CanStrikeCritically", "CanJumpkick", "CanPanic", "IsTethered", "IsInvulnerable",
		"IsInvisible", "CanRegenerateMana", "CanRegenerateHealth", "AttackSound", "DeathSound",
		"NearbySound", "WarmSound", "CombatantChangeInterval", "CombatantSearchStrategy"
	};
	private static readonly IReadOnlyList<string> DtoPropertyNames = new[]
	{
		"BodyId", "Name", "Alignment", "Visibility", "VisionRange", "Health", "Mana", "Attack", "Defense",
		"HideDetection", "Movement", "EXP", "NearbySound", "AttackSound", "DeathSound", "level",
		"IsBoss", "IsRare", "IsKillCount"
	};
	private static readonly IReadOnlyList<string> BloodDtoArgumentNames = new[]
	{
		"BodyId", "Name", "Alignment", "Visibility", "Health", "Mana", "Attack", "Defense",
		"HideDetection", "Movement", "EXP"
	};
	private static readonly IReadOnlyList<string> BloodDtoPropertyNames = BloodDtoArgumentNames
		.Concat(DirectPropertyNames).Distinct(StringComparer.Ordinal).ToArray();
	private static readonly IReadOnlyList<string> MuPropertyNames = new[]
	{
		"Name", "Body", "NoCorpse", "DropLeftHand", "DropRightHand", "DropPaperdollArmor", "Hue", "Level",
		"Experience", "Health", "Mana", "Stamina", "BaseDodge", "HideDetection", "Movement", "VisionRange",
		"RangePerception", "VisibilityDistance", "BasePenetration", "Alignment", "CombatantSearchStrategy",
		"CombatantChangeInterval", "NearbySound", "AttackSound", "DeathSound", "WarmSound", "CanOrderFollow",
		"CanOrderAttack", "CanOrderCarry", "CanRegenerateHealth", "CanRegenerateMana", "CanRegenerateStamina",
		"CanLoot", "CanFly", "CanWalk", "CanSwim", "CanPanic", "CanFlee", "IsTethered", "CanCharge",
		"IsInvulnerable", "LootingOn", "MaxExperience", "OnHear"
	};
	public IReadOnlyList<string> CommonPropertyNames
	{
		get
		{
			if (String.Equals(CreationPattern, "Direct creature", StringComparison.OrdinalIgnoreCase))
				return DirectPropertyNames;
			if (String.Equals(CreationPattern, "CreatureDTO", StringComparison.OrdinalIgnoreCase))
				return DtoPropertyNames;
			if (String.Equals(CreationPattern, "CreatureBloodDTO", StringComparison.OrdinalIgnoreCase))
				return BloodDtoPropertyNames;
			if (String.Equals(CreationPattern, "Mu mobile entity", StringComparison.OrdinalIgnoreCase))
				return MuPropertyNames;
			return Array.Empty<string>();
		}
	}
	public IReadOnlyList<string> AvailablePropertyNames => CommonPropertyNames
		.Where(name => Properties.All(property => property.Name != name))
		.ToArray();
	public IReadOnlyList<string> CommonMethodNames { get; } = new[]
	{
		"AddStatus", "AddWeakness", "AddImmunity", "AddGold", "Wield", "Equip", "AddCounter"
	};
	public IReadOnlyList<string> OtherMethodNames { get; } = new[] { "AddGold", "Wield", "Equip", "AddCounter" };
	public IReadOnlyList<string> AvailableOtherMethodNames => OtherMethodNames
		.Where(name => name != "AddCounter" || _canUseCounters).ToArray();
	public void SetCanUseCounters(bool value)
	{
		if (_canUseCounters == value) return;
		_canUseCounters = value;
		OnPropertyChanged(nameof(AvailableOtherMethodNames));
	}
	public void SetCreatureDefenseSuggestions(IEnumerable<string> suggestions)
	{
		_creatureDefenseSuggestions = suggestions.Distinct(StringComparer.Ordinal)
			.OrderBy(value => value, StringComparer.Ordinal).ToArray();
		foreach (var method in Methods.Where(method => method.Name is "AddImmunity" or "AddWeakness"))
			method.SetSuggestedArguments(GetMethodSuggestions(method.Name));
	}
	public void SetSpellStatusSuggestions(IEnumerable<string> suggestions)
	{
		_spellStatusSuggestionTemplates = suggestions.Distinct(StringComparer.Ordinal).ToArray();
		RefreshSpellStatusSuggestions();
	}
	private void RefreshSpellStatusSuggestions()
	{
		_spellStatusSuggestions = _spellStatusSuggestionTemplates
			.Select(value => value.Replace("{entity}", _entityVariable))
			.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();
		foreach (var method in Methods.Where(method => method.Name == "AddStatus"))
			method.SetSuggestedArguments(GetMethodSuggestions(method.Name));
	}

	public static StructuredOnSpawnModel Parse(string? source, StructuredOnSpawnCatalog? catalog = null)
	{
		var model = new StructuredOnSpawnModel();
		model._sourceSnapshot = source ?? String.Empty;
		catalog ??= new StructuredOnSpawnCatalog();
		StructuredCollectionArgument.SetIntegerSuggestions(catalog.IntegerValues);
		model.KnownEntityVariables = catalog.EntityVariables.ToArray();
		model.KnownEntityTypes = catalog.EntityTypes.ToArray();
		model.KnownConfigurationVariables = WithNone(catalog.ConfigurationVariables);
		model.KnownConfigurationTypes = WithNone(catalog.ConfigurationTypes);
		model.KnownBaseMobVariables = WithNone(catalog.BaseMobVariables);
		model.KnownBaseMobTypes = WithNone(catalog.BaseMobTypes);
		model._knownActionArguments = catalog.ActionArguments.ToDictionary(
			entry => entry.Key, entry => (IReadOnlyList<string>)entry.Value.ToArray(), StringComparer.Ordinal);
		model._treasureNames = catalog.TreasureNames.ToArray();
		model._lootCreateSuggestions = catalog.LootCreateExpressions.ToArray();
		if (String.IsNullOrWhiteSpace(source))
			return model;

		var declarations = DeclarationRegex.Matches(source).Cast<Match>().ToList();
		var returnedVariable = ReturnRegex.Matches(source).Cast<Match>().LastOrDefault()?.Groups[1].Value;
		var returnedDeclaration = declarations.LastOrDefault(m => m.Groups[1].Value == returnedVariable);

		var returnedType = returnedDeclaration?.Groups[2].Value;
		var hasNestedMuConfiguration = Regex.IsMatch(source, @"(?m)^\s*Mv\s*=\s*new\s*(?:\(\)|[A-Za-z_])");
		var isMuEntity = returnedType?.StartsWith("Mu", StringComparison.Ordinal) == true &&
			returnedType.EndsWith("Entity", StringComparison.Ordinal);
		if (isMuEntity || hasNestedMuConfiguration || source.Contains("MuMobileEntity", StringComparison.Ordinal))
			model._creationPattern = "Mu mobile entity";
		else if (source.Contains("CreatureBloodDTO", StringComparison.Ordinal))
			model._creationPattern = "CreatureBloodDTO";
		else if (source.Contains("CreatureDTO", StringComparison.Ordinal))
			model._creationPattern = "CreatureDTO";
		else if (returnedDeclaration != null)
			model._creationPattern = "Direct creature";

		var configuration = declarations.FirstOrDefault(m =>
			m.Groups[2].Value.Contains("DTO", StringComparison.Ordinal) ||
			m.Groups[2].Value.Contains("MuMobileEntity", StringComparison.Ordinal));
		if (configuration != null)
		{
			model._configurationVariable = configuration.Groups[1].Value;
			model._configurationType = configuration.Groups[2].Value;
			model.ConfigurationDescription = $"{configuration.Groups[1].Value}: {configuration.Groups[2].Value}";
		}
		else if (model.CreationPattern == "Mu mobile entity")
		{
			model._configurationVariable = $"{returnedVariable ?? model._entityVariable}.Mv";
			model._configurationType = "MuVar.MuMobileEntity";
			var defaultsSource = declarations.FirstOrDefault(m => m != returnedDeclaration);
			if (defaultsSource != null)
			{
				model._baseMobVariable = defaultsSource.Groups[1].Value;
				model._baseMobType = defaultsSource.Groups[2].Value;
			}
			model.ConfigurationDescription = defaultsSource == null
				? $"{model._configurationVariable}: {model._configurationType} (inferred from target-typed new())"
				: $"{model._configurationVariable}: {model._configurationType} (inferred); defaults source {defaultsSource.Groups[1].Value}: {defaultsSource.Groups[2].Value}";
		}
		else
		{
			model._configurationVariable = "None";
			model._configurationType = "None";
			model._baseMobVariable = "None";
			model._baseMobType = "None";
		}

		if (String.IsNullOrWhiteSpace(model._baseMobType))
		{
			model._baseMobVariable = "None";
			model._baseMobType = "None";
		}

		if (returnedDeclaration != null)
		{
			model._entityVariable = returnedVariable!;
			model._entityType = returnedDeclaration.Groups[2].Value;
			var constructorOpen = source.IndexOf('(', returnedDeclaration.Index + returnedDeclaration.Length);
			if (constructorOpen >= 0)
			{
				var constructorClose = FindClosingParenthesis(source, constructorOpen);
				if (constructorClose > constructorOpen)
				{
					model._entityConstructorArgumentsStart = constructorOpen + 1;
					model._entityConstructorArgumentsLength = constructorClose - constructorOpen - 1;
					model._entityConstructorArguments = source.Substring(
						model._entityConstructorArgumentsStart, model._entityConstructorArgumentsLength).Trim();
				}
			}
			model.EntityDescription = $"{returnedVariable}: {returnedDeclaration.Groups[2].Value}";
		}
		else if (returnedVariable != null)
			model.EntityDescription = returnedVariable;

		if (model.CreationPattern is "CreatureDTO" or "CreatureBloodDTO")
			model.ParseCreatureDtoInitializer(source, catalog);
		model.ParseStats(source);

		var collectionRanges = new List<(int Start, int End)>();
		foreach (Match match in CollectionPropertyRegex.Matches(source))
		{
			var valueStart = match.Index + match.Length;
			var statementEnd = FindStatementEnd(source, valueStart);
			if (statementEnd < 0)
				continue;

			var valueEnd = statementEnd;
			while (valueEnd > valueStart && Char.IsWhiteSpace(source[valueEnd - 1])) valueEnd--;
			var name = match.Groups["name"].Value;
			var suggestions = (Suggestions.TryGetValue(name, out var values) ? values : Array.Empty<string>())
				.Concat(catalog.GetIntegerValues(name)).Distinct(StringComparer.Ordinal).ToArray();
			var fullEnd = statementEnd + 1;
			if (fullEnd < source.Length && source[fullEnd] == '\r') fullEnd++;
			if (fullEnd < source.Length && source[fullEnd] == '\n') fullEnd++;
			var property = new StructuredOnSpawnProperty(name,
				source.Substring(valueStart, valueEnd - valueStart), suggestions,
				valueStart, valueEnd - valueStart, match.Groups["target"].Value,
				match.Index, fullEnd - match.Index, !match.Groups["comment"].Success,
				match.Groups["indent"].Index + match.Groups["indent"].Length,
				match.Groups["comment"].Length);
			model.Properties.Add(property);
			model._parsedProperties.Add(property);
			collectionRanges.Add((match.Index, fullEnd));
		}

		foreach (Match match in PropertyRegex.Matches(source))
		{
			if (model._configurationInitializerStart >= 0 &&
				match.Index < model._configurationInitializerEnd &&
				match.Index + match.Length > model._configurationInitializerStart)
				continue;
			if (collectionRanges.Any(range => match.Index >= range.Start && match.Index < range.End))
				continue;
			var name = match.Groups["name"].Value;
			var valueGroup = match.Groups["value"];
			var suggestions = (Suggestions.TryGetValue(name, out var values) ? values : Array.Empty<string>())
				.Concat(catalog.GetIntegerValues(name)).Distinct(StringComparer.Ordinal).ToArray();
			var fullEnd = source.IndexOf('\n', match.Index + match.Length);
			if (fullEnd < 0) fullEnd = match.Index + match.Length;
			else fullEnd++;
			var targetVariable = model.CreationPattern is "CreatureDTO" or "CreatureBloodDTO"
				? model._entityVariable : null;
			var property = new StructuredOnSpawnProperty(name, valueGroup.Value.Trim(), suggestions,
				valueGroup.Index, valueGroup.Length, targetVariable, fullStart: match.Index, fullLength: fullEnd - match.Index,
				isEnabled: !match.Groups["comment"].Success,
				commentStart: match.Groups["indent"].Index + match.Groups["indent"].Length,
				commentLength: match.Groups["comment"].Length);
			model.Properties.Add(property);
			model._parsedProperties.Add(property);
		}

		model.ParseMethods(source);
		model.ParseCollectionItems();
		foreach (var lootItem in model.LootItems)
		{
			lootItem.SetLootTreasureNames(model._treasureNames);
			lootItem.SetLootCreateSuggestions(model._lootCreateSuggestions);
		}

		return model;
	}

	private void ParseCreatureDtoInitializer(string source, StructuredOnSpawnCatalog catalog)
	{
		var call = Regex.Match(source,
			$@"{Regex.Escape(_configurationVariable)}\s*=\s*{Regex.Escape(_configurationVariable)}\.SetCreatureValues\s*\(");
		if (!call.Success) return;
		var open = call.Index + call.Length - 1;
		var close = FindClosingParenthesis(source, open);
		if (close <= open) return;
		_configurationInitializerStart = call.Index;
		_configurationInitializerEnd = close + 1;
		var argumentsStart = open + 1;
		var arguments = source.Substring(argumentsStart, close - argumentsStart);
		var positionalNames = CreationPattern == "CreatureBloodDTO" ? BloodDtoArgumentNames : DtoPropertyNames;
		var position = 0;
		foreach (var argument in SplitArgumentSpans(arguments))
		{
			var text = arguments.Substring(argument.Start, argument.Length);
			var leadingWhitespace = text.Length - text.TrimStart().Length;
			var trimmed = text.Trim();
			if (trimmed.Length == 0) continue;
			var colon = FindDtoArgumentColon(trimmed);
			var rawName = colon >= 0
				? trimmed.Substring(0, colon).Trim()
				: position < positionalNames.Count ? positionalNames[position] : $"Argument{position + 1}";
			var name = DtoPropertyNames.FirstOrDefault(candidate =>
				String.Equals(candidate, rawName, StringComparison.OrdinalIgnoreCase)) ?? rawName switch
			{
				"visibilty" => "Visibility",
				"exp" => "EXP",
				_ => rawName
			};
			var valueText = colon >= 0 ? trimmed.Substring(colon + 1).Trim() : trimmed;
			var valueOffsetInTrimmed = colon >= 0
				? colon + 1 + trimmed.Substring(colon + 1).Length - trimmed.Substring(colon + 1).TrimStart().Length
				: 0;
			var valueStart = argumentsStart + argument.Start + leadingWhitespace + valueOffsetInTrimmed;
			var suggestions = (Suggestions.TryGetValue(name, out var known) ? known : Array.Empty<string>())
				.Concat(catalog.GetIntegerValues(name)).Distinct(StringComparer.Ordinal).ToArray();
			var property = new StructuredOnSpawnProperty(name, valueText, suggestions,
				valueStart, valueText.Length, _configurationVariable);
			Properties.Add(property);
			_parsedProperties.Add(property);
			position++;
		}
	}

	private static IEnumerable<(int Start, int Length)> SplitArgumentSpans(string source)
	{
		var start = 0;
		var depth = 0;
		var quoted = false;
		var quote = '\0';
		for (var index = 0; index < source.Length; index++)
		{
			var current = source[index];
			if (quoted)
			{
				if (current == quote && (index == 0 || source[index - 1] != '\\')) quoted = false;
				continue;
			}
			if (current is '\"' or '\'') { quoted = true; quote = current; continue; }
			if (current is '(' or '{' or '[') depth++;
			else if (current is ')' or '}' or ']') depth--;
			else if (current == ',' && depth == 0)
			{
				yield return (start, index - start);
				start = index + 1;
			}
		}
		if (start < source.Length) yield return (start, source.Length - start);
	}

	private static int FindDtoArgumentColon(string source)
	{
		var depth = 0;
		var quoted = false;
		var quote = '\0';
		for (var index = 0; index < source.Length; index++)
		{
			var current = source[index];
			if (quoted)
			{
				if (current == quote && (index == 0 || source[index - 1] != '\\')) quoted = false;
				continue;
			}
			if (current is '\"' or '\'') { quoted = true; quote = current; continue; }
			if (current is '(' or '{' or '[') depth++;
			else if (current is ')' or '}' or ']') depth--;
			else if (current == ':' && depth == 0) return index;
		}
		return -1;
	}

	private void ParseStats(string source)
	{
		var pattern = new Regex(
			$@"(?m)^(?<indent>[ \t]*)(?<comment>//\s*)?{Regex.Escape(_entityVariable)}\.Stats\[EntityStat\.(?<stat>[A-Za-z_][A-Za-z0-9_]*)\]\.(?:(?<assignment>Base|MaximumValue)\s*=\s*(?<value>[^;\r\n]+)|(?<method>Add|Remove)\s*\((?<args>[^;\r\n]*)\))\s*;[ \t]*(?:\r?\n)?");
		foreach (Match match in pattern.Matches(source))
		{
			var operation = match.Groups["method"].Success
				? match.Groups["method"].Value : match.Groups["assignment"].Value;
			var value = match.Groups["method"].Success
				? match.Groups["args"].Value.Trim() : match.Groups["value"].Value.Trim();
			StatItems.Add(new StructuredStatItem(match.Groups["stat"].Value, operation, value,
				!match.Groups["comment"].Success, _entityStatNames));
			_statSpans.Add((match.Index, match.Length));
		}
	}

	private void ParseMethods(string source)
	{
		var methodNames = String.Join("|", CommonMethodNames.Append("AddLoot").Select(Regex.Escape));
		var startRegex = new Regex($@"(?m)^(?<indent>\s*)(?<comment>//\s*)?{Regex.Escape(_entityVariable)}\.(?<name>{methodNames})\s*\(");
		foreach (Match match in startRegex.Matches(source))
		{
			var open = match.Index + match.Length - 1;
			var close = FindClosingParenthesis(source, open);
			if (close < 0)
				continue;

			var name = match.Groups["name"].Value;
			if (name == "AddLoot")
			{
				var lootStatementEnd = close + 1;
				while (lootStatementEnd < source.Length && source[lootStatementEnd] is ' ' or '\t') lootStatementEnd++;
				if (lootStatementEnd < source.Length && source[lootStatementEnd] == ';') lootStatementEnd++;
				if (lootStatementEnd < source.Length && source[lootStatementEnd] == '\r') lootStatementEnd++;
				if (lootStatementEnd < source.Length && source[lootStatementEnd] == '\n') lootStatementEnd++;
				ParseLootPack(source.Substring(open + 1, close - open - 1), open + 1, close - open - 1,
					match.Groups["comment"].Success,
					match.Groups["indent"].Index + match.Groups["indent"].Length,
					match.Groups["comment"].Length, match.Index, lootStatementEnd - match.Index);
				continue;
			}
			var statementEnd = close + 1;
			while (statementEnd < source.Length && source[statementEnd] is ' ' or '\t') statementEnd++;
			if (statementEnd < source.Length && source[statementEnd] == ';') statementEnd++;
			if (statementEnd < source.Length && source[statementEnd] == '\r') statementEnd++;
			if (statementEnd < source.Length && source[statementEnd] == '\n') statementEnd++;
			var method = new StructuredOnSpawnMethod(name,
				source.Substring(open + 1, close - open - 1).Trim(),
				GetMethodSuggestions(name),
				open + 1, close - open - 1,
				!match.Groups["comment"].Success,
				match.Groups["indent"].Index + match.Groups["indent"].Length,
				match.Groups["comment"].Length, match.Index, statementEnd - match.Index);
			Methods.Add(method);
			_parsedMethods.Add(method);
		}
	}

	private void ParseLootPack(string arguments, int argumentsStart, int argumentsLength,
		bool packIsCommented, int commentStart, int commentLength, int fullStart, int fullLength)
	{
		var packNumber = _lootPackSpans.Count + 1;
		_lootPackSpans.Add(new StructuredLootPackSpan(packNumber, argumentsStart, argumentsLength,
			commentStart, commentLength, fullStart, fullLength));
		// A disabled pack is commonly written with // at the start of every line. Remove
		// those continuation markers for structural parsing; the item state is retained below.
		var parseArguments = packIsCommented
			? Regex.Replace(arguments, @"(?m)^(\s*)//\s?", "$1")
			: arguments;
		var outerArguments = SplitTopLevel(parseArguments).Select(value => value.Trim())
			.Where(value => value.Length > 0).ToList();
		var lootPackArgument = outerArguments.FirstOrDefault() ?? parseArguments.Trim();
		if (outerArguments.Count > 1)
		{
			var amount = outerArguments[1];
			var colon = amount.IndexOf(':');
			LootAmount = colon > 0 && amount.Substring(0, colon).Trim()
				.Equals("amount", StringComparison.OrdinalIgnoreCase)
				? amount.Substring(colon + 1).Trim() : amount;
		}
		var match = Regex.Match(lootPackArgument, @"^\s*new\s+LootPack\s*\((?<items>[\s\S]*)\)\s*$");
		if (!match.Success)
		{
			LootItems.Add(new StructuredLootItem(packNumber, lootPackArgument.Trim().TrimEnd(','), !packIsCommented));
			return;
		}

		// Commented entries often intentionally omit their comma. Treat every top-level
		// commented LootPackEntry line as a new item even when the prior line has none.
		var lootSource = Regex.Replace(match.Groups["items"].Value,
			@"(?m)^(?<indent>[ \t]*)//\s*,\s*(?=new\s+LootPackEntry\s*\()",
			"${indent},//");
		// Put an explicit separator before every entry line. Existing commas may produce
		// empty split fragments, which are ignored below. This also separates an enabled
		// entry following a final commented entry that intentionally has no comma.
		var separatedItems = Regex.Replace(lootSource,
			@"(?m)(?=^[ \t]*(?://\s*)?new\s+LootPackEntry\s*\()", ",");
		foreach (var item in SplitTopLevel(separatedItems))
			if (!String.IsNullOrWhiteSpace(item))
			{
				var expression = item.Trim().TrimEnd().TrimEnd(',').TrimEnd();
				var expressionIsCommented = expression.StartsWith("//", StringComparison.Ordinal);
				var enabled = !packIsCommented && !expressionIsCommented;
				if (expressionIsCommented) expression = expression.Substring(2).TrimStart();
				LootItems.Add(new StructuredLootItem(packNumber, expression, enabled));
			}
	}

	private string FormatLootPack(StructuredLootPackSpan pack)
	{
		var items = LootItems.Where(item => item.PackNumber == pack.PackNumber).ToList();
		var entries = items.Select((item, index) =>
		{
			var expression = item.Expression.Trim().TrimEnd(',').TrimEnd();
			// Enabled and commented entries form separate effective lists. A disabled
			// entry must not force a comma onto the final active C# argument.
			var comma = items.Skip(index + 1).Any(next => next.IsEnabled == item.IsEnabled) ? "," : "";
			return $"        {(item.IsEnabled ? "" : "//")}{expression}{comma}";
		});
		return $"new LootPack(\r\n{String.Join("\r\n", entries)}\r\n    )";
	}

	private static IEnumerable<string> SplitTopLevel(string source)
	{
		var start = 0;
		var depth = 0;
		var inString = false;
		var quote = '\0';
		for (var index = 0; index < source.Length; index++)
		{
			var current = source[index];
			if (inString)
			{
				if (current == quote && (index == 0 || source[index - 1] != '\\')) inString = false;
				continue;
			}
			if (current is '\"' or '\'') { inString = true; quote = current; continue; }
			if (current is '(' or '{' or '[') depth++;
			else if (current is ')' or '}' or ']') depth--;
			else if (current == ',' && depth == 0)
			{
				yield return source.Substring(start, index - start);
				start = index + 1;
			}
		}
		yield return source.Substring(start);
	}

	private static int FindClosingParenthesis(string source, int open)
	{
		var depth = 0;
		var inString = false;
		var quote = '\0';
		for (var index = open; index < source.Length; index++)
		{
			var current = source[index];
			if (inString)
			{
				if (current == quote && (index == 0 || source[index - 1] != '\\'))
					inString = false;
				continue;
			}
			if (current is '\"' or '\'')
			{
				inString = true;
				quote = current;
				continue;
			}
			if (current == '(') depth++;
			if (current == ')' && --depth == 0) return index;
		}
		return -1;
	}

	private static int FindStatementEnd(string source, int start)
	{
		var parentheses = 0;
		var braces = 0;
		var brackets = 0;
		var inString = false;
		var quote = '\0';
		for (var index = start; index < source.Length; index++)
		{
			var current = source[index];
			if (inString)
			{
				if (current == quote && (index == 0 || source[index - 1] != '\\')) inString = false;
				continue;
			}
			if (current is '\"' or '\'') { inString = true; quote = current; continue; }
			if (current == '(') parentheses++;
			else if (current == ')') parentheses--;
			else if (current == '{') braces++;
			else if (current == '}') braces--;
			else if (current == '[') brackets++;
			else if (current == ']') brackets--;
			else if (current == ';' && parentheses == 0 && braces == 0 && brackets == 0) return index;
		}
		return -1;
	}

	public void AddProperty(string name)
	{
		if (Properties.Any(property => property.Name == name))
			return;
		var suggestions = Suggestions.TryGetValue(name, out var values) ? values : Array.Empty<string>();
		var isEntityCollection = name is "Attacks" or "Spells" or "Blocks";
		var isDtoArgument = CreationPattern == "CreatureDTO" && DtoPropertyNames.Contains(name) ||
			CreationPattern == "CreatureBloodDTO" && BloodDtoArgumentNames.Contains(name);
		var target = !isEntityCollection && (isDtoArgument || CreationPattern == "Mu mobile entity")
			? _configurationVariable : _entityVariable;
		Properties.Add(new StructuredOnSpawnProperty(name, suggestions.FirstOrDefault() ?? "0", suggestions, -1, 0, target));
		OnPropertyChanged(nameof(AvailablePropertyNames));
	}

	public void RemoveProperty(StructuredOnSpawnProperty property)
	{
		if (Properties.Remove(property))
			OnPropertyChanged(nameof(AvailablePropertyNames));
	}

	public void AddMethod(string name)
	{
		var arguments = name switch
		{
			"AddGold" => "0",
			"Wield" => "new Longsword()",
			"Equip" => "new LeatherArmor()",
			"AddStatus" => $"new Status({_entityVariable})",
			"AddWeakness" => "CreatureWeakness.None",
			"AddImmunity" => "CreatureImmunity.None",
			"AddCounter" => "new Point2D(0, 0, 0)",
			"AddLoot" => "new LootPack()",
			_ => ""
		};
		Methods.Add(new StructuredOnSpawnMethod(name, arguments, GetMethodSuggestions(name)));
	}

	public void AddLootItem()
	{
		var packNumber = _lootPackSpans.Count > 0 ? _lootPackSpans[0].PackNumber : 1;
		var item = new StructuredLootItem(packNumber, "new LootPackEntry()");
		item.SetLootTreasureNames(_treasureNames);
		item.SetLootCreateSuggestions(_lootCreateSuggestions);
		item.SetConstructorDescriptors(_constructorDescriptors);
		item.InitializeFromRoslynDefaults();
		LootItems.Add(item);
	}

	private IReadOnlyList<string> GetMethodSuggestions(string name)
	{
		if (name == "AddLoot")
			return Array.Empty<string>();

		var values = new SortedSet<string>(StringComparer.Ordinal);
		if (MethodSuggestions.TryGetValue(name, out var builtIn))
			values.UnionWith(builtIn);
		if (name is "AddImmunity" or "AddWeakness")
			values.UnionWith(_creatureDefenseSuggestions);
		if (name == "AddStatus") values.UnionWith(_spellStatusSuggestions);
		if (name != "AddStatus" && _knownActionArguments.TryGetValue(name, out var known))
			values.UnionWith(known);
		if (name == "Wield")
			values.UnionWith(_weaponSuggestions);
		if (name == "Equip")
			values.UnionWith(_equipmentSuggestions);
		return values.ToArray();
	}
	public void SetWeaponTypes(IEnumerable<string> typeNames)
	{
		_weaponSuggestions = typeNames.Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal)
			.Select(name => $"new {name}()").ToArray();
		foreach (var method in Methods.Where(method => method.Name == "Wield"))
			method.SetSuggestedArguments(GetMethodSuggestions("Wield"));
	}
	public void SetEquipmentTypes(IEnumerable<string> typeNames)
	{
		_equipmentSuggestions = typeNames.Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal)
			.Select(name => $"new {name}()").ToArray();
		foreach (var method in Methods.Where(method => method.Name == "Equip"))
			method.SetSuggestedArguments(GetMethodSuggestions("Equip"));
	}

	public string CreateSkeleton()
	{
		var configVariable = IsNone(ConfigurationVariable) ? "configuration" : ConfigurationVariable;
		var configType = IsNone(ConfigurationType) ? "CreatureDTO" : ConfigurationType;
		var entityVariable = String.IsNullOrWhiteSpace(EntityVariable) ? "creature" : EntityVariable;
		var entityType = String.IsNullOrWhiteSpace(EntityType) ? "CreatureEntity" : EntityType;
		var entityArguments = EntityConstructorArguments?.Trim() ?? "";
		var baseVariable = IsNone(BaseMobVariable) ? "baseMob" : BaseMobVariable;
		var hasBaseMob = !IsNone(BaseMobVariable) && !IsNone(BaseMobType);
		var baseDeclaration = hasBaseMob ? $"    var {baseVariable} = new {BaseMobType}();\r\n\r\n" : "";

		return CreationPattern switch
		{
			"CreatureDTO" => CreateCreatureDtoSkeleton(configVariable, configType, entityVariable, entityType),
			"CreatureBloodDTO" => $"    var {configVariable} = new CreatureBloodDTO();\r\n    {configVariable} = {configVariable}.SetCreatureValues(0, \"creature\", 0, 3, 10, 0, 1, 1, 1, 1, 0);\r\n\r\n    var {entityVariable} = new CreatureBloodDefaultEntity({configVariable});\r\n\r\n    return {entityVariable};",
			"Mu mobile entity" when hasBaseMob => $"{baseDeclaration}    var {entityVariable} = new {entityType}()\r\n    {{\r\n        Mv = new MuVar.MuMobileEntity()\r\n        {{\r\n            Name = {baseVariable}.Name,\r\n            Body = {baseVariable}.Body,\r\n            Alignment = {baseVariable}.Alignment,\r\n        }}\r\n    }};\r\n\r\n    return {entityVariable};",
			"Mu mobile entity" => $"    var {entityVariable} = new {entityType}()\r\n    {{\r\n        Mv = new MuVar.MuMobileEntity()\r\n        {{\r\n            Name = \"creature\",\r\n        }}\r\n    }};\r\n\r\n    return {entityVariable};",
			_ => $"{baseDeclaration}    var {entityVariable} = new {entityType}({entityArguments})\r\n    {{\r\n    }};\r\n\r\n    return {entityVariable};"
		};
	}

	private static bool IsNone(string? value) =>
		String.IsNullOrWhiteSpace(value) || String.Equals(value, "None", StringComparison.OrdinalIgnoreCase);

	private static IReadOnlyList<string> WithNone(IEnumerable<string> values) =>
		new[] { "None" }.Concat(values.Where(value => !String.Equals(value, "None", StringComparison.OrdinalIgnoreCase)))
			.ToArray();

	private static string CreateCreatureDtoSkeleton(string configVariable, string configType,
		string entityVariable, string entityType) =>
		$"    {configType} {configVariable} = new {configType}();\r\n" +
		$"    {configVariable} = {configVariable}.SetCreatureValues\r\n" +
		"    (\r\n" +
		"        bodyId: 0,\r\n" +
		"        name: \"creature\",\r\n" +
		"        alignment: 0,\r\n" +
		"        visibilty: 3,\r\n" +
		"        visionRange: 3,\r\n" +
		"        health: 10,\r\n" +
		"        mana: 0,\r\n" +
		"        attack: 1,\r\n" +
		"        defense: 1,\r\n" +
		"        hideDetection: 1,\r\n" +
		"        movement: 1,\r\n" +
		"        exp: 0,\r\n" +
		"        nearbySound: 0,\r\n" +
		"        attackSound: 0,\r\n" +
		"        deathSound: 0,\r\n" +
		"        level: 1\r\n" +
		"    );\r\n\r\n" +
		$"    var {entityVariable} = new {entityType}({configVariable});\r\n\r\n" +
		$"    return {entityVariable};";

	public string Apply(string source)
	{
		if (!String.Equals(source, _sourceSnapshot, StringComparison.Ordinal))
			return RebaseForApply(source).Apply(source);

		RebuildCollectionValue("Attacks", AttackItems, "CreatureAttackCollection");
		RebuildCollectionValue("Blocks", BlockItems, "CreatureBlockCollection");
		RebuildCollectionValue("Spells", SpellItems, "CreatureSpellCollection");
		var fieldEdits = OrdinaryProperties.Where(item => item.ValueStart >= 0)
			.Select(item => (Start: item.ValueStart, Length: item.ValueLength, Value: item.Value))
			.Concat(OrdinaryProperties.Where(item => item.CommentStart >= 0)
				.Select(item => (Start: item.CommentStart, Length: item.CommentLength,
					Value: item.IsEnabled ? "" : "//")))
			.Concat(_entityConstructorArgumentsStart >= 0
				? new[] { (Start: _entityConstructorArgumentsStart, Length: _entityConstructorArgumentsLength,
					Value: EntityConstructorArguments ?? "") }
				: Array.Empty<(int Start, int Length, string Value)>());
		var removals = _parsedProperties.Where(item => item.Name is "Attacks" or "Blocks" or "Spells")
			.Select(item => (Start: item.FullStart, Length: item.FullLength))
			.Concat(_parsedMethods.Select(item => (Start: item.FullStart, Length: item.FullLength)))
			.Concat(_lootPackSpans.Select(item => (Start: item.FullStart, Length: item.FullLength)))
			.Concat(_statSpans.Select(item => (Start: item.Start, Length: item.Length)))
			.Where(item => item.Start >= 0 && item.Length > 0)
			.Distinct().Select(item => (item.Start, item.Length, Value: ""));
		foreach (var edit in fieldEdits.Concat(removals).OrderByDescending(item => item.Start))
			source = source.Remove(edit.Start, edit.Length).Insert(edit.Start, edit.Value);

		var sections = new List<string>();
		AddSection(sections, OrdinaryProperties.Where(item => item.ValueStart < 0).Select(FormatProperty));
		AddSection(sections, DefenseMethods.Select(FormatMethod));
		AddSection(sections, StatItems.Select(FormatStat));
		AddSection(sections, AttackProperties.Select(FormatProperty));
		AddSection(sections, BlockProperties.Select(FormatProperty));
		AddSection(sections, SpellProperties.Select(FormatProperty));
		AddSection(sections, OtherMethods.Select(FormatMethod));
		var lootPacks = LootItems.Select(item => item.PackNumber).Distinct().ToArray();
		AddSection(sections, lootPacks.Select(FormatLootPack));
		if (sections.Count > 0)
		{
			var returnMatch = ReturnRegex.Matches(source).Cast<Match>().LastOrDefault();
			var insertion = String.Join("\r\n\r\n", sections) + "\r\n\r\n";
			if (returnMatch != null)
			{
				var returnLineStart = source.LastIndexOf('\n', returnMatch.Index);
				returnLineStart = returnLineStart < 0 ? 0 : returnLineStart + 1;
				// Removed structured sections leave their separator lines behind. Rebuild the
				// whitespace immediately before return so repeated Apply operations do not
				// accumulate increasingly large blank gaps.
				var beforeReturn = source.Substring(0, returnLineStart).TrimEnd();
				var returnAndAfter = source.Substring(returnLineStart);
				source = beforeReturn + "\r\n\r\n" + insertion + returnAndAfter;
			}
			else
				source += "\r\n" + insertion;
		}

		return source;
	}

	private static void AddSection(ICollection<string> sections, IEnumerable<string> lines)
	{
		var section = String.Join("\r\n", lines.Where(line => !String.IsNullOrWhiteSpace(line)));
		if (section.Length > 0) sections.Add(section);
	}

	private string FormatProperty(StructuredOnSpawnProperty property)
	{
		var target = property.TargetVariable ??
			(CreationPattern is "CreatureDTO" or "CreatureBloodDTO" or "Mu mobile entity"
				? _configurationVariable : _entityVariable);
		return CommentLines($"    {target}.{property.Name} = {property.Value};", property.IsEnabled);
	}

	private string FormatMethod(StructuredOnSpawnMethod method) =>
		CommentLines($"    {_entityVariable}.{method.Name}({method.Arguments});", method.IsEnabled);

	private string FormatStat(StructuredStatItem item)
	{
		var isAssignment = item.Operation is "Base" or "MaximumValue";
		var expression = isAssignment
			? $"    {_entityVariable}.Stats[EntityStat.{item.Stat}].{item.Operation} = {item.Value};"
			: $"    {_entityVariable}.Stats[EntityStat.{item.Stat}].{item.Operation}({item.Value});";
		return CommentLines(expression, item.IsEnabled);
	}

	private string FormatLootPack(int packNumber)
	{
		var items = LootItems.Where(item => item.PackNumber == packNumber).ToList();
		var entries = items.Select((item, index) =>
			$"        {(item.IsEnabled ? "" : "//")}{item.Expression.Trim().TrimEnd(',').TrimEnd()}" +
			(items.Skip(index + 1).Any(next => next.IsEnabled == item.IsEnabled) ? "," : ""));
		var amount = String.IsNullOrWhiteSpace(LootAmount) ? "" :
			$",\r\n        amount: {LootAmount.Trim()}";
		return $"    {_entityVariable}.AddLoot(new LootPack(\r\n{String.Join("\r\n", entries)}\r\n    ){amount}\r\n    );";
	}

	private static string CommentLines(string value, bool isEnabled) => isEnabled ? value :
		String.Join("\r\n", value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
			.Select(line => Regex.Replace(line, @"^(\s*)", "$1//", RegexOptions.None,
				TimeSpan.FromMilliseconds(100))));

	private StructuredOnSpawnModel RebaseForApply(string source)
	{
		var rebased = Parse(source);
		var baseline = Parse(_sourceSnapshot);
		rebased.SetConstructorDescriptors(_constructorDescriptors);
		rebased.SetWeaponTypes(_weaponSuggestions.Select(value => Regex.Match(value, @"new\s+(?<type>[A-Za-z_][A-Za-z0-9_]*)").Groups["type"].Value)
			.Where(value => value.Length > 0));
		rebased.SetEquipmentTypes(_equipmentSuggestions.Select(value => Regex.Match(value, @"new\s+(?<type>[A-Za-z_][A-Za-z0-9_]*)").Groups["type"].Value)
			.Where(value => value.Length > 0));

		var unmatchedProperties = rebased.Properties.ToList();
		foreach (var current in Properties)
		{
			var original = baseline.Properties.FirstOrDefault(property => property.Name == current.Name &&
				String.Equals(property.TargetVariable, current.TargetVariable, StringComparison.Ordinal));
			var match = unmatchedProperties.FirstOrDefault(property => property.Name == current.Name &&
				String.Equals(property.TargetVariable, current.TargetVariable, StringComparison.Ordinal));
			if (match != null)
			{
				if (original == null || current.Value != original.Value || current.IsEnabled != original.IsEnabled)
				{
					match.Value = current.Value;
					match.IsEnabled = current.IsEnabled;
				}
				unmatchedProperties.Remove(match);
			}
			else
			{
				rebased.Properties.Add(new StructuredOnSpawnProperty(current.Name, current.Value,
					current.SuggestedValues, -1, 0, current.TargetVariable, isEnabled: current.IsEnabled));
			}
		}
		foreach (var removed in unmatchedProperties.Where(property => baseline.Properties.Any(original =>
			         original.Name == property.Name && String.Equals(original.TargetVariable, property.TargetVariable,
				         StringComparison.Ordinal))).ToArray())
			rebased.Properties.Remove(removed);

		var unmatchedMethods = rebased.Methods.ToList();
		foreach (var current in Methods)
		{
			var original = baseline.Methods.FirstOrDefault(method => method.Name == current.Name);
			var match = unmatchedMethods.FirstOrDefault(method => method.Name == current.Name);
			if (match != null)
			{
				if (original == null || current.Arguments != original.Arguments || current.IsEnabled != original.IsEnabled)
				{
					match.Arguments = current.Arguments;
					match.IsEnabled = current.IsEnabled;
				}
				unmatchedMethods.Remove(match);
			}
			else
			{
				rebased.Methods.Add(new StructuredOnSpawnMethod(current.Name, current.Arguments,
					current.SuggestedArguments, isEnabled: current.IsEnabled));
			}
		}
		foreach (var removed in unmatchedMethods.Where(method => baseline.Methods.Any(original =>
			         original.Name == method.Name)).ToArray())
			rebased.Methods.Remove(removed);

		MergeCollectionItems(AttackItems, baseline.AttackItems, rebased.AttackItems, rebased.AttackTypes, _constructorDescriptors);
		MergeCollectionItems(BlockItems, baseline.BlockItems, rebased.BlockItems, rebased.BlockTypes, _constructorDescriptors);
		MergeCollectionItems(SpellItems, baseline.SpellItems, rebased.SpellItems, rebased.SpellTypes, _constructorDescriptors);
		rebased.StatItems.Clear();
		rebased.SetEntityStatNames(_entityStatNames);
		foreach (var item in StatItems)
			rebased.StatItems.Add(new StructuredStatItem(item.Stat, item.Operation, item.Value, item.IsEnabled,
				_entityStatNames));
		rebased.LootItems.Clear();
		rebased.LootAmount = LootAmount;
		foreach (var item in LootItems)
			rebased.LootItems.Add(new StructuredLootItem(item.PackNumber, item.Expression, item.IsEnabled));
		return rebased;
	}

	private static void MergeCollectionItems(IReadOnlyList<StructuredCollectionItem> source,
		IReadOnlyList<StructuredCollectionItem> baseline,
		ObservableCollection<StructuredCollectionItem> target, IReadOnlyList<string> knownTypes,
		IReadOnlyList<RoslynConstructorDescriptor> constructors)
	{
		for (var index = 0; index < source.Count; index++)
		{
			var item = source[index];
			var changed = index >= baseline.Count || item.Expression != baseline[index].Expression ||
				item.IsEnabled != baseline[index].IsEnabled || item.Type != baseline[index].Type;
			if (!changed && index < target.Count) continue;
			var copy = new StructuredCollectionItem(item.Type, item.Expression, item.IsEnabled, knownTypes);
			copy.SetConstructorDescriptors(constructors);
			if (index < target.Count) target[index] = copy;
			else target.Add(copy);
		}
		if (source.Count < baseline.Count)
			while (target.Count > source.Count) target.RemoveAt(source.Count);
	}

	private void RebuildCollectionValue(string name, ObservableCollection<StructuredCollectionItem> items, string collectionType)
	{
		var property = Properties.FirstOrDefault(p => p.Name == name);
		if (property == null) return;
		var brace = property.Value.IndexOf('{');
		var prefix = brace >= 0 ? property.Value.Substring(0, brace).TrimEnd() : $"new {collectionType}()";
		var lines = items.Select((item, index) =>
		{
			var hasLaterPeer = items.Skip(index + 1).Any(next => next.IsEnabled == item.IsEnabled);
			var expressionLines = item.FormatForSource().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
			expressionLines[^1] += hasLaterPeer ? "," : "";
			return String.Join("\r\n", expressionLines.Select(line =>
				$"        {(item.IsEnabled ? "" : "//")}{line}"));
		});
		property.Value = $"{prefix}\r\n    {{\r\n{String.Join("\r\n", lines)}\r\n    }}";
	}
}
