#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kesmai.WorldForge.Editor.Assets;

/// <summary>
/// A form model of a whole OnSpawn script: it can be read from an existing script (<see cref="Parse"/>) and
/// written back as a complete body (<see cref="Generate"/>).
/// </summary>
/// <remarks>
/// Values are kept as the C# text they had in the script, so expressions such as <c>creatureDTO.Attack + 2</c> or
/// <c>.9</c> survive a read and write. Code the form has no field for is kept word for word in
/// <see cref="CodeBefore"/>, <see cref="OtherInitializers"/> and <see cref="OtherStatements"/>, so writing back a
/// script that was read never drops anything.
/// </remarks>
public partial class OnSpawnBuilder : ObservableObject
{
	public static readonly string[] FlagNames =
	[
		"CanFlee", "CanPanic", "CanSwim", "CanFly", "CanWalk", "CanLoot", "CanJumpkick", "CanCharge", "CanThrow",
		"CanStrikeCritically", "CanRegenerateHealth", "CanRegenerateMana", "IsTethered", "IsInvulnerable",
		"IsInvisible", "IsFemale", "NoCorpse",
	];

	public static readonly string[] HueModes = ["None", "Named", "Custom"];
	public static readonly string[] AttackTypes = ["CreatureBasicAttack", "CreatureAttack"];

	/// <summary>Raised whenever anything in the form changes, including list rows.</summary>
	public event EventHandler? Changed;

	private string _variable = "creature";
	private string _creatureType = "CreatureEntity";
	private string _constructorArguments = String.Empty;
	private string _name = String.Empty;
	private string _alignment = String.Empty;
	private string _maxHealth = String.Empty;
	private string _maxMana = String.Empty;
	private string _baseDodge = String.Empty;
	private string _experience = String.Empty;
	private string _movement = String.Empty;
	private string _hideDetection = String.Empty;
	private string _visibilityDistance = String.Empty;
	private string _visionRange = String.Empty;
	private string _rangePerception = String.Empty;
	private string _level = String.Empty;
	private string _nearbySound = String.Empty;
	private string _attackSound = String.Empty;
	private string _deathSound = String.Empty;
	private string _warmSound = String.Empty;
	private string _body = String.Empty;
	private string _hueMode = "None";
	private string _hueName = "Red";
	private byte _hueA = 255, _hueR = 255, _hueG = 255, _hueB = 255;
	private string _penetration = String.Empty;
	private string _searchStrategy = String.Empty;
	private string _weapon = String.Empty;
	private string _shield = String.Empty;
	private string _armor = String.Empty;
	private string _helmet = String.Empty;
	private string _gauntlets = String.Empty;
	private string _boots = String.Empty;
	private string _brain = String.Empty;
	private string _gold = String.Empty;
	private string _spellChance = String.Empty;
	private string _targetChangeInterval = String.Empty;
	private string _weaknessDamage = String.Empty;
	private string _codeBefore = String.Empty;
	private string _otherInitializers = String.Empty;
	private string _otherStatements = String.Empty;

	public string Variable { get => _variable; set => Set(ref _variable, value); }
	public string CreatureType { get => _creatureType; set => Set(ref _creatureType, value); }

	/// <summary>The text between the parentheses of <c>new CreatureType(...)</c>, for example <c>creatureDTO, true</c>.</summary>
	public string ConstructorArguments { get => _constructorArguments; set => Set(ref _constructorArguments, value); }

	public string Name { get => _name; set => Set(ref _name, value); }
	public string Alignment { get => _alignment; set => Set(ref _alignment, value); }
	public string MaxHealth { get => _maxHealth; set => Set(ref _maxHealth, value); }
	public string MaxMana { get => _maxMana; set => Set(ref _maxMana, value); }
	public string BaseDodge { get => _baseDodge; set => Set(ref _baseDodge, value); }
	public string Experience { get => _experience; set => Set(ref _experience, value); }
	public string Movement { get => _movement; set => Set(ref _movement, value); }
	public string HideDetection { get => _hideDetection; set => Set(ref _hideDetection, value); }
	public string VisibilityDistance { get => _visibilityDistance; set => Set(ref _visibilityDistance, value); }
	public string VisionRange { get => _visionRange; set => Set(ref _visionRange, value); }
	public string RangePerception { get => _rangePerception; set => Set(ref _rangePerception, value); }
	public string Level { get => _level; set => Set(ref _level, value); }
	public string NearbySound { get => _nearbySound; set => Set(ref _nearbySound, value); }
	public string AttackSound { get => _attackSound; set => Set(ref _attackSound, value); }
	public string DeathSound { get => _deathSound; set => Set(ref _deathSound, value); }
	public string WarmSound { get => _warmSound; set => Set(ref _warmSound, value); }
	public string Body { get => _body; set => Set(ref _body, value); }
	public string HueMode { get => _hueMode; set => Set(ref _hueMode, value); }
	public string HueName { get => _hueName; set => Set(ref _hueName, value); }
	public byte HueA { get => _hueA; set => Set(ref _hueA, value); }
	public byte HueR { get => _hueR; set => Set(ref _hueR, value); }
	public byte HueG { get => _hueG; set => Set(ref _hueG, value); }
	public byte HueB { get => _hueB; set => Set(ref _hueB, value); }
	public string Penetration { get => _penetration; set => Set(ref _penetration, value); }
	public string SearchStrategy { get => _searchStrategy; set => Set(ref _searchStrategy, value); }
	public string Weapon { get => _weapon; set => Set(ref _weapon, value); }
	public string Shield { get => _shield; set => Set(ref _shield, value); }
	public string Armor { get => _armor; set => Set(ref _armor, value); }
	public string Helmet { get => _helmet; set => Set(ref _helmet, value); }
	public string Gauntlets { get => _gauntlets; set => Set(ref _gauntlets, value); }
	public string Boots { get => _boots; set => Set(ref _boots, value); }
	public string Brain { get => _brain; set => Set(ref _brain, value); }
	public string Gold { get => _gold; set => Set(ref _gold, value); }

	private const string WeaknessDamageStat = "MagicDamageTakenIncrease";

	/// <summary>
	/// Extra magic damage the creature takes, in percent: <c>Stats[EntityStat.MagicDamageTakenIncrease].Add(value,
	/// ModifierType.Constant)</c>. It applies to all magic damage, not only its weaknesses. Empty writes nothing.
	/// </summary>
	public string WeaknessDamage { get => _weaknessDamage; set => Set(ref _weaknessDamage, value); }

	/// <summary><c>CombatantChangeInterval</c>, for example <c>TimeSpan.FromSeconds(5)</c>.</summary>
	public string TargetChangeInterval { get => _targetChangeInterval; set => Set(ref _targetChangeInterval, value); }

	/// <summary>The arguments of <c>new CreatureSpellCollection(...)</c>, for example <c>25, 75</c>.</summary>
	public string SpellChance { get => _spellChance; set => Set(ref _spellChance, value); }

	/// <summary>Statements before the creature is created, other than the CreatureDTO values (which fill the form).</summary>
	public string CodeBefore { get => _codeBefore; set => Set(ref _codeBefore, value); }

	/// <summary>Initializer lines the form has no field for, one per line, written inside the creature's braces.</summary>
	public string OtherInitializers { get => _otherInitializers; set => Set(ref _otherInitializers, value); }

	/// <summary>Statements the form has no field for, written after everything else and before the return.</summary>
	public string OtherStatements { get => _otherStatements; set => Set(ref _otherStatements, value); }

	/// <summary>CanFlee, CanSwim and the rest: null leaves the class default.</summary>
	public ObservableCollection<FlagOption> Flags { get; } = new(FlagNames.Select(n => new FlagOption(n)));
	public ObservableCollection<FlagOption> Immunities { get; } = new();
	public ObservableCollection<FlagOption> Weaknesses { get; } = new();

	public ObservableCollection<AttackRow> Attacks { get; } = new();
	public ObservableCollection<BlockRow> Blocks { get; } = new();
	public ObservableCollection<SpellRow> Spells { get; } = new();
	public ObservableCollection<LootRow> Loot { get; } = new();

	/// <summary><c>AddStatus(new NightVisionStatus(creature))</c> and the like, by status class.</summary>
	public ObservableCollection<NameRow> Statuses { get; } = new();

	/// <summary><c>AddImmunity(typeof(WhirlwindSpell))</c>: spells or items the creature ignores.</summary>
	public ObservableCollection<NameRow> SpellImmunities { get; } = new();

	/// <summary><c>AddWeakness(typeof(XmasCane))</c>.</summary>
	public ObservableCollection<NameRow> SpellWeaknesses { get; } = new();

	/// <summary><c>Stats[EntityStat.FireProtection].Base = 100</c> and the like.</summary>
	public ObservableCollection<StatRow> Stats { get; } = new();

	// Checked immunities and weaknesses are written in the order the script had them, new ones after.
	private readonly List<string> _immunityOrder = new();
	private readonly List<string> _weaknessOrder = new();

