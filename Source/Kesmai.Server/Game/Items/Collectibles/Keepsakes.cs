using System;
using Kesmai.Server.Engines.Collections;

namespace Kesmai.Server.Items;

/// <summary>
/// A keepsake: something one of a land's people lost, which players find on the creature that took it and keep in
/// their Keepsakes collection.
/// </summary>
/// <remarks>
/// Every keepsake class with a parameterless constructor is an entry, in any repository or segment and any namespace,
/// as the library's publications are. A keepsake sets its title, owner, icon, item name, quality and land in its
/// constructor. Players add it with the <see cref="Keepsake"/> item.
/// </remarks>
public abstract class KeepsakeDefinition
{
	/// <summary>
	/// Gets the entry id, such as <c>og.kesmai-coin-pouch</c>. Start it with the repository that adds the keepsake and
	/// its land. Players' collections are saved by it, so it must never change.
	/// </summary>
	public string Id { get; }

	/// <summary>
	/// Gets the title, "Lost" and the thing, such as "Lost Coin Pouch". Messages put "The" before it.
	/// </summary>
	public string Title { get; protected set; }

	/// <summary>
	/// Gets who the keepsake belonged to, such as "Kesmai's banker". The collection shows it as the entry's owner.
	/// </summary>
	public string Owner { get; protected set; }

	public string Description { get; protected set; }

	/// <summary>
	/// Gets the item sprite, shown on the item and as the entry's icon.
	/// </summary>
	public int ItemId { get; protected set; }

	/// <summary>
	/// Gets the item's name in the game, one of the client's item names, such as 6000099 for "pouch".
	/// </summary>
	public int LabelNumber { get; protected set; }

	public ItemQuality Quality { get; protected set; } = ItemQuality.Common;

	/// <summary>
	/// Gets the land the keepsake is listed under, such as "Kesmai".
	/// </summary>
	public string Group { get; protected set; }

	/// <summary>
	/// Gets the place of the keepsake's land among the lands. Keepsakes of one land share it, and are listed by id.
	/// </summary>
	public int Order { get; protected set; }

	protected KeepsakeDefinition(string id)
	{
		Id = id;
	}
}

/// <summary>
/// Keepsakes: the things the people of each land lost, listed under their land.
/// </summary>
/// <remarks>
/// An entry for every <see cref="KeepsakeDefinition"/>. The collection is Hidden, so the keepsakes a player hasn't
/// found show as blank slots under their land.
/// </remarks>
public class KeepsakeCollection : ReflectionCollection<KeepsakeDefinition>
{
	public const string CollectionId = "stormhalter.keepsakes";

	/// <summary>
	/// The sprite and item name of a keepsake that sets none: a pouch. An item with sprite 0 disconnects the game client.
	/// </summary>
	public const int DefaultItemId = 391;
	public const int DefaultLabelNumber = 6000099;

	public KeepsakeCollection() : base(CollectionId)
	{
		Title = "Keepsakes";

		Scope = CollectionScope.Account;
		Visibility = CollectionVisibility.Hidden;
	}

	protected override CollectionEntry CreateEntry(KeepsakeDefinition keepsake)
	{
		if (String.IsNullOrEmpty(keepsake.Title) || keepsake.ItemId <= 0)
			Log.Warn($"Keepsake {keepsake.Id} ({keepsake.GetType().Name}) has no title or item id. Set Title and ItemId in its constructor.");

		var entry = new CollectionEntry(keepsake.Id)
		{
			Title = String.IsNullOrEmpty(keepsake.Title) ? keepsake.GetType().Name : keepsake.Title,
			Description = keepsake.Description ?? String.Empty,
			ItemId = (keepsake.ItemId > 0) ? keepsake.ItemId : DefaultItemId,
			Quality = keepsake.Quality ?? ItemQuality.Common,
			Group = keepsake.Group ?? String.Empty,
			Order = keepsake.Order,
		};

		if (!String.IsNullOrEmpty(keepsake.Owner))
			entry.Properties.Set("owner", keepsake.Owner);

		return entry;
	}
}
