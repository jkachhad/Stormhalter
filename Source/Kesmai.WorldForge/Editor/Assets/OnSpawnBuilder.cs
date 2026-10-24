#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.Editor.Assets;

/// <summary>
/// A form model of a whole OnSpawn script: it can be read from an existing script (<see cref="Parse"/>) and
/// written back as a complete body (<see cref="Generate"/>).
/// </summary>
public class OnSpawnBuilder : ObservableObject
{
	public static readonly string[] FlagNames =
	[
		"CanFlee", "CanPanic", "CanSwim", "CanFly", "CanWalk", "CanLoot", "CanJumpkick", "CanCharge", "CanThrow",
		"CanStrikeCritically", "CanRegenerateHealth", "CanRegenerateMana",
	];

	public static readonly string[] HueModes = ["None", "Named", "Custom"];
	public static readonly string[] AttackTypes = ["CreatureBasicAttack", "CreatureAttack"];

	/// <summary>Raised whenever anything in the form changes, including list rows.</summary>
	public event EventHandler? Changed;

	private string _variable = "creature";
	private string _creatureType = "CreatureEntity";
	private string _name = String.Empty;
	private string _alignment = String.Empty;
	private string _maxHealth = String.Empty;
	private string _baseDodge = String.Empty;
	private string _experience = String.Empty;
	private string _movement = String.Empty;
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

	public string Variable { get => _variable; set => Set(ref _variable, value); }
	public string CreatureType { get => _creatureType; set => Set(ref _creatureType, value); }
	public string Name { get => _name; set => Set(ref _name, value); }
	public string Alignment { get => _alignment; set => Set(ref _alignment, value); }
	public string MaxHealth { get => _maxHealth; set => Set(ref _maxHealth, value); }
	public string BaseDodge { get => _baseDodge; set => Set(ref _baseDodge, value); }
	public string Experience { get => _experience; set => Set(ref _experience, value); }
	public string Movement { get => _movement; set => Set(ref _movement, value); }
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

	/// <summary>CanFlee, CanSwim and the rest: null leaves the class default.</summary>
	public ObservableCollection<FlagOption> Flags { get; } = new(FlagNames.Select(n => new FlagOption(n)));
	public ObservableCollection<FlagOption> Immunities { get; } = new();
	public ObservableCollection<FlagOption> Weaknesses { get; } = new();

	public ObservableCollection<AttackRow> Attacks { get; } = new();
	public ObservableCollection<BlockRow> Blocks { get; } = new();
	public ObservableCollection<SpellRow> Spells { get; } = new();
	public ObservableCollection<LootRow> Loot { get; } = new();