	private static IEnumerable<FlagOption> InReadOrder(IEnumerable<FlagOption> options, List<string> order)
		=> options.Where(f => f.IsChecked == true)
			.Select((f, i) => (Option: f, Rank: order.IndexOf(f.Name) is var at and >= 0 ? at : order.Count + i))
			.OrderBy(o => o.Rank)
			.Select(o => o.Option);

	// "new ()" when the creature's values are written inside "Mv = new () { ... }", and the creature's other lines.
	private string? _mvCreation;
	private string _outerInitializers = String.Empty;

	// The target strategy is written "(attacker) => new Strategy()", inside the creature's braces when it was read there.
	private string _strategyParameter = "attacker";
	private bool _strategyInInitializer;

	// The script was "return new Type(...);" with no variable.
	private bool _returnsCreation;

	// Written as "MaxHealth = x, Health = x" unless the script set MaxHealth alone.
	private PoolStyle _healthStyle = PoolStyle.Both;
	private PoolStyle _manaStyle = PoolStyle.Both;

	private enum PoolStyle { Both, Maximum, Current }

	private static string Pool(PoolStyle style, string maximum, string current, string value) => style switch
	{
		PoolStyle.Maximum => $"{maximum} = {value},",
		PoolStyle.Current => $"{current} = {value},",
		_ => $"{maximum} = {value}, {current} = {value},",
	};

	// The CreatureDTO.SetCreatureValues(...) call, when the script uses one. Its values fill the form and are written
	// back into the same call, so they keep the meaning they had there.
	private string? _dtoPrefix;
	private string? _dtoSuffix;
	private string _codeAfterDto = String.Empty;
	private readonly List<(string Name, string Value)> _dtoArguments = new();
	private readonly HashSet<string> _dtoFields = new();

	private static readonly Dictionary<string, string> _dtoArgumentFields = new()
	{
		["name"] = nameof(Name), ["bodyId"] = nameof(Body), ["health"] = nameof(MaxHealth), ["mana"] = nameof(MaxMana),
		["exp"] = nameof(Experience), ["movement"] = nameof(Movement), ["hideDetection"] = nameof(HideDetection),
		["visionRange"] = nameof(VisionRange), ["level"] = nameof(Level), ["nearbySound"] = nameof(NearbySound),
		["attackSound"] = nameof(AttackSound), ["deathSound"] = nameof(DeathSound),
		// the server's parameter really is spelled "visibilty"; it is written back with the name the script used
		["visibilty"] = nameof(VisibilityDistance), ["visibility"] = nameof(VisibilityDistance),
		["alignment"] = nameof(DtoAlignment), ["attack"] = nameof(DtoAttack), ["defense"] = nameof(DtoDefense),
	};

	private string _dtoAlignment = String.Empty;
	private string _dtoAttack = String.Empty;
	private string _dtoDefense = String.Empty;

	/// <summary>The CreatureDTO's alignment number: 1 chaotic, 2 evil, anything else lawful.</summary>
	public string DtoAlignment { get => _dtoAlignment; set => Set(ref _dtoAlignment, value); }

	/// <summary>The CreatureDTO's attack, which its attacks scale from.</summary>
	public string DtoAttack { get => _dtoAttack; set => Set(ref _dtoAttack, value); }

	/// <summary>The CreatureDTO's defense.</summary>
	public string DtoDefense { get => _dtoDefense; set => Set(ref _dtoDefense, value); }

	/// <summary>The script has a CreatureDTO.SetCreatureValues call, so its values are shown in their own section.</summary>
	public bool HasDto => _dtoPrefix != null || IsDtoOnly;

	// An OnSpawn the builder can't rewrite as a whole (one that picks a creature with if or switch) can still have its
	// CreatureDTO values edited: each one is replaced where it stands, and everything else stays exactly as typed.
	private string? _dtoOnlyScript;
	private readonly List<(string Field, int Start, int Length, string Read)> _dtoOnlyValues = new();

	/// <summary>Only the CreatureDTO values are edited; the rest of the form isn't written.</summary>
	public bool IsDtoOnly => _dtoOnlyScript != null;

