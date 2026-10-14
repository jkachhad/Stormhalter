using System;
using System.Collections.Generic;
using Kesmai.Server.Engines.Collections;

namespace Kesmai.Server.Game;

/// <summary>
/// Dragon eggs, kept by each character. Every entry is shown, found or not. The entries are built in code.
/// </summary>
/// <remarks>
/// An egg grows from the day it's collected. Each egg rolls a growth rate when it's added
/// (<see cref="Items.DragonEgg"/>) and saves it as its state; its age, growth and stage are
/// calculated from that when the collection is sent, so nothing else is stored.
/// </remarks>
public class DragonEggs : Collection
{
	public const string CollectionId = "stormhalter.eggs";

	private const byte StateVersion = 1;

	public DragonEggs() : base(CollectionId)
	{
		Title = "Dragon Eggs";
		View = "Eggs";

		Scope = CollectionScope.Player;
		Visibility = CollectionVisibility.Public;

		Order = 20;
	}

	protected override IEnumerable<CollectionEntry> Build()
	{
		yield return CreateEgg(1, 169, "Drake Egg", "A leathery ochre egg, warm to the touch.", ItemQuality.Uncommon, "Drake", 20);
		yield return CreateEgg(2, 169, "Wyvern Egg", "Its shell is ridged like a wyvern's back.", ItemQuality.Rare, "Wyvern", 30);
		yield return CreateEgg(3, 191, "Golden Dragon Egg", "A small golden egg surrounded by a fiery glow.", ItemQuality.Epic, "Dragon", 45);
		yield return CreateEgg(4, 191, "Ice Dragon Egg", "Frost spreads across anything it rests on.", ItemQuality.Legendary, "Ice Dragon", 60);
	}

	private static CollectionEntry CreateEgg(uint id, int itemId, string title, string description, ItemQuality quality, string hatchling, int hatchDays)
	{
		var entry = new CollectionEntry(id)
		{
			ItemId = itemId,
			Title = title,
			Description = description,
			Quality = quality,
		};

		entry.Properties.Set("hatchling", hatchling);
		entry.Properties.Set("hatch.days", hatchDays);

		return entry;
	}

	/// <summary>
	/// Creates an egg's state with its growth rate, as a percentage of normal (100).
	/// </summary>
	public static byte[] CreateState(int growthRate)
	{
		return CollectionState.Write(writer =>
		{
			writer.Write(StateVersion);
			writer.Write(growthRate);
		});
	}

	/// <summary>
	/// Gets an egg's growth rate from its state; 100 when it has none.
	/// </summary>
	public static int GetGrowthRate(OwnedEntry owned)
	{
		if (owned is null || owned.State is null || owned.State.Length is 0)
			return 100;

		return CollectionState.Read(owned.State, (ref SpanReader reader) =>
		{
			var version = reader.ReadByte();

			return version switch
			{
				1 => reader.ReadInt32(),
				_ => 100,
			};
		});
	}

	/// <summary>
	/// Gets how far an egg has grown, from 0 to 1.
	/// </summary>
	public static float GetGrowth(CollectionEntry entry, OwnedEntry owned, DateTime now)
	{
		if (!entry.Properties.TryGet<int>("hatch.days", out var hatchDays) || hatchDays <= 0)
			return 1f;

		var days = (float)(now - owned.Acquired).TotalDays * GetGrowthRate(owned) / 100f;

		return Math.Clamp(days / hatchDays, 0f, 1f);
	}

	public static string GetStage(float growth)
	{
		if (growth >= 1f)
			return "Ready to hatch";

		if (growth >= 0.66f)
			return "Cracking";

		if (growth >= 0.33f)
			return "Stirring";

		return "Dormant";
	}

	/// <inheritdoc />
	public override CollectionProperties GetOwnerProperties(PlayerEntity player, CollectionEntry entry, OwnedEntry owned)
	{
		var now = DateTime.Now;
		var growth = GetGrowth(entry, owned, now);

		var properties = new CollectionProperties();

		properties.Set("age", now - owned.Acquired);
		properties.Set("growth", growth);
		properties.Set("stage", GetStage(growth));
		properties.Set("growth.rate", GetGrowthRate(owned));

		return properties;
	}
}
