#nullable enable

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Editor.Assets;
using Kesmai.WorldForge.Editor.Quests;

namespace Kesmai.WorldForge.Editor.Items;

public enum ItemKind
{
	Item,
	Weapon,
}

/// <summary>
/// An item or weapon made in the Item Maker. It becomes a C# class in the segment's Source\Items folder; the definition
/// is stored as XML in a comment at the top of that file, so the item can be reopened and edited.
/// </summary>
public class ItemDefinition : ObservableObject
{
	public const string Marker = "// worldforge-item: ";

	/// <summary>The weapon classes a weapon can be built on. Each takes the item's art id in its constructor.</summary>
	public static readonly string[] WeaponTypes =
		["Sword", "Greatsword", "Rapier", "Axe", "Mace", "Flail", "Dagger", "Staff", "Threestaff", "Halberd", "Bow", "Crossbow"];

	/// <summary>Weapon types that need a second art id: a dagger's poisoned look, a bow's nocked look.</summary>
	public static string? AlternateArtMember(string weaponType) => weaponType switch
	{
		"Dagger" => "protected override int PoisonedItemId",
		"Bow" => "public override int NockedID",
		_ => null,
	};

	/// <summary>Storage categories from Items-External.xml: where the item may be carried.</summary>
	public static readonly (int Number, string Name)[] Categories =
	[
		(1, "Large weapon / back"), (2, "Small weapon / belt"), (3, "Misc / sack"), (4, "Ring / sack"), (5, "Helmet / head"),
		(6, "Amulet / neck"), (7, "Bracelet"), (8, "Gauntlets"), (9, "Armor / back"), (10, "Robe / back"), (11, "Boots"),
		(12, "Consumable / sack"), (13, "Coin"), (14, "Dagger / belt, thrown from belt"), (15, "Halberd / back"),
	];

	private ItemKind _kind;
	private string _className = "NewItem";
	private string _displayName = "trinket";
	private int _labelNumber;
	private int _itemId = 900;
	private string _hue = String.Empty;
	private int _weight = 100;
	private uint _price = 100;
	private int _category = 3;
	private bool _canBind;
	private int _requiredLevel;
	private string _requiredProfession = String.Empty;
	private string _description = String.Empty;
	private string _identifiedDescription = String.Empty;

	private string _weaponType = "Sword";
	private int _alternateItemId;
	private string _skill = String.Empty;
	private int _minimumDamage = 1;
	private int _maximumDamage = 10;
	private int _attackBonus;
	private int _armorBonus;
	private string _penetration = String.Empty;

	public ItemKind Kind { get => _kind; set => SetProperty(ref _kind, value); }

	/// <summary>The C# class name, which is also how scripts create it: new ClassName().</summary>
	public string ClassName { get => _className; set => SetProperty(ref _className, value); }