	/// <summary>OnSpawn is a method body, so it is read as one: the statements of <c>{ script }</c>. Null when it has syntax errors.</summary>
	private static BlockSyntax? ParseBody(string script)
	{
		var wrapped = "{" + Environment.NewLine + script + Environment.NewLine + "}";

		return SyntaxFactory.ParseStatement(wrapped) is BlockSyntax block && block.FullSpan.Length == wrapped.Length
		       && !block.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error)
			? block
			: null;
	}

	public OnSpawnBuilder()
	{
		Watch(Flags);
		Watch(Immunities);
		Watch(Weaknesses);
		Watch(Attacks);
		Watch(Blocks);
		Watch(Spells);
		Watch(Loot);
		Watch(Statuses);
		Watch(SpellImmunities);
		Watch(SpellWeaknesses);
		Watch(Stats);
		Watch(Dialogue);
	}

	/// <summary>Fills the immunity and weakness choices from the server's enums.</summary>
	public void SetEnumChoices(IEnumerable<string> immunities, IEnumerable<string> weaknesses)
	{
		Immunities.Clear();
		Weaknesses.Clear();

		foreach (var name in immunities)
			Immunities.Add(new FlagOption(name) { IsChecked = false });

		foreach (var name in weaknesses)
			Weaknesses.Add(new FlagOption(name) { IsChecked = false });
	}

	private void Set<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
	{
		if (SetProperty(ref field, value, propertyName))
			Changed?.Invoke(this, EventArgs.Empty);
	}

	private void Watch<T>(ObservableCollection<T> collection) where T : INotifyPropertyChanged
	{
		collection.CollectionChanged += (_, args) =>
		{
			if (args.NewItems != null)
			{
				foreach (INotifyPropertyChanged item in args.NewItems)
					item.PropertyChanged += OnRowChanged;
			}

			if (args.OldItems != null)
			{
				foreach (INotifyPropertyChanged item in args.OldItems)
					item.PropertyChanged -= OnRowChanged;
			}

			Changed?.Invoke(this, EventArgs.Empty);
		};

		foreach (var item in collection)
			item.PropertyChanged += OnRowChanged;
	}

	private void OnRowChanged(object? sender, PropertyChangedEventArgs e) => Changed?.Invoke(this, EventArgs.Empty);

	private static bool Has(string? text) => !String.IsNullOrWhiteSpace(text);

	private static string Quote(string text) => SymbolDisplay.FormatLiteral(text, quote: true);

	/// <summary>A message is written as typed when it is already a C# string ($"..." or "..."), otherwise quoted.</summary>
	private static string Message(string text)
	{
		var trimmed = text.Trim();
		return trimmed.StartsWith("$\"") || trimmed.StartsWith("@\"") || trimmed.StartsWith("$@\"") ? trimmed : Quote(text);
	}

	/// <summary>A cooldown in seconds is written as TimeSpan.FromSeconds, anything else as typed.</summary>
	private static string Cooldown(string text)
		=> Double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out _) ? $"TimeSpan.FromSeconds({text.Trim()})" : text.Trim();

	private (string Field, string Value)[] ValueFields() =>
	[
		(nameof(Name), Has(Name) ? Quote(Name) : String.Empty),
		(nameof(MaxHealth), MaxHealth), (nameof(MaxMana), MaxMana), (nameof(BaseDodge), BaseDodge),
		(nameof(Experience), Experience), (nameof(Body), Body), (nameof(Movement), Movement),
		(nameof(HideDetection), HideDetection), (nameof(VisibilityDistance), VisibilityDistance),
		(nameof(VisionRange), VisionRange), (nameof(RangePerception), RangePerception), (nameof(Level), Level),
		(nameof(NearbySound), NearbySound), (nameof(AttackSound), AttackSound), (nameof(DeathSound), DeathSound),
		(nameof(WarmSound), WarmSound), (nameof(DtoAlignment), DtoAlignment), (nameof(DtoAttack), DtoAttack),
		(nameof(DtoDefense), DtoDefense),
	];

	/// <summary>Writes the form as a complete OnSpawn body.</summary>
	public string Generate()
	{
		if (_dtoOnlyScript != null)
			return GenerateDtoOnly(_dtoOnlyScript);

		var v = Has(Variable) ? Variable.Trim() : "creature";
		var lines = new List<string>();
		var init = new List<string>();
		var values = ValueFields().ToDictionary(f => f.Field, f => f.Value.Trim());

		void Chunk(string text)
		{
			if (Has(text))
				lines.AddRange(text.TrimEnd().Replace("\r\n", "\n").Split('\n'));
		}

		Chunk(CodeBefore);

		if (_dtoPrefix != null)
		{
			var arguments = new List<string>();

			foreach (var (name, value) in _dtoArguments)
			{
				var written = _dtoArgumentFields.TryGetValue(name, out var field) && _dtoFields.Contains(field) ? values[field] : value;

				if (Has(written))
					arguments.Add($"\t{name}: {written}");
			}

			Chunk(_dtoPrefix);
			lines.AddRange(arguments.Select((a, i) => i < arguments.Count - 1 ? a + "," : a));
			Chunk(_dtoSuffix ?? ");");

			Chunk(_codeAfterDto);
			lines.Add(String.Empty);
		}

		string? Value(string field) => !_dtoFields.Contains(field) && Has(values[field]) ? values[field] : null;

		if (Value(nameof(Name)) is string quoted) init.Add($"Name = {quoted},");

		if (Value(nameof(MaxHealth)) is string health)
			init.Add(Pool(_healthStyle, "MaxHealth", "Health", health));

		if (Value(nameof(MaxMana)) is string mana)
			init.Add(Pool(_manaStyle, "MaxMana", "Mana", mana));

		foreach (var field in new[] { nameof(BaseDodge), nameof(Experience), nameof(Body) })
		{
			if (Value(field) is string value)
				init.Add($"{field} = {value},");
		}

		if (HueMode == "Named" && Has(HueName))
			init.Add($"Hue = Color.{HueName.Trim()},");
		else if (HueMode == "Custom")
			init.Add($"Hue = Color.FromArgb({HueA}, {HueR}, {HueG}, {HueB}),");

		if (Has(Penetration))
			init.Add($"BasePenetration = ShieldPenetration.{Penetration.Trim()},");

		if (Has(Alignment))
			init.Add($"Alignment = Alignment.{Alignment.Trim()},");

		if (Has(SearchStrategy) && _strategyInInitializer)
			init.Add($"CombatantSearchStrategy = {StrategyExpression()},");

		foreach (var field in new[] { nameof(Movement), nameof(HideDetection), nameof(VisibilityDistance), nameof(VisionRange),
			         nameof(RangePerception), nameof(Level), nameof(NearbySound), nameof(AttackSound), nameof(DeathSound), nameof(WarmSound) })
		{
			if (Value(field) is string value)
				init.Add($"{field} = {value},");
		}

		var immunities = InReadOrder(Immunities, _immunityOrder).Select(f => "CreatureImmunity." + f.Name).ToList();
		if (immunities.Count > 0)
			init.Add($"Immunity = {String.Join(" | ", immunities)},");

		var weaknesses = InReadOrder(Weaknesses, _weaknessOrder).Select(f => "CreatureWeakness." + f.Name).ToList();
		if (weaknesses.Count > 0)
			init.Add($"Weakness = {String.Join(" | ", weaknesses)},");

		foreach (var flag in Flags.Where(f => f.IsChecked.HasValue))
			init.Add($"{flag.Name} = {(flag.IsChecked == true ? "true" : "false")},");

		if (Has(OtherInitializers))
		{
			var other = OtherInitializers.Trim().Replace("\r\n", "\n");
			init.Add(other.EndsWith(",") ? other : other + ",");
		}

		var creation = $"new {(Has(CreatureType) ? CreatureType.Trim() : "CreatureEntity")}({ConstructorArguments.Trim()})";
		var onlyCreation = lines.Count == 0 && init.Count == 0 && _mvCreation == null;

		var properties = init.SelectMany(i => i.Split('\n')).Select(i => "\t" + i.TrimEnd('\r')).ToList();

		if (_mvCreation != null)
		{
			properties = [$"\tMv = {_mvCreation}", "\t{", ..properties.Select(p => "\t" + p), "\t},"];

			if (Has(_outerInitializers))
				properties.AddRange(_outerInitializers.Replace("\r\n", "\n").Split('\n').Select(p => "\t" + p));
		}

		lines.Add($"var {v} = {creation}");
		lines.Add("{");
		lines.AddRange(properties);
		lines.Add("};");
		lines.Add(String.Empty);

		var creatureEnd = lines.Count;
		var setup = new List<string>();

		setup.AddRange(Statuses.Where(s => Has(s.Name)).Select(s => $"{v}.AddStatus(new {s.Name.Trim()}({v}));"));
		setup.AddRange(SpellImmunities.Where(s => Has(s.Name)).Select(s => $"{v}.AddImmunity(typeof({s.Name.Trim()}));"));
		setup.AddRange(SpellWeaknesses.Where(s => Has(s.Name)).Select(s => $"{v}.AddWeakness(typeof({s.Name.Trim()}));"));
		setup.AddRange(Stats.Where(s => Has(s.Stat) && Has(s.Value)).Select(s => Has(s.Modifier) && s.Modifier.Trim() != StatRow.Base
			? $"{v}.Stats[EntityStat.{s.Stat.Trim()}].Add({s.Value.Trim()}, ModifierType.{s.Modifier.Trim()});"
			: $"{v}.Stats[EntityStat.{s.Stat.Trim()}].Base = {s.Value.Trim()};"));

		if (Has(WeaknessDamage))
			setup.Add($"{v}.Stats[EntityStat.{WeaknessDamageStat}].Add({WeaknessDamage.Trim()}, ModifierType.Constant);");

		if (Has(TargetChangeInterval))
			setup.Add($"{v}.CombatantChangeInterval = {TargetChangeInterval.Trim()};");

		if (setup.Count > 0)
		{
			lines.AddRange(setup);
			lines.Add(String.Empty);
		}

		if (Has(SearchStrategy) && !_strategyInInitializer)
		{
			lines.Add($"{v}.CombatantSearchStrategy = {StrategyExpression()};");
			lines.Add(String.Empty);
		}

		if (Attacks.Count > 0)
		{
			lines.Add($"{v}.Attacks = new CreatureAttackCollection()");
			lines.Add("{");

			foreach (var attack in Attacks)
			{
				var arguments = new List<string> { Has(attack.Skill) ? attack.Skill.Trim() : "0" };

				if (Has(attack.MinDamage) && Has(attack.MaxDamage))
					arguments.AddRange([attack.MinDamage.Trim(), attack.MaxDamage.Trim()]);
				else if (attack.Type == "CreatureAttack")
					arguments.AddRange(["1", "1"]);

				if (Has(attack.Message))
					arguments.Add(Message(attack.Message));

				if (Has(attack.Effect))
				{
					var parameters = new[] { attack.EffectValue1, attack.EffectValue2 }.Where(Has).Select(p => p.Trim());
					arguments.Add($"new {attack.Effect.Trim()}({String.Join(", ", parameters)})");
				}

				if (Has(attack.MoreEffects))
					arguments.Add(attack.MoreEffects.Trim().TrimEnd(','));

				var expression = $"new {(Has(attack.Type) ? attack.Type.Trim() : "CreatureBasicAttack")}({String.Join(", ", arguments)})";

				// a weight, and optionally a cooldown, puts the attack in braces: { attack, weight, cooldown }
				if (Has(attack.Weight) || Has(attack.Cooldown))
				{
					var weight = Has(attack.Weight) ? attack.Weight.Trim() : "100";
					var cooldown = Has(attack.Cooldown) ? ", " + Cooldown(attack.Cooldown) : String.Empty;

					lines.Add($"\t{{ {expression}, {weight}{cooldown} }},");
				}
				else
				{
					lines.Add($"\t{expression},");
				}
			}

			lines.Add("};");
			lines.Add(String.Empty);
		}

		var spells = Spells.Where(s => Has(s.Spell)).ToList();

		if (spells.Count > 0)
		{
			lines.Add($"{v}.Spells = new CreatureSpellCollection({SpellChance.Trim()})");
			lines.Add("{");

			foreach (var spell in spells)
			{
				var entry = $"new CreatureSpell<{spell.Spell.Trim()}>({(Has(spell.Skill) ? spell.Skill.Trim() : "10")})";

				// no weight and no cooldown writes the spell on its own, as some scripts have it
				if (!Has(spell.Weight) && !Has(spell.Cooldown))
				{
					lines.Add($"\t{entry},");
					continue;
				}

				entry += $", {(Has(spell.Weight) ? spell.Weight.Trim() : "100")}";

				if (Has(spell.Cooldown))
					entry += ", " + Cooldown(spell.Cooldown);

				lines.Add($"\t{{ {entry} }},");
			}

			lines.Add("};");
			lines.Add(String.Empty);
		}

		var gear = new List<string>();

		foreach (var wielded in new[] { Weapon, Shield }.Where(Has))
			gear.Add($"{v}.Wield(new {wielded.Trim()}());");

		foreach (var equipped in new[] { Armor, Helmet, Gauntlets, Boots }.Where(Has))
			gear.Add($"{v}.Equip(new {equipped.Trim()}());");

		if (Has(Brain))
			gear.Add($"{v}.Brain = new {Brain.Trim()}({v});");

		if (gear.Count > 0)
		{
			lines.AddRange(gear);
			lines.Add(String.Empty);
		}

		var loot = Loot.Where(l => Has(l.Item)).ToList();

		if (Has(Gold))
			lines.Add($"{v}.AddGold({Gold.Trim()});");

		// each pack number is its own AddLoot, in the order the packs first appear
		foreach (var pack in loot.GroupBy(l => Has(l.Pack) ? l.Pack.Trim() : "1"))
		{
			var entries = pack.ToList();

			lines.Add($"{v}.AddLoot(new LootPack(");

			for (var i = 0; i < entries.Count; i++)
			{
				var entry = entries[i];
				var mutator = Has(entry.Mutator) ? ", " + entry.Mutator.Trim() : String.Empty;

				lines.Add($"  new LootPackEntry({(entry.AtSpawn ? "true" : "false")}, {entry.Item.Trim()}, {(Has(entry.Chance) ? entry.Chance.Trim() : "1")}{mutator}){(i < entries.Count - 1 ? "," : String.Empty)}");
			}

			lines.Add("));");
		}

		if (Has(Gold) || loot.Count > 0)
			lines.Add(String.Empty);

		if (Blocks.Count > 0)
		{
			lines.Add($"{v}.Blocks = new CreatureBlockCollection");
			lines.Add("{");

			foreach (var block in Blocks)
				lines.Add($"\tnew CreatureBlock({(Has(block.Roll) ? block.Roll.Trim() : "1")}, {Message(block.Message ?? String.Empty)}),");

			lines.Add("};");
			lines.Add(String.Empty);
		}

		if (DialogueAllowed)
		{
			lines.AddRange(GenerateDialogue(v));
		}
		else if (_dialogueAsRead != null)
		{
			// not a humanoid: options the script already had are written back untouched, and none are added
			Chunk(_dialogueAsRead);
			lines.Add(String.Empty);
		}

		if (Has(OtherStatements))
		{
			Chunk(OtherStatements);
			lines.Add(String.Empty);
		}

		// a script that was just "return new Type();" stays that way until something is added
		if (_returnsCreation && onlyCreation && lines.Count == creatureEnd)
			return $"\treturn {creation};";

		lines.Add($"return {v};");

		return String.Join(Environment.NewLine, lines.Select(l => l.Length > 0 ? "\t" + l : l));
	}

	/// <summary>
	/// Fills the form from an existing script, on a new builder after <see cref="SetEnumChoices"/>. Code the form has
	/// no field for goes into the "kept" text boxes, so <see cref="Generate"/> writes the same script back.
	/// </summary>
	public OnSpawnParseResult Parse(string script, IReadOnlyCollection<string> shields, IReadOnlyCollection<string> armor,
		IReadOnlyCollection<string> helmets, IReadOnlyCollection<string> gauntlets, IReadOnlyCollection<string> boots)
	{
		var result = new OnSpawnParseResult();
		var body = ParseBody(script);

		if (body is null)
			return result.Fail("OnSpawn has C# syntax errors, so the builder can't read it safely. Fix them in the script first.");

		var statements = body.Statements.ToList();
		var returnIndex = statements.FindLastIndex(s => s is ReturnStatementSyntax);

		// "return new Type(...);" reads as "var creature = new Type(...); return creature;"
		if (returnIndex == statements.Count - 1 && statements[returnIndex] is ReturnStatementSyntax { Expression: ObjectCreationExpressionSyntax returnedCreation }
		    && !statements.Any(s => s.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(i => i.Identifier.Text == "creature")))
		{
			var before = String.Concat(statements.Take(returnIndex).Select(s => s.ToFullString()));
			var expanded = $"{before}{Environment.NewLine}var creature = {returnedCreation};{Environment.NewLine}return creature;";
			var parsed = Parse(expanded, shields, armor, helmets, gauntlets, boots);

			_returnsCreation = parsed.Succeeded;

			// the expanded text starts with the script's own code, so the values read stand at the same places in it
			if (parsed.DtoOnly)
				_dtoOnlyScript = script;

			return parsed;
		}

		if (returnIndex < 0 || statements[returnIndex] is not ReturnStatementSyntax { Expression: IdentifierNameSyntax returned })
		{
			var branches = statements.Any(s => s is IfStatementSyntax or SwitchStatementSyntax);

			if (branches && ReadDtoOnly(script, body, result))
				return result;

			return result.Fail(branches
				? "OnSpawn picks between creatures with if or switch (for example a rare variant), so the builder can't edit it. Edit that one in the script."
				: "OnSpawn doesn't end by returning its creature (return creature;), so the builder can't rewrite it safely.");
		}

		if (returnIndex != statements.Count - 1)
			return result.Fail("OnSpawn has code after its return, so the builder can't rewrite it safely.");

		var variable = returned.Identifier.Text;
		var declarationIndex = statements.FindIndex(s => s is LocalDeclarationStatementSyntax local
			&& local.Declaration.Variables.Count == 1
			&& local.Declaration.Variables[0].Identifier.Text == variable
			&& local.Declaration.Variables[0].Initializer?.Value is ObjectCreationExpressionSyntax);

		if (declarationIndex < 0)
			return result.Fail($"OnSpawn doesn't create '{variable}' with 'var {variable} = new ...()', so the builder can't rewrite it safely.");

		var creation = (ObjectCreationExpressionSyntax)((LocalDeclarationStatementSyntax)statements[declarationIndex]).Declaration.Variables[0].Initializer!.Value;

		if (creation.Initializer is { } initializer && !initializer.IsKind(SyntaxKind.ObjectInitializerExpression))
			return result.Fail("The creature's braces hold a collection, not property values, so the builder can't rewrite it safely.");

		Variable = variable;
		CreatureType = creation.Type.ToString();
		ConstructorArguments = creation.ArgumentList is { } argumentList ? argumentList.Arguments.ToString() : String.Empty;
		result.Filled++;

		ReadCodeBefore(statements.Take(declarationIndex).ToList(), result);
		var properties = creation.Initializer?.Expressions ?? default;

		// Mu creatures keep their values in "Mv = new () { ... }", which then stands in for the creature's braces
		var mv = properties.OfType<AssignmentExpressionSyntax>().Where(a => a.Left is IdentifierNameSyntax { Identifier.Text: "Mv" }).ToList();

		if (mv is [{ Right: BaseObjectCreationExpressionSyntax { Initializer: { } mvInitializer } mvCreation } mvAssignment]
		    && mvInitializer.IsKind(SyntaxKind.ObjectInitializerExpression))
		{
			_mvCreation = mvCreation.WithInitializer(null).ToString().Trim();
			_outerInitializers = String.Join(Environment.NewLine, properties.Where(p => p != mvAssignment).Select(p => p.ToString() + ","));
			result.KeptInitializers += properties.Count - 1;
			properties = mvInitializer.Expressions;
		}

		ReadInitializers(properties, result);

		var other = new List<StatementSyntax>();

		foreach (var statement in statements.Skip(declarationIndex + 1).Take(returnIndex - declarationIndex - 1))
		{
			if (!ReadStatement(statement, variable, shields, armor, helmets, gauntlets, boots, result))
				other.Add(statement);
		}

		OtherStatements = Verbatim(other);
		result.KeptStatements += other.Count;

		return result;
	}

	private void ReadCodeBefore(List<StatementSyntax> statements, OnSpawnParseResult result)
	{
		var dtoIndex = statements.FindIndex(s => FindDtoCall(s) is not null);

		if (dtoIndex < 0)
		{
			CodeBefore = Verbatim(statements);
			result.KeptStatements += statements.Count;
			return;
		}

		var statement = statements[dtoIndex];
		var call = FindDtoCall(statement)!;
		var text = statement.ToString();
		var start = call.ArgumentList.OpenParenToken.Span.End - statement.SpanStart;
		var end = call.ArgumentList.CloseParenToken.SpanStart - statement.SpanStart;

		_dtoPrefix = text.Substring(0, start).TrimEnd();
		_dtoSuffix = text.Substring(end).Trim();

		foreach (var argument in call.ArgumentList.Arguments)
		{
			var name = argument.NameColon!.Name.Identifier.Text;
			var value = argument.Expression.ToString();

			_dtoArguments.Add((name, value));

			if (!_dtoArgumentFields.TryGetValue(name, out var field) || _dtoFields.Contains(field))
				continue;

			if (field == nameof(Name))
			{
				if (argument.Expression is not LiteralExpressionSyntax { RawKind: (int)SyntaxKind.StringLiteralExpression } literal)
					continue;

				value = literal.Token.ValueText;
			}

			SetField(field, value);
			_dtoFields.Add(field);
			result.Filled++;
		}

		CodeBefore = Verbatim(statements.Take(dtoIndex).ToList());
		_codeAfterDto = Verbatim(statements.Skip(dtoIndex + 1).ToList());
		result.KeptStatements += statements.Count - 1;
	}

	/// <summary>
	/// Reads only the CreatureDTO values of a script the builder can't rewrite as a whole. False when it has no
	/// SetCreatureValues call before its creatures are picked.
	/// </summary>
	private bool ReadDtoOnly(string script, BlockSyntax body, OnSpawnParseResult result)
	{
		var call = body.Statements.Select(FindDtoCall).FirstOrDefault(c => c is not null);

		if (call is null)
			return false;

		// the body was parsed as "{" + newline + script + newline + "}"
		var offset = 1 + Environment.NewLine.Length;

		foreach (var argument in call.ArgumentList.Arguments)
		{
			if (!_dtoArgumentFields.TryGetValue(argument.NameColon!.Name.Identifier.Text, out var field)
			    || _dtoOnlyValues.Any(v => v.Field == field))
				continue;

			var read = argument.Expression.ToString();

			if (field == nameof(Name))
			{
				if (argument.Expression is not LiteralExpressionSyntax { RawKind: (int)SyntaxKind.StringLiteralExpression } literal)
					continue;

				read = literal.Token.ValueText;
			}

			_dtoOnlyValues.Add((field, argument.Expression.SpanStart - offset, argument.Expression.Span.Length, read));
			SetField(field, read);
			result.Filled++;
		}

		_dtoOnlyScript = script;
		result.DtoOnly = true;

		return true;
	}

	/// <summary>The script as read, with each CreatureDTO value the form changed replaced where it stands.</summary>
	private string GenerateDtoOnly(string script)
	{
		var text = new StringBuilder(script);

		foreach (var (field, start, length, read) in _dtoOnlyValues.OrderByDescending(v => v.Start))
		{
			// a name is written as typed, spaces included; other values are code
			var value = field == nameof(Name) ? GetField(field) : GetField(field).Trim();

			// a value can't be removed from the call, so an emptied field keeps what the script had
			if (!Has(value) || value == read)
				continue;

			text.Remove(start, length).Insert(start, field == nameof(Name) ? Quote(value) : value);
		}

		return text.ToString();
	}

	private string GetField(string field) => field == nameof(Name) ? Name : ValueFields().First(f => f.Field == field).Value;

	/// <summary>A SetCreatureValues call with only named arguments, which the form can map back by name.</summary>
	private static InvocationExpressionSyntax? FindDtoCall(StatementSyntax statement)
	{
		var value = statement switch
		{
			ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment } => assignment.Right,
			LocalDeclarationStatementSyntax { Declaration.Variables: [{ Initializer: { } initializer }] } => initializer.Value,
			_ => null,
		};

		return value is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "SetCreatureValues" } } call
		       && call.ArgumentList.Arguments.All(a => a.NameColon is not null)
			? call
			: null;
	}

	private void SetField(string field, string value)
	{
		switch (field)
		{
			case nameof(Name): Name = value; break;
			case nameof(MaxHealth): MaxHealth = value; break;
			case nameof(MaxMana): MaxMana = value; break;
			case nameof(BaseDodge): BaseDodge = value; break;
			case nameof(Experience): Experience = value; break;
			case nameof(Body): Body = value; break;
			case nameof(Movement): Movement = value; break;
			case nameof(HideDetection): HideDetection = value; break;
			case nameof(VisibilityDistance): VisibilityDistance = value; break;
			case nameof(VisionRange): VisionRange = value; break;
			case nameof(RangePerception): RangePerception = value; break;
			case nameof(Level): Level = value; break;
			case nameof(NearbySound): NearbySound = value; break;
			case nameof(AttackSound): AttackSound = value; break;
			case nameof(DeathSound): DeathSound = value; break;
			case nameof(WarmSound): WarmSound = value; break;
			case nameof(DtoAlignment): DtoAlignment = value; break;
			case nameof(DtoAttack): DtoAttack = value; break;
			case nameof(DtoDefense): DtoDefense = value; break;
		}
	}

	private static readonly HashSet<string> _plainInitializers =
	[
		nameof(BaseDodge), nameof(Experience), nameof(Body), nameof(Movement), nameof(HideDetection), nameof(VisibilityDistance),
		nameof(VisionRange), nameof(RangePerception), nameof(Level), nameof(NearbySound), nameof(AttackSound),
		nameof(DeathSound), nameof(WarmSound),
	];

	private void ReadInitializers(SeparatedSyntaxList<ExpressionSyntax> expressions, OnSpawnParseResult result)
	{
		var assignments = new Dictionary<string, ExpressionSyntax>();
		var other = new List<ExpressionSyntax>();

		foreach (var expression in expressions)
		{
			if (expression is AssignmentExpressionSyntax { Left: IdentifierNameSyntax left } assignment
			    && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression)
			    && assignments.TryAdd(left.Identifier.Text, assignment.Right))
				continue;

			other.Add(expression);
		}

		bool Take(string property, Func<ExpressionSyntax, bool> read)
		{
			if (!assignments.TryGetValue(property, out var value))
				return false;

			if (!read(value))
				return false;

			assignments.Remove(property);
			result.Filled++;
			return true;
		}

		// a value the CreatureDTO already set is overridden here, so the DTO's copy is no longer the form's value
		bool Override(string field)
		{
			if (_dtoFields.Remove(field))
				result.Filled--;

			return true;
		}

		Take("Name", value => value is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.StringLiteralExpression } literal
		                      && Override(nameof(Name)) && Assign(() => Name = literal.Token.ValueText));

		// "MaxHealth = x, Health = x", "MaxHealth = x" or "Health = x" all fill Max health, and are written back the same way
		PoolStyle ReadPool(string maximum, string current, string field, Action<string> set)
		{
			var style = PoolStyle.Both;

			if (!Take(maximum, value =>
			    {
				    var paired = assignments.GetValueOrDefault(current);

				    if (paired == null)
					    style = PoolStyle.Maximum;
				    else if (paired.ToString() == value.ToString())
					    assignments.Remove(current);
				    else
					    style = PoolStyle.Maximum; // a different current value stays as its own line

				    return Override(field) && Assign(() => set(value.ToString()));
			    }))
			{
				if (Take(current, value => Override(field) && Assign(() => set(value.ToString()))))
					style = PoolStyle.Current;
			}

			return style;
		}

		_healthStyle = ReadPool("MaxHealth", "Health", nameof(MaxHealth), value => MaxHealth = value);
		_manaStyle = ReadPool("MaxMana", "Mana", nameof(MaxMana), value => MaxMana = value);

		foreach (var property in _plainInitializers)
			Take(property, value => Override(property) && Assign(() => SetField(property, value.ToString())));

		Take("BasePenetration", value => Member(value, "ShieldPenetration") is string name && Assign(() => Penetration = name));
		Take("Alignment", value => Member(value, "Alignment") is string name && Assign(() => Alignment = name));
		Take("CombatantSearchStrategy", value => ReadStrategy(value) && Assign(() => _strategyInInitializer = true));

		Take("Hue", value =>
		{
			if (Member(value, "Color") is string name)
				return Assign(() => (HueMode, HueName) = ("Named", name));

			if (value is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "Color" }, Name.Identifier.Text: "FromArgb" } } call)
				return false;

			var bytes = call.ArgumentList.Arguments
				.Select(a => Byte.TryParse(a.Expression.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var b) && a.NameColon is null ? b : (byte?)null)
				.ToList();

			if (bytes.Count is not (3 or 4) || bytes.Any(b => b is null))
				return false;

			HueMode = "Custom";
			(HueA, HueR, HueG, HueB) = bytes.Count == 4
				? (bytes[0]!.Value, bytes[1]!.Value, bytes[2]!.Value, bytes[3]!.Value)
				: ((byte)255, bytes[0]!.Value, bytes[1]!.Value, bytes[2]!.Value);
			return true;
		});

		Take("Immunity", value => ReadEnumFlags(value, "CreatureImmunity", Immunities, _immunityOrder));
		Take("Weakness", value => ReadEnumFlags(value, "CreatureWeakness", Weaknesses, _weaknessOrder));

		foreach (var flag in Flags)
		{
			Take(flag.Name, value =>
			{
				if (value.IsKind(SyntaxKind.TrueLiteralExpression))
					flag.IsChecked = true;
				else if (value.IsKind(SyntaxKind.FalseLiteralExpression))
					flag.IsChecked = false;
				else
					return false;

				return true;
			});
		}

		// whatever is left keeps its place in the script's order
		var kept = expressions
			.Where(e => other.Contains(e) || e is AssignmentExpressionSyntax { Left: IdentifierNameSyntax left } a
				&& assignments.TryGetValue(left.Identifier.Text, out var right) && right == a.Right)
			.Select(e => e.ToString().Replace("\r\n", "\n").Replace("\n", Environment.NewLine) + ",")
			.ToList();

		OtherInitializers = String.Join(Environment.NewLine, kept);
		result.KeptInitializers += kept.Count;
	}

	private static bool Assign(Action set)
	{
		set();
		return true;
	}

	/// <summary>The member name of <c>Type.Member</c>, or null when the value is anything else.</summary>
	private static string? Member(ExpressionSyntax value, string type)
		=> value is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax owner } access && owner.Identifier.Text == type
			? access.Name.Identifier.Text
			: null;

	private static bool ReadEnumFlags(ExpressionSyntax value, string type, ObservableCollection<FlagOption> options, List<string> order)
	{
		var names = new List<string>();

		bool Collect(ExpressionSyntax expression)
		{
			switch (expression)
			{
				case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.BitwiseOrExpression):
					return Collect(binary.Left) && Collect(binary.Right);
				case ParenthesizedExpressionSyntax parenthesized:
					return Collect(parenthesized.Expression);
				default:
					if (Member(expression, type) is not string name || options.All(o => o.Name != name))
						return false;

					names.Add(name);
					return true;
			}
		}

		if (!Collect(value))
			return false;

		foreach (var option in options)
			option.IsChecked = names.Contains(option.Name);

		order.AddRange(names);
		return true;
	}

	/// <summary>Reads <c>(attacker) => new SomeSearchStrategy()</c>, the way scripts set a target strategy.</summary>
	private bool ReadStrategy(ExpressionSyntax value)
	{
		var (parameter, body) = value switch
		{
			ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters: [var single] } lambda => (single.Identifier.Text, lambda.ExpressionBody),
			SimpleLambdaExpressionSyntax lambda => (lambda.Parameter.Identifier.Text, lambda.ExpressionBody),
			_ => (null, null),
		};

		if (parameter is null || body is not ObjectCreationExpressionSyntax { Initializer: null } created
		    || created.ArgumentList is { Arguments.Count: > 0 } || value is LambdaExpressionSyntax { AsyncKeyword.RawKind: not 0 })
			return false;

		_strategyParameter = parameter;
		SearchStrategy = created.Type.ToString();
		return true;
	}

	private string StrategyExpression() => $"({_strategyParameter}) => new {SearchStrategy.Trim()}()";

	private bool ReadStatement(StatementSyntax statement, string v, IReadOnlyCollection<string> shields,
		IReadOnlyCollection<string> armor, IReadOnlyCollection<string> helmets, IReadOnlyCollection<string> gauntlets,
		IReadOnlyCollection<string> boots, OnSpawnParseResult result)
	{
		if (statement is not ExpressionStatementSyntax { Expression: var expression })
			return false;

		bool Done()
		{
			result.Filled++;
			return true;
		}

		// creature.Stats[EntityStat.Name].Base = value;
		if (expression is AssignmentExpressionSyntax
		    {
			    Left: MemberAccessExpressionSyntax
			    {
				    Name.Identifier.Text: "Base",
				    Expression: ElementAccessExpressionSyntax
				    {
					    Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax statOwner, Name.Identifier.Text: "Stats" },
					    ArgumentList.Arguments: [{ NameColon: null } statArgument],
				    },
			    },
		    } statAssignment
		    && statAssignment.IsKind(SyntaxKind.SimpleAssignmentExpression) && statOwner.Identifier.Text == v
		    && Member(statArgument.Expression, "EntityStat") is string stat)
		{
			Stats.Add(new StatRow { Stat = stat, Value = statAssignment.Right.ToString() });
			return Done();
		}

		// creature.Stats[EntityStat.Name].Add(value, ModifierType.Kind);
		if (expression is InvocationExpressionSyntax
		    {
			    Expression: MemberAccessExpressionSyntax
			    {
				    Name.Identifier.Text: "Add",
				    Expression: ElementAccessExpressionSyntax
				    {
					    Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax modifierOwner, Name.Identifier.Text: "Stats" },
					    ArgumentList.Arguments: [{ NameColon: null } modifiedStat],
				    },
			    },
			    ArgumentList.Arguments: [{ NameColon: null } amount, { NameColon: null } kind],
		    }
		    && modifierOwner.Identifier.Text == v
		    && Member(modifiedStat.Expression, "EntityStat") is string modified
		    && Member(kind.Expression, "ModifierType") is string modifier && StatRow.Modifiers.Contains(modifier))
		{
			// the first constant magic damage increase is what the Weakness damage box shows
			if (modified == WeaknessDamageStat && modifier == "Constant" && !Has(WeaknessDamage))
			{
				WeaknessDamage = amount.Expression.ToString();
				return Done();
			}

			Stats.Add(new StatRow { Stat = modified, Value = amount.Expression.ToString(), Modifier = modifier });
			return Done();
		}

		// creature.Gossips = new List<GossipInteraction>() { ... };
		if (expression is AssignmentExpressionSyntax { Left: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax gossipOwner, Name.Identifier.Text: "Gossips" } } gossips
		    && gossips.IsKind(SyntaxKind.SimpleAssignmentExpression) && gossipOwner.Identifier.Text == v && ReadDialogue(gossips.Right))
		{
			_dialogueAsRead = Verbatim([statement]);
			return Done();
		}

		// creature.CombatantSearchStrategy = (attacker) => new RandomSearchStrategy();
		if (expression is AssignmentExpressionSyntax { Left: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax strategyOwner, Name.Identifier.Text: "CombatantSearchStrategy" } } strategy
		    && strategy.IsKind(SyntaxKind.SimpleAssignmentExpression) && strategyOwner.Identifier.Text == v && !Has(SearchStrategy)
		    && ReadStrategy(strategy.Right))
			return Done();

		// creature.CombatantChangeInterval = value;
		if (expression is AssignmentExpressionSyntax { Left: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax intervalOwner, Name.Identifier.Text: "CombatantChangeInterval" } } interval
		    && interval.IsKind(SyntaxKind.SimpleAssignmentExpression) && intervalOwner.Identifier.Text == v && !Has(TargetChangeInterval))
		{
			TargetChangeInterval = interval.Right.ToString();
			return Done();
		}

		// creature.Property = new Type(...) { ... };
		if (expression is AssignmentExpressionSyntax { Left: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax target } property } assignment
		    && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) && target.Identifier.Text == v
		    && assignment.Right is ObjectCreationExpressionSyntax created)
		{
			var arguments = created.ArgumentList?.Arguments.ToString() ?? String.Empty;
			var type = created.Type.ToString();

			switch (property.Name.Identifier.Text)
			{
				case "Brain" when created.Initializer is null && arguments == v && !Has(Brain):
					Brain = type;
					return Done();

				case "Attacks" when type == "CreatureAttackCollection" && arguments.Length == 0 && Attacks.Count == 0:
					return ReadAttacks(created.Initializer) && Done();

				case "Spells" when type == "CreatureSpellCollection" && Spells.Count == 0:
					if (!ReadSpells(created.Initializer))
						return false;

					SpellChance = arguments;
					return Done();

				case "Blocks" when type == "CreatureBlockCollection" && arguments.Length == 0 && Blocks.Count == 0:
					return ReadBlocks(created.Initializer) && Done();
			}

			return false;
		}

		// creature.Method(argument);
		if (expression is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax owner } method } call
		    || owner.Identifier.Text != v || call.ArgumentList.Arguments is not [{ NameColon: null, RefKindKeyword.RawKind: 0 } argument])
			return false;

		// new Type() with nothing else, for gear
		var gearType = argument.Expression is ObjectCreationExpressionSyntax { Initializer: null } item
		               && (item.ArgumentList is null || item.ArgumentList.Arguments.Count == 0)
			? item.Type.ToString()
			: null;

		switch (method.Name.Identifier.Text)
		{
			case "Wield" when gearType != null:
				if (shields.Contains(gearType) && !Has(Shield)) { Shield = gearType; return Done(); }
				if (!shields.Contains(gearType) && !Has(Weapon)) { Weapon = gearType; return Done(); }
				return false;

			case "Equip" when gearType != null:
				if (armor.Contains(gearType) && !Has(Armor)) { Armor = gearType; return Done(); }
				if (helmets.Contains(gearType) && !Has(Helmet)) { Helmet = gearType; return Done(); }
				if (gauntlets.Contains(gearType) && !Has(Gauntlets)) { Gauntlets = gearType; return Done(); }
				if (boots.Contains(gearType) && !Has(Boots)) { Boots = gearType; return Done(); }
				return false;

			case "AddGold" when !Has(Gold):
				Gold = argument.Expression.ToString();
				return Done();

			case "AddStatus" when argument.Expression is ObjectCreationExpressionSyntax { Initializer: null, ArgumentList.Arguments: [{ NameColon: null, Expression: IdentifierNameSyntax statusOwner }] } status
			                      && statusOwner.Identifier.Text == v:
				Statuses.Add(new NameRow { Name = status.Type.ToString() });
				return Done();

			case "AddImmunity" when argument.Expression is TypeOfExpressionSyntax immunity:
				SpellImmunities.Add(new NameRow { Name = immunity.Type.ToString() });
				return Done();

			case "AddWeakness" when argument.Expression is TypeOfExpressionSyntax weakness:
				SpellWeaknesses.Add(new NameRow { Name = weakness.Type.ToString() });
				return Done();

			case "AddLoot":
				return ReadLoot(argument.Expression, (Loot.Select(l => l.Pack).Distinct().Count() + 1).ToString()) && Done();
		}

		return false;
	}

	/// <summary>The entries of a collection initializer, each either a value or a <c>{ value, extra... }</c> group.</summary>
	private static List<(ExpressionSyntax Value, List<ExpressionSyntax> Extra)>? Entries(InitializerExpressionSyntax? initializer)
	{
		if (initializer is null || !initializer.IsKind(SyntaxKind.CollectionInitializerExpression) || initializer.Expressions.Count == 0)
			return null;

		return initializer.Expressions
			.Select(e => e is InitializerExpressionSyntax group && group.IsKind(SyntaxKind.ComplexElementInitializerExpression)
				? (group.Expressions.First(), group.Expressions.Skip(1).ToList())
				: (e, new List<ExpressionSyntax>()))
			.ToList();
	}

	private static bool IsString(ExpressionSyntax expression)
		=> expression is InterpolatedStringExpressionSyntax || expression.IsKind(SyntaxKind.StringLiteralExpression);

	/// <summary>A plain "text" reads as its text; an interpolated or verbatim string stays as typed.</summary>
	private static string MessageText(ExpressionSyntax expression)
		=> expression is LiteralExpressionSyntax literal && !literal.Token.Text.StartsWith("@") ? literal.Token.ValueText : expression.ToString();

	private bool ReadAttacks(InitializerExpressionSyntax? initializer)
	{
		var entries = Entries(initializer);

		if (entries is null)
			return false;

		if (entries.Any(e => e.Extra.Count > 2))
			return false;

		var rows = new List<AttackRow>();

		foreach (var (value, extra) in entries)
		{
			if (value is not ObjectCreationExpressionSyntax { Initializer: null, ArgumentList: { } argumentList } attack
			    || !AttackTypes.Contains(attack.Type.ToString())
			    || argumentList.Arguments.Any(a => a.NameColon != null))
				return false;

			var row = new AttackRow
			{
				Type = attack.Type.ToString(),
				Weight = extra.FirstOrDefault()?.ToString() ?? String.Empty,
				Cooldown = Seconds(extra.ElementAtOrDefault(1)),
			};
			var numbers = new List<string>();
			var stage = 0; // 0 numbers, 1 message, 2 effect, 3 more effects

			foreach (var argument in argumentList.Arguments.Select(a => a.Expression))
			{
				if (IsString(argument) && stage < 1)
				{
					row.Message = MessageText(argument);
					stage = 1;
				}
				else if (argument is ObjectCreationExpressionSyntax { Initializer: null } effect && stage < 2
				         && (effect.ArgumentList?.Arguments.Count ?? 0) <= 2 && effect.ArgumentList?.Arguments.All(a => a.NameColon is null) != false)
				{
					var parameters = effect.ArgumentList?.Arguments.Select(a => a.Expression.ToString()).ToList() ?? [];

					row.Effect = effect.Type.ToString();
					row.EffectValue1 = parameters.ElementAtOrDefault(0) ?? String.Empty;
					row.EffectValue2 = parameters.ElementAtOrDefault(1) ?? String.Empty;
					stage = 2;
				}
				else if (argument is ObjectCreationExpressionSyntax && numbers.Count > 0)
				{
					// a second effect, or one with more values than the Effect columns hold, is kept as typed
					row.MoreEffects = Has(row.MoreEffects) ? $"{row.MoreEffects}, {argument}" : argument.ToString();
					stage = 3;
				}
				else if (stage == 0 && !IsString(argument))
				{
					numbers.Add(argument.ToString());
				}
				else
				{
					return false;
				}
			}

			switch (numbers.Count)
			{
				case 1 when row.Type == "CreatureBasicAttack":
					row.Skill = numbers[0];
					break;
				case 3:
					(row.Skill, row.MinDamage, row.MaxDamage) = (numbers[0], numbers[1], numbers[2]);
					break;
				default:
					return false;
			}

			rows.Add(row);
		}

		foreach (var row in rows)
			Attacks.Add(row);

		return true;
	}

	/// <summary>"TimeSpan.FromSeconds(n)" reads as n; any other cooldown stays as typed, and none reads as empty.</summary>
	private static string Seconds(ExpressionSyntax? cooldown)
		=> cooldown is InvocationExpressionSyntax
		{
			Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "TimeSpan" }, Name.Identifier.Text: "FromSeconds" },
			ArgumentList.Arguments: [{ NameColon: null, Expression: LiteralExpressionSyntax number }]
		} && number.IsKind(SyntaxKind.NumericLiteralExpression)
			? number.Token.Text
			: cooldown?.ToString() ?? String.Empty;

	private bool ReadSpells(InitializerExpressionSyntax? initializer)
	{
		var entries = Entries(initializer);

		if (entries is null)
			return false;

		var rows = new List<SpellRow>();

		foreach (var (value, extra) in entries)
		{
			if (value is not ObjectCreationExpressionSyntax { Initializer: null, ArgumentList: { } argumentList, Type: GenericNameSyntax { Identifier.Text: "CreatureSpell", TypeArgumentList.Arguments: [var spell] } }
			    || extra.Count > 2)
				return false;

			rows.Add(new SpellRow
			{
				Spell = spell.ToString(),
				Skill = argumentList.Arguments.ToString(),
				Weight = extra.FirstOrDefault()?.ToString() ?? String.Empty,
				Cooldown = Seconds(extra.ElementAtOrDefault(1)),
			});
		}

		foreach (var row in rows)
			Spells.Add(row);

		return true;
	}

	private bool ReadBlocks(InitializerExpressionSyntax? initializer)
	{
		var entries = Entries(initializer);

		if (entries is null || entries.Any(e => e.Extra.Count > 0))
			return false;

		var rows = new List<BlockRow>();

		foreach (var (value, _) in entries)
		{
			if (value is not ObjectCreationExpressionSyntax { Type: IdentifierNameSyntax { Identifier.Text: "CreatureBlock" }, Initializer: null, ArgumentList.Arguments: [{ NameColon: null } roll, { NameColon: null } message] }
			    || !IsString(message.Expression))
				return false;

			rows.Add(new BlockRow { Roll = roll.Expression.ToString(), Message = MessageText(message.Expression) });
		}

		foreach (var row in rows)
			Blocks.Add(row);

		return true;
	}

	private bool ReadLoot(ExpressionSyntax expression, string pack)
	{
		if (expression is not ObjectCreationExpressionSyntax { Type: IdentifierNameSyntax { Identifier.Text: "LootPack" }, Initializer: null, ArgumentList: { Arguments.Count: > 0 } lootPack })
			return false;

		var rows = new List<LootRow>();

		foreach (var argument in lootPack.Arguments)
		{
			if (argument.NameColon != null
			    || argument.Expression is not ObjectCreationExpressionSyntax { Type: IdentifierNameSyntax { Identifier.Text: "LootPackEntry" }, Initializer: null, ArgumentList.Arguments: { Count: 3 or 4 } entry }
			    || entry.Any(a => a.NameColon != null)
			    || !(entry[0].Expression.IsKind(SyntaxKind.TrueLiteralExpression) || entry[0].Expression.IsKind(SyntaxKind.FalseLiteralExpression)))
				return false;

			rows.Add(new LootRow
			{
				Pack = pack,
				AtSpawn = entry[0].Expression.IsKind(SyntaxKind.TrueLiteralExpression),
				Item = entry[1].Expression.ToString(),
				Chance = entry[2].Expression.ToString(),
				Mutator = entry.Count == 4 ? entry[3].Expression.ToString() : String.Empty,
			});
		}

		foreach (var row in rows)
			Loot.Add(row);

		return true;
	}

	/// <summary>Statements as they were typed, with their comments, re-indented to line up with the generated code.</summary>
	private static string Verbatim(IReadOnlyCollection<StatementSyntax> statements)
	{
		if (statements.Count == 0)
			return String.Empty;

		var lines = String.Concat(statements.Select(s => s.ToFullString())).Replace("\r\n", "\n").Split('\n').Select(l => l.TrimEnd()).ToList();

		while (lines.Count > 0 && lines[0].Length == 0)
			lines.RemoveAt(0);

		while (lines.Count > 0 && lines[^1].Length == 0)
			lines.RemoveAt(lines.Count - 1);

		// scripts mix tabs and spaces, so indents are measured in columns (a tab is 4) and written back as tabs
		static int Columns(string line) => line.TakeWhile(Char.IsWhiteSpace).Sum(c => c == '\t' ? 4 : 1);

		var indent = lines.Where(l => l.Length > 0).Select(Columns).DefaultIfEmpty(0).Min();

		return String.Join(Environment.NewLine, lines.Select(l =>
		{
			if (l.Length == 0)
				return l;

			var columns = Columns(l) - indent;
			return new string('\t', columns / 4) + new string(' ', columns % 4) + l.TrimStart();
		}));
	}

	/// <summary>
	/// Whether two scripts are the same code once whitespace and comments are ignored, with the creature's property
	/// values and the statements after it compared in any order. Used to check that a read and write changes nothing.
	/// </summary>
	public static bool SameCode(string a, string b) => Canonical(a).SequenceEqual(Canonical(b));

	private static List<string> Canonical(string script)
	{
		var body = ParseBody(script);

		if (body is null)
			return [script];

		var root = (BlockSyntax)new CanonicalRewriter().Visit(body)!;
		var shapes = new List<string>();

		static string Tokens(SyntaxNode node) => String.Join(" ", node.DescendantTokens().Select(t => t.Text));

		void AddCreation(string variable, ObjectCreationExpressionSyntax creation)
		{
			var properties = creation.Initializer?.Expressions.Select(Tokens).OrderBy(t => t, StringComparer.Ordinal) ?? Enumerable.Empty<string>();
			shapes.Add($"{variable} = new {Tokens(creation.Type)} {Tokens(creation.ArgumentList!)} {{ {String.Join(" , ", properties)} }}");
		}

		foreach (var statement in root.Statements)
		{
			if (statement is LocalDeclarationStatementSyntax { Declaration.Variables: [{ Initializer.Value: ObjectCreationExpressionSyntax creation } declared] })
			{
				AddCreation(declared.Identifier.Text, creation);
			}
			else if (statement is ReturnStatementSyntax { Expression: ObjectCreationExpressionSyntax returned })
			{
				// the same as "var creature = new ...; return creature;"
				AddCreation("creature", returned);
				shapes.Add("return creature ;");
			}
			else
			{
				shapes.Add(Tokens(statement));
			}
		}

		shapes.Sort(StringComparer.Ordinal);
		return shapes;
	}

	/// <summary>
	/// Writes <c>new T</c> as <c>new T()</c>, <c>{ { x } }</c> as <c>{ x }</c>, and drops trailing commas in braces,
	/// none of which change the code.
	/// </summary>
	private sealed class CanonicalRewriter : CSharpSyntaxRewriter
	{
		public override SyntaxNode? VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
		{
			var visited = (ObjectCreationExpressionSyntax)base.VisitObjectCreationExpression(node)!;
			return visited.ArgumentList is null ? visited.WithArgumentList(SyntaxFactory.ArgumentList()) : visited;
		}

		public override SyntaxNode? VisitInitializerExpression(InitializerExpressionSyntax node)
		{
			var visited = (InitializerExpressionSyntax)base.VisitInitializerExpression(node)!;
			var expressions = visited.Expressions.Select(e => visited.IsKind(SyntaxKind.CollectionInitializerExpression)
			                                                  && e is InitializerExpressionSyntax { Expressions.Count: 1 } single
			                                                  && single.IsKind(SyntaxKind.ComplexElementInitializerExpression)
				? single.Expressions[0]
				: e);

			// property values are compared in any order, as the builder writes them in its own order
			if (visited.IsKind(SyntaxKind.ObjectInitializerExpression))
				expressions = expressions.OrderBy(e => e.ToString(), StringComparer.Ordinal);

			return visited.WithExpressions(SyntaxFactory.SeparatedList(expressions));
		}
	}
}

