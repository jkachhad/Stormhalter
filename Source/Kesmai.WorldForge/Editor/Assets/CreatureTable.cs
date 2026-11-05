using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;

namespace Kesmai.WorldForge.Editor.Assets;

/// <summary>
/// One row of the creature table: a segment entity and the values its OnSpawn sets, read through the OnSpawn builder.
/// Changing a value writes the builder's script back to OnSpawn.
/// </summary>
public class CreatureRow : ObservableObject
{
	/// <summary>The OnSpawn values the table shows, as (builder property, column header).</summary>
	public static readonly (string Property, string Header)[] Columns =
	[
		(nameof(OnSpawnBuilder.Name), "Name in game"),
		(nameof(OnSpawnBuilder.Level), "Level"),
		(nameof(OnSpawnBuilder.MaxHealth), "Health"),
		(nameof(OnSpawnBuilder.MaxMana), "Mana"),
		(nameof(OnSpawnBuilder.Experience), "Exp"),
		(nameof(OnSpawnBuilder.BaseDodge), "Dodge"),
		(nameof(OnSpawnBuilder.Alignment), "Alignment"),
		(nameof(OnSpawnBuilder.Movement), "Movement"),
		(nameof(OnSpawnBuilder.Body), "Body"),
		(nameof(OnSpawnBuilder.Gold), "Gold"),
	];

	private readonly OnSpawnBuilder _builder;
	private readonly Script _onSpawn;

	public SegmentEntity Entity { get; }

	public string EntityName
	{
		get => Entity.Name;
		set
		{
			if (!string.IsNullOrWhiteSpace(value) && value != Entity.Name)
			{
				Entity.Name = value.Trim();
				OnPropertyChanged();
			}
		}
	}

	public string Group
	{
		get => Entity.Group;
		set
		{
			if (value != Entity.Group)
			{
				Entity.Group = value;
				OnPropertyChanged();
			}
		}
	}

	public string CreatureType => _builder?.CreatureType ?? "";

	/// <summary>Why the OnSpawn values can't be changed here, or empty when they can.</summary>
	public string Problem { get; }

	public bool CanEdit => _builder is not null;

	public CreatureRow(SegmentEntity entity, AssetCatalog catalog)
	{
		Entity = entity;
		_onSpawn = entity.Scripts.FirstOrDefault(s => s.Name == "OnSpawn");

		var script = _onSpawn?.Body;

		if (string.IsNullOrWhiteSpace(script))
		{
			Problem = "No OnSpawn";
			return;
		}

		var builder = new OnSpawnBuilder();
		builder.SetEnumChoices(catalog.Immunities, catalog.Weaknesses);

		var result = builder.Parse(script, catalog.Shields, catalog.Armor, catalog.Helmets, catalog.Gauntlets, catalog.Boots);

		if (!result.Succeeded)
		{
			Problem = result.Problem;
			return;
		}

		// only a script the builder writes back the same can be changed from the table
		if (!OnSpawnBuilder.SameCode(builder.Generate(), script))
		{
			Problem = "The builder would write this OnSpawn differently, so it isn't changed here. Edit it in the Entities editor.";
			return;
		}

		_builder = builder;
		Problem = result.DtoOnly ? "CreatureDTO values only (OnSpawn picks with if or switch)" : "";
	}

	/// <summary>An OnSpawn value by builder property name; setting one writes OnSpawn.</summary>
	public string this[string property]
	{
		get => _builder is null ? "" : (string)typeof(OnSpawnBuilder).GetProperty(property)!.GetValue(_builder) ?? "";
		set
		{
			if (_builder is null || (value ?? "") == this[property])
				return;

			typeof(OnSpawnBuilder).GetProperty(property)!.SetValue(_builder, (value ?? "").Trim());

			_onSpawn.Body = _builder.Generate();
			WeakReferenceMessenger.Default.Send(new SegmentEntityChanged(Entity));

			OnPropertyChanged("Item[]");
		}
	}

	/// <summary>The ways a bulk edit can change a value.</summary>
	public static readonly string[] BulkModes = ["Set to", "Add", "Multiply by", "Change by %"];

	/// <summary>
	/// A value after a bulk edit: <paramref name="amount"/> itself for "Set to", otherwise the number worked out from
	/// the current one (whole numbers stay whole). Null when the current value or the amount isn't a number.
	/// </summary>
	public static string Bulk(string current, string mode, string amount)
	{
		if (mode == BulkModes[0])
			return amount;

		static bool Number(string text, out double value)
			=> double.TryParse((text ?? "").Replace("_", "").Trim(), System.Globalization.NumberStyles.Float,
				System.Globalization.CultureInfo.InvariantCulture, out value);

		if (!Number(current, out var value) || !Number(amount, out var by))
			return null;

		var result = mode switch
		{
			"Add" => value + by,
			"Multiply by" => value * by,
			"Change by %" => value * (1 + by / 100),
			_ => value,
		};

		var whole = !(current ?? "").Contains('.');

		return whole
			? Math.Round(result, MidpointRounding.AwayFromZero).ToString(System.Globalization.CultureInfo.InvariantCulture)
			: result.ToString(System.Globalization.CultureInfo.InvariantCulture);
	}

	public static List<CreatureRow> For(Segment segment, AssetCatalog catalog)
		=> segment.Entities.Select(entity => new CreatureRow(entity, catalog)).ToList();
}