	public OnSpawnBuilder()
	{
		Watch(Flags);
		Watch(Immunities);
		Watch(Weaknesses);
		Watch(Attacks);
		Watch(Blocks);
		Watch(Spells);
		Watch(Loot);
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

	private static int? Number(string? text)
		=> Int32.TryParse(text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : null;

	private static string Quote(string text) => "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

	/// <summary>Writes the form as a complete OnSpawn body.</summary>
	public string Generate()
	{
		var v = String.IsNullOrWhiteSpace(Variable) ? "creature" : Variable.Trim();
		var nl = Environment.NewLine;
		var lines = new List<string>();
		var init = new List<string>();

		if (!String.IsNullOrWhiteSpace(Name))
			init.Add($"Name = {Quote(Name)},");

		if (Number(MaxHealth) is int health)
			init.Add($"MaxHealth = {health}, Health = {health},");

		if (Number(BaseDodge) is int dodge)
			init.Add($"BaseDodge = {dodge},");

		if (Number(Experience) is int experience)
			init.Add($"Experience = {experience},");

		if (Number(Body) is int body)
			init.Add($"Body = {body},");

		if (HueMode == "Named" && !String.IsNullOrWhiteSpace(HueName))
			init.Add($"Hue = Color.{HueName.Trim()},");
		else if (HueMode == "Custom")
			init.Add($"Hue = Color.FromArgb({HueA}, {HueR}, {HueG}, {HueB}),");

		if (!String.IsNullOrWhiteSpace(Penetration))
			init.Add($"BasePenetration = ShieldPenetration.{Penetration.Trim()},");

		if (!String.IsNullOrWhiteSpace(Alignment))
			init.Add($"Alignment = Alignment.{Alignment.Trim()},");

		if (Number(Movement) is int movement)
			init.Add($"Movement = {movement},");

		var immunities = Immunities.Where(f => f.IsChecked == true).Select(f => "CreatureImmunity." + f.Name).ToList();
		if (immunities.Count > 0)
			init.Add($"Immunity = {String.Join(" | ", immunities)},");

		var weaknesses = Weaknesses.Where(f => f.IsChecked == true).Select(f => "CreatureWeakness." + f.Name).ToList();
		if (weaknesses.Count > 0)
			init.Add($"Weakness = {String.Join(" | ", weaknesses)},");

		foreach (var flag in Flags.Where(f => f.IsChecked.HasValue))
			init.Add($"{flag.Name} = {(flag.IsChecked == true ? "true" : "false")},");

		lines.Add($"var {v} = new {(String.IsNullOrWhiteSpace(CreatureType) ? "CreatureEntity" : CreatureType.Trim())}()");
		lines.Add("{");
		lines.AddRange(init.Select(i => "\t" + i));
		lines.Add("};");
		lines.Add(String.Empty);

		if (!String.IsNullOrWhiteSpace(SearchStrategy))
		{
			var argument = SearchStrategy is "ClosestSearchStrategy" or "FarthestSearchStrategy" ? $"{v}.Location" : String.Empty;
			lines.Add($"{v}.CombatantSearchStrategy = new {SearchStrategy.Trim()}({argument});");
			lines.Add(String.Empty);
		}

		if (Attacks.Count > 0)
		{
			var weighted = Attacks.Count > 1 || Attacks.Any(a => Number(a.Weight) != null);

			lines.Add($"{v}.Attacks = new CreatureAttackCollection()");
			lines.Add("{");

			foreach (var attack in Attacks)
			{
				var arguments = new List<string> { (Number(attack.Skill) ?? 0).ToString() };

				if (Number(attack.MinDamage) is int min && Number(attack.MaxDamage) is int max)
					arguments.AddRange([min.ToString(), max.ToString()]);
				else if (attack.Type == "CreatureAttack")
					arguments.AddRange(["1", "1"]);

				if (!String.IsNullOrWhiteSpace(attack.Message))
					arguments.Add(Quote(attack.Message));

				if (!String.IsNullOrWhiteSpace(attack.Effect))
				{
					var parameters = attack.Effect is "AttackPoisonComponent" or "AttackAgeComponent"
						? $"{Number(attack.EffectValue1) ?? 0}, {Number(attack.EffectValue2) ?? 0}"
						: $"{Number(attack.EffectValue1) ?? 0}";
					arguments.Add($"new {attack.Effect}({parameters})");
				}

				var expression = $"new {attack.Type}({String.Join(", ", arguments)})";
				var weight = Number(attack.Weight) ?? (int)Math.Round(100.0 / Attacks.Count);

				lines.Add(weighted ? $"\t{{ {expression}, {weight} }}," : $"\t{expression},");
			}

			lines.Add("};");
			lines.Add(String.Empty);
		}

		if (Spells.Count > 0)
		{
			lines.Add($"{v}.Spells = new CreatureSpellCollection()");
			lines.Add("{");

			foreach (var spell in Spells.Where(s => !String.IsNullOrWhiteSpace(s.Spell)))
			{
				var cooldown = Number(spell.Cooldown) is int seconds && seconds > 0 ? $"TimeSpan.FromSeconds({seconds})" : "TimeSpan.Zero";
				lines.Add($"\t{{ new CreatureSpellEntry(new CreatureSpell<{spell.Spell.Trim()}>({Number(spell.Skill) ?? 1}), {Number(spell.Weight) ?? 100}, {cooldown}) }},");
			}

			lines.Add("};");
			lines.Add(String.Empty);
		}

		var gear = new List<string>();

		foreach (var wielded in new[] { Weapon, Shield }.Where(w => !String.IsNullOrWhiteSpace(w)))
			gear.Add($"{v}.Wield(new {wielded.Trim()}());");

		foreach (var equipped in new[] { Armor, Helmet, Gauntlets, Boots }.Where(e => !String.IsNullOrWhiteSpace(e)))
			gear.Add($"{v}.Equip(new {equipped.Trim()}());");

		if (!String.IsNullOrWhiteSpace(Brain))
			gear.Add($"{v}.Brain = new {Brain.Trim()}({v});");

		if (gear.Count > 0)
		{
			lines.AddRange(gear);
			lines.Add(String.Empty);
		}

		var gold = Number(Gold);
		var loot = Loot.Where(l => !String.IsNullOrWhiteSpace(l.Item)).ToList();

		if (gold is int amount)
			lines.Add($"{v}.AddGold({amount});");

		if (loot.Count > 0)
		{
			lines.Add($"{v}.AddLoot(new LootPack(");

			for (var i = 0; i < loot.Count; i++)
			{
				var entry = loot[i];
				lines.Add($"  new LootPackEntry({(entry.AtSpawn ? "true" : "false")}, {entry.Item.Trim()}, {Number(entry.Chance) ?? 1}){(i < loot.Count - 1 ? "," : String.Empty)}");
			}

			lines.Add("));");
		}

		if (gold != null || loot.Count > 0)
			lines.Add(String.Empty);

		if (Blocks.Count > 0)
		{
			lines.Add($"{v}.Blocks = new CreatureBlockCollection");
			lines.Add("{");

			foreach (var block in Blocks)
				lines.Add($"\tnew CreatureBlock({Number(block.Roll) ?? 1}, {Quote(block.Message ?? String.Empty)}),");

			lines.Add("};");
		}

		lines.Add($"return {v};");

		return String.Join(nl, lines.Select(l => l.Length > 0 ? "\t" + l : l));
	}

	/// <summary>Fills the form from an existing script. Returns how many things it recognized.</summary>
	public int Parse(string script, IReadOnlyCollection<string> shields, IReadOnlyCollection<string> armor,
		IReadOnlyCollection<string> helmets, IReadOnlyCollection<string> gauntlets, IReadOnlyCollection<string> boots)
	{
		var found = 0;
		var match = Match.Empty;

		bool Find(string pattern)
		{
			match = Regex.Match(script, pattern);

			if (match.Success)
				found++;

			return match.Success;
		}

		if (Find(@"var\s+(\w+)\s*=\s*new\s+([\w<>.]+)\s*\("))
		{
			Variable = match.Groups[1].Value;
			CreatureType = match.Groups[2].Value;
		}

		if (Find(@"\bName\s*=\s*""((?:[^""\\]|\\.)*)""")) Name = Regex.Unescape(match.Groups[1].Value);
		if (Find(@"\bMaxHealth\s*=\s*(\d+)")) MaxHealth = match.Groups[1].Value;
		if (Find(@"\bBaseDodge\s*=\s*(\d+)")) BaseDodge = match.Groups[1].Value;
		if (Find(@"\bExperience\s*=\s*(\d+)")) Experience = match.Groups[1].Value;
		if (Find(@"\bBody\s*=\s*(\d+)")) Body = match.Groups[1].Value;
		if (Find(@"\bMovement\s*=\s*(\d+)")) Movement = match.Groups[1].Value;
		if (Find(@"ShieldPenetration\.(\w+)")) Penetration = match.Groups[1].Value;
		if (Find(@"Alignment\s*=\s*Alignment\.(\w+)")) Alignment = match.Groups[1].Value;
		if (Find(@"CombatantSearchStrategy\s*=\s*new\s+(\w+)")) SearchStrategy = match.Groups[1].Value;
		if (Find(@"\.Brain\s*=\s*new\s+(\w+)")) Brain = match.Groups[1].Value;
		if (Find(@"\.AddGold\(\s*(\d+)")) Gold = match.Groups[1].Value;

		if (Find(@"Hue\s*=\s*Color\.FromArgb\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*(?:,\s*(\d+)\s*)?\)"))
		{
			var values = Enumerable.Range(1, 4).Select(i => match.Groups[i].Success ? Byte.Parse(match.Groups[i].Value) : (byte?)null).ToArray();

			HueMode = "Custom";
			(HueA, HueR, HueG, HueB) = values[3] is byte b
				? (values[0]!.Value, values[1]!.Value, values[2]!.Value, b)
				: ((byte)255, values[0]!.Value, values[1]!.Value, values[2]!.Value);
		}
		else if (Find(@"Hue\s*=\s*Color\.(\w+)"))
		{
			HueMode = "Named";
			HueName = match.Groups[1].Value;
		}

		foreach (var flag in Flags)
		{
			var flagMatch = Regex.Match(script, $@"\b{flag.Name}\s*=\s*(true|false)");

			if (flagMatch.Success)
			{
				flag.IsChecked = flagMatch.Groups[1].Value == "true";
				found++;
			}
		}

		var immunity = Regex.Match(script, @"Immunity\s*=\s*([^,;\r\n]+)");
		if (immunity.Success)
		{
			foreach (var option in Immunities)
				option.IsChecked = immunity.Groups[1].Value.Contains("CreatureImmunity." + option.Name);
			found++;
		}

		var weakness = Regex.Match(script, @"Weakness\s*=\s*([^,;\r\n]+)");
		if (weakness.Success)
		{
			foreach (var option in Weaknesses)
				option.IsChecked = weakness.Groups[1].Value.Contains("CreatureWeakness." + option.Name);
			found++;
		}

		var wielded = Regex.Matches(script, @"\.Wield\(\s*new\s+(\w+)").Select(m => m.Groups[1].Value).ToList();
		if (wielded.Count > 0)
		{
			Weapon = wielded.FirstOrDefault(w => !shields.Contains(w)) ?? String.Empty;
			Shield = wielded.FirstOrDefault(e => shields.Contains(e)) ?? String.Empty;
			found++;
		}

		var equipped = Regex.Matches(script, @"\.Equip\(\s*new\s+(\w+)").Select(m => m.Groups[1].Value).ToList();
		if (equipped.Count > 0)
		{
			Armor = equipped.FirstOrDefault(e => armor.Contains(e)) ?? String.Empty;
			Helmet = equipped.FirstOrDefault(e => helmets.Contains(e)) ?? String.Empty;
			Gauntlets = equipped.FirstOrDefault(e => gauntlets.Contains(e)) ?? String.Empty;
			Boots = equipped.FirstOrDefault(e => boots.Contains(e)) ?? String.Empty;
			found++;
		}

		var attacks = Regex.Matches(script, @"new\s+(CreatureBasicAttack|CreatureAttack)\(([^\r\n]*?)\)\s*(?:,\s*(\d+)\s*\})?\s*,?\s*(?=\r?$)", RegexOptions.Multiline);
		if (attacks.Count > 0)
		{
			Attacks.Clear();

			foreach (Match attack in attacks)
				Attacks.Add(AttackRow.FromArguments(attack.Groups[1].Value, attack.Groups[2].Value, attack.Groups[3].Value));

			found++;
		}

		var spells = Regex.Matches(script, @"CreatureSpell<(\w+)>\(\s*(\d+)[^)]*\)\s*,\s*(\d+)\s*,\s*(?:TimeSpan\.FromSeconds\(\s*(\d+)\s*\)|TimeSpan\.Zero)");
		if (spells.Count > 0)
		{
			Spells.Clear();

			foreach (Match spell in spells)
				Spells.Add(new SpellRow { Spell = spell.Groups[1].Value, Skill = spell.Groups[2].Value, Weight = spell.Groups[3].Value, Cooldown = spell.Groups[4].Success ? spell.Groups[4].Value : "0" });

			found++;
		}

		var blocks = Regex.Matches(script, @"new\s+CreatureBlock\(\s*(\d+)\s*,\s*""((?:[^""\\]|\\.)*)""");
		if (blocks.Count > 0)
		{
			Blocks.Clear();

			foreach (Match block in blocks)
				Blocks.Add(new BlockRow { Roll = block.Groups[1].Value, Message = Regex.Unescape(block.Groups[2].Value) });

			found++;
		}

		var loot = Regex.Matches(script, @"new\s+LootPackEntry\(\s*(true|false)\s*,\s*(\w+)\s*,\s*(\d+)");
		if (loot.Count > 0)
		{
			Loot.Clear();

			foreach (Match entry in loot)
				Loot.Add(new LootRow { AtSpawn = entry.Groups[1].Value == "true", Item = entry.Groups[2].Value, Chance = entry.Groups[3].Value });

			found++;
		}

		return found;
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

	public string Type { get => _type; set => SetProperty(ref _type, value); }
	public string Skill { get => _skill; set => SetProperty(ref _skill, value); }
	public string MinDamage { get => _minDamage; set => SetProperty(ref _minDamage, value); }
	public string MaxDamage { get => _maxDamage; set => SetProperty(ref _maxDamage, value); }
	public string Weight { get => _weight; set => SetProperty(ref _weight, value); }
	public string Message { get => _message; set => SetProperty(ref _message, value); }

	/// <summary>An optional attack component: daze, prone, poison or age.</summary>
	public string Effect { get => _effect; set => SetProperty(ref _effect, value); }
	public string EffectValue1 { get => _effectValue1; set => SetProperty(ref _effectValue1, value); }
	public string EffectValue2 { get => _effectValue2; set => SetProperty(ref _effectValue2, value); }

	/// <summary>Reads <c>skill[, min, max][, "message"][, new Component(a[, b])]</c>.</summary>
	public static AttackRow FromArguments(string type, string arguments, string weight)
	{
		var row = new AttackRow { Type = type, Weight = weight };

		var message = Regex.Match(arguments, @"""((?:[^""\\]|\\.)*)""");
		if (message.Success)
			row.Message = Regex.Unescape(message.Groups[1].Value);

		var effect = Regex.Match(arguments, @"new\s+(\w+)\(\s*(\d+)?\s*(?:,\s*(\d+)\s*)?\)");
		if (effect.Success)
		{
			row.Effect = effect.Groups[1].Value;
			row.EffectValue1 = effect.Groups[2].Value;
			row.EffectValue2 = effect.Groups[3].Value;
		}

		var plain = Regex.Replace(arguments, @"""(?:[^""\\]|\\.)*""|new\s+\w+\([^)]*\)", String.Empty);
		var numbers = Regex.Matches(plain, @"\d+").Select(m => m.Value).ToList();

		if (numbers.Count > 0)
			row.Skill = numbers[0];

		if (numbers.Count >= 3)
		{
			row.MinDamage = numbers[1];
			row.MaxDamage = numbers[2];
		}

		return row;
	}
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
	public string Skill { get => _skill; set => SetProperty(ref _skill, value); }
	public string Weight { get => _weight; set => SetProperty(ref _weight, value); }
	public string Cooldown { get => _cooldown; set => SetProperty(ref _cooldown, value); }
}

public class LootRow : ObservableObject
{
	private bool _atSpawn = true;
	private string _item = String.Empty;
	private string _chance = "10";

	public bool AtSpawn { get => _atSpawn; set => SetProperty(ref _atSpawn, value); }
	public string Item { get => _item; set => SetProperty(ref _item, value); }
	public string Chance { get => _chance; set => SetProperty(ref _chance, value); }
}