/// <summary>What <see cref="OnSpawnBuilder.Parse"/> recognized, and whether writing the form back is safe.</summary>
public class OnSpawnParseResult
{
	/// <summary>Form fields and lists that were filled.</summary>
	public int Filled { get; set; }

	/// <summary>Creature property lines kept as typed because the form has no field for them.</summary>
	public int KeptInitializers { get; set; }

	/// <summary>Statements kept as typed because the form has no field for them.</summary>
	public int KeptStatements { get; set; }

	/// <summary>Why the script couldn't be read; null when it was.</summary>
	public string? Problem { get; private set; }

	public bool Succeeded => Problem is null;

	/// <summary>Only the CreatureDTO values were read; the rest of the script is kept exactly as typed.</summary>
	public bool DtoOnly { get; set; }

	public OnSpawnParseResult Fail(string problem)
	{
		Problem = problem;
		return this;
	}
}

public class FlagOption : ObservableObject
{
	private bool? _isChecked;

	public string Name { get; }

	/// <summary>True or false sets the flag; null leaves the class default.</summary>
	public bool? IsChecked { get => _isChecked; set => SetProperty(ref _isChecked, value); }

	public FlagOption(string name)
	{
		Name = name;
	}
}

public class AttackRow : ObservableObject
{
	private string _type = "CreatureBasicAttack";
	private string _skill = "10";
	private string _minDamage = String.Empty;
	private string _maxDamage = String.Empty;
	private string _weight = String.Empty;
	private string _message = String.Empty;
	private string _effect = String.Empty;
	private string _effectValue1 = String.Empty;
	private string _effectValue2 = String.Empty;
	private string _moreEffects = String.Empty;
	private string _cooldown = String.Empty;