	/// <summary>What players see the item called. Saved as a localization entry the class points to.</summary>
	public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }

	/// <summary>The localization entry for <see cref="DisplayName"/>; 0 until it has one.</summary>
	public int LabelNumber { get => _labelNumber; set => SetProperty(ref _labelNumber, value); }

	/// <summary>The item art, an id from Items-External.xml or the game's own items.</summary>
	public int ItemId { get => _itemId; set => SetProperty(ref _itemId, value); }

	/// <summary>A color name such as Red, or A,R,G,B; empty for no hue.</summary>
	public string Hue { get => _hue; set => SetProperty(ref _hue, value); }

	public int Weight { get => _weight; set => SetProperty(ref _weight, value); }
	public uint Price { get => _price; set => SetProperty(ref _price, value); }
	public int Category { get => _category; set => SetProperty(ref _category, value); }
	public bool CanBind { get => _canBind; set => SetProperty(ref _canBind, value); }

	/// <summary>The player level needed to use it; 0 for none.</summary>
	public int RequiredLevel { get => _requiredLevel; set => SetProperty(ref _requiredLevel, value); }

	/// <summary>A Profession that alone may use it; empty for anyone.</summary>
	public string RequiredProfession { get => _requiredProfession; set => SetProperty(ref _requiredProfession, value); }

	/// <summary>What looking at the item says.</summary>
	public string Description { get => _description; set => SetProperty(ref _description, value); }

	/// <summary>Added to the description once the item is identified.</summary>
	public string IdentifiedDescription { get => _identifiedDescription; set => SetProperty(ref _identifiedDescription, value); }

	public string WeaponType { get => _weaponType; set => SetProperty(ref _weaponType, value); }

	/// <summary>A dagger's poisoned art or a bow's nocked art; 0 uses the item's own art.</summary>
	public int AlternateItemId { get => _alternateItemId; set => SetProperty(ref _alternateItemId, value); }

	/// <summary>A Skill member such as Sword; empty keeps the weapon type's own skill.</summary>
	public string Skill { get => _skill; set => SetProperty(ref _skill, value); }

	public int MinimumDamage { get => _minimumDamage; set => SetProperty(ref _minimumDamage, value); }
	public int MaximumDamage { get => _maximumDamage; set => SetProperty(ref _maximumDamage, value); }
	public int AttackBonus { get => _attackBonus; set => SetProperty(ref _attackBonus, value); }

	/// <summary>The weapon's blocking bonus.</summary>
	public int ArmorBonus { get => _armorBonus; set => SetProperty(ref _armorBonus, value); }

	/// <summary>A ShieldPenetration member; empty keeps the weapon type's own.</summary>
	public string Penetration { get => _penetration; set => SetProperty(ref _penetration, value); }

	/// <summary>WeaponFlags, checked or not.</summary>
	public ObservableCollection<FlagOption> Flags { get; } = new();

	public void SetFlagChoices(System.Collections.Generic.IEnumerable<string> names)
	{
		var checkedNames = Flags.Where(f => f.IsChecked == true).Select(f => f.Name).ToHashSet();

		Flags.Clear();

		foreach (var name in names)
		{
			var option = new FlagOption(name) { IsChecked = checkedNames.Contains(name) };

			// a ticked flag counts as a change to the item, so the code preview follows it
			option.PropertyChanged += (_, _) => OnPropertyChanged(nameof(Flags));
			Flags.Add(option);
		}
	}

	public static ItemDefinition Create(ItemKind kind)
	{
		var item = new ItemDefinition { Kind = kind };

		if (kind == ItemKind.Weapon)
		{
			item.ClassName = "NewSword";
			item.DisplayName = "sword";
			item.ItemId = 900;
			item.Category = 1;
			item.Weight = 400;
			item.Price = 500;
			item.Description = "You are looking at a sword.";
		}
		else
		{
			item.Description = "You are looking at a trinket.";
		}

		return item;
	}

	public XElement ToXml() => new("item",
		new XAttribute("kind", Kind),
		new XAttribute("class", ClassName),
		new XAttribute("name", DisplayName),
		new XAttribute("label", LabelNumber),
		new XAttribute("itemId", ItemId),
		new XAttribute("hue", Hue),
		new XAttribute("weight", Weight),
		new XAttribute("price", Price),
		new XAttribute("category", Category),
		new XAttribute("canBind", CanBind),
		new XAttribute("level", RequiredLevel),
		new XAttribute("profession", RequiredProfession),
		new XElement("description", Description),
		new XElement("identified", IdentifiedDescription),
		new XElement("weapon",
			new XAttribute("type", WeaponType),
			new XAttribute("alternateItemId", AlternateItemId),
			new XAttribute("skill", Skill),
			new XAttribute("min", MinimumDamage),
			new XAttribute("max", MaximumDamage),
			new XAttribute("attack", AttackBonus),
			new XAttribute("armor", ArmorBonus),
			new XAttribute("penetration", Penetration),
			new XAttribute("flags", String.Join(",", Flags.Where(f => f.IsChecked == true).Select(f => f.Name)))));

	public static ItemDefinition FromXml(XElement element)
	{
		var weapon = element.Element("weapon") ?? new XElement("weapon");

		var item = new ItemDefinition
		{
			Kind = Enum.TryParse<ItemKind>((string?)element.Attribute("kind"), out var kind) ? kind : ItemKind.Item,
			ClassName = (string?)element.Attribute("class") ?? "NewItem",
			DisplayName = (string?)element.Attribute("name") ?? String.Empty,
			LabelNumber = (int?)element.Attribute("label") ?? 0,
			ItemId = (int?)element.Attribute("itemId") ?? 0,
			Hue = (string?)element.Attribute("hue") ?? String.Empty,
			Weight = (int?)element.Attribute("weight") ?? 0,
			Price = (uint?)element.Attribute("price") ?? 0,
			Category = (int?)element.Attribute("category") ?? 3,
			CanBind = (bool?)element.Attribute("canBind") ?? false,
			RequiredLevel = (int?)element.Attribute("level") ?? 0,
			RequiredProfession = (string?)element.Attribute("profession") ?? String.Empty,
			Description = (string?)element.Element("description") ?? String.Empty,
			IdentifiedDescription = (string?)element.Element("identified") ?? String.Empty,
			WeaponType = (string?)weapon.Attribute("type") ?? "Sword",
			AlternateItemId = (int?)weapon.Attribute("alternateItemId") ?? 0,
			Skill = (string?)weapon.Attribute("skill") ?? String.Empty,
			MinimumDamage = (int?)weapon.Attribute("min") ?? 0,
			MaximumDamage = (int?)weapon.Attribute("max") ?? 0,
			AttackBonus = (int?)weapon.Attribute("attack") ?? 0,
			ArmorBonus = (int?)weapon.Attribute("armor") ?? 0,
			Penetration = (string?)weapon.Attribute("penetration") ?? String.Empty,
		};

		foreach (var flag in ((string?)weapon.Attribute("flags") ?? String.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
			item.Flags.Add(new FlagOption(flag) { IsChecked = true });

		return item;
	}

	/// <summary>The definition as the one-line comment at the top of the C# file.</summary>
	public string ToMarkerLine()
		=> Marker + Convert.ToBase64String(Encoding.UTF8.GetBytes(ToXml().ToString(SaveOptions.DisableFormatting)));

	/// <summary>Reads the definition back from a C# file the Item Maker wrote, or returns null for any other file.</summary>
	public static ItemDefinition? FromSource(string source)
	{
		var line = source.Split('\n').Select(l => l.Trim()).FirstOrDefault(l => l.StartsWith(Marker.Trim()));

		if (line is null)
			return null;

		try
		{
			var xml = Encoding.UTF8.GetString(Convert.FromBase64String(line.Substring(Marker.Trim().Length).Trim()));
			return FromXml(XElement.Parse(xml));
		}
		catch
		{
			return null;
		}
	}

	public static bool IsIdentifier(string? value) => QuestDefinition.IsIdentifier(value);
}