	public string Type { get => _type; set => SetProperty(ref _type, value); }
	public string Skill { get => _skill; set => SetProperty(ref _skill, value); }
	public string MinDamage { get => _minDamage; set => SetProperty(ref _minDamage, value); }
	public string MaxDamage { get => _maxDamage; set => SetProperty(ref _maxDamage, value); }
	public string Weight { get => _weight; set => SetProperty(ref _weight, value); }

	/// <summary>Seconds, or a TimeSpan expression kept as typed; empty writes no cooldown.</summary>
	public string Cooldown { get => _cooldown; set => SetProperty(ref _cooldown, value); }

	/// <summary>The attack text; a $"..." interpolated string is kept as typed.</summary>
	public string Message { get => _message; set => SetProperty(ref _message, value); }

	/// <summary>An optional attack component: daze, prone, poison or age.</summary>
	public string Effect { get => _effect; set => SetProperty(ref _effect, value); }
	public string EffectValue1 { get => _effectValue1; set => SetProperty(ref _effectValue1, value); }
	public string EffectValue2 { get => _effectValue2; set => SetProperty(ref _effectValue2, value); }

	/// <summary>Further effects as typed, for example <c>new AttackDazeComponent(5)</c>.</summary>
	public string MoreEffects { get => _moreEffects; set => SetProperty(ref _moreEffects, value); }
}

public class NameRow : ObservableObject
{
	private string _name = String.Empty;

	public string Name { get => _name; set => SetProperty(ref _name, value); }
}

public class StatRow : ObservableObject
{
	/// <summary>Sets the stat's base value: <c>Stats[EntityStat.X].Base = value</c>.</summary>
	public const string Base = "Base";

	/// <summary>Base, or a ModifierType that adds to the stat: <c>Stats[EntityStat.X].Add(value, ModifierType.Kind)</c>.</summary>
	public static readonly string[] Modifiers = [Base, "Constant", "AdditivePercent", "MultiplicativePercent"];

	private string _stat = String.Empty;
	private string _value = String.Empty;
	private string _modifier = Base;

	/// <summary>An EntityStat member, such as FireProtection.</summary>
	public string Stat { get => _stat; set => SetProperty(ref _stat, value); }
	public string Value { get => _value; set => SetProperty(ref _value, value); }
	public string Modifier { get => _modifier; set => SetProperty(ref _modifier, value); }
}

public class BlockRow : ObservableObject
{
	private string _roll = "3";
	private string _message = String.Empty;

	public string Roll { get => _roll; set => SetProperty(ref _roll, value); }
	public string Message { get => _message; set => SetProperty(ref _message, value); }
}

public class SpellRow : ObservableObject
{
	private string _spell = String.Empty;
	private string _skill = "10";
	private string _weight = "100";
	private string _cooldown = "3";

	public string Spell { get => _spell; set => SetProperty(ref _spell, value); }

	/// <summary>The spell's constructor arguments: a skill level, or named ones such as <c>skillLevel: 12, cost: 6</c>.</summary>
	public string Skill { get => _skill; set => SetProperty(ref _skill, value); }
	public string Weight { get => _weight; set => SetProperty(ref _weight, value); }

	/// <summary>Seconds, or a TimeSpan expression kept as typed; empty writes no cooldown.</summary>
	public string Cooldown { get => _cooldown; set => SetProperty(ref _cooldown, value); }
}

public class LootRow : ObservableObject
{
	private string _pack = "1";
	private bool _atSpawn = true;
	private string _item = String.Empty;
	private string _chance = "10";
	private string _mutator = String.Empty;

	/// <summary>Rows with the same pack number go in the same AddLoot(new LootPack(...)).</summary>
	public string Pack { get => _pack; set => SetProperty(ref _pack, value); }
	public bool AtSpawn { get => _atSpawn; set => SetProperty(ref _atSpawn, value); }
	public string Item { get => _item; set => SetProperty(ref _item, value); }
	public string Chance { get => _chance; set => SetProperty(ref _chance, value); }

	/// <summary>An optional price mutator such as gemsPriceMutator.</summary>
	public string Mutator { get => _mutator; set => SetProperty(ref _mutator, value); }
}
