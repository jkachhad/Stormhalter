using System.Collections.Generic;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Collections;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

/// <summary>
/// An item that adds an entry to a collection when used in hand.
/// </summary>
/// <remarks>
/// The item takes its icon, quality and description from its collection entry. Whether it's used up
/// when added is up to the subclass (<see cref="ConsumeOnAdd"/>).
/// </remarks>
public abstract class CollectibleItem : ItemEntity
{
	/// <summary>
	/// Gets the id of the collection this item belongs to.
	/// </summary>
	public abstract string CollectionId { get; }

	/// <summary>
	/// Gets whether the item is deleted once its entry is added. Defaults to <c>true</c>.
	/// </summary>
	protected virtual bool ConsumeOnAdd => true;

	/// <summary>
	/// Gets or sets the collection entry this item adds.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public uint EntryId { get; set; }

	/// <summary>
	/// Gets the collection, or <c>null</c> if it isn't registered.
	/// </summary>
	public Collection Collection => CollectionRegistry.TryGetCollection(CollectionId, out var collection) ? collection : null;

	/// <summary>
	/// Gets the entry this item adds, or <c>null</c>.
	/// </summary>
	public CollectionEntry Entry
	{
		get
		{
			var collection = Collection;

			if (collection is not null && collection.TryGetEntry(EntryId, out var entry))
				return entry;

			return null;
		}
	}

	/// <inheritdoc />
	public override int ItemId
	{
		get
		{
			var entry = Entry;

			if (entry is not null && entry.ItemId > 0)
				return entry.ItemId;

			return base.ItemId;
		}
		set => base.ItemId = value;
	}

	/// <inheritdoc />
	public override ItemQuality Quality
	{
		get
		{
			var entry = Entry;

			if (entry is not null)
				return entry.Quality;

			return base.Quality;
		}
		set => base.Quality = value;
	}

	protected CollectibleItem(int itemId, uint entryId) : base(itemId)
	{
		EntryId = entryId;
	}

	protected CollectibleItem(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		var entry = Entry;

		if (entry is null)
		{
			base.GetDescription(entries);
			return;
		}

		entries.Add(new LocalizationEntry(6200000, $"the {entry.Title}.")); /* [You are looking at] {0} */

		if (!string.IsNullOrEmpty(entry.Description))
			entries.Add(new LocalizationEntry(entry.Description));
	}

	/// <inheritdoc />
	public override ActionType GetAction()
	{
		if (Container is Hands)
			return ActionType.Use;

		return base.GetAction();
	}

	/// <inheritdoc />
	public override bool HandleInteraction(MobileEntity entity, ActionType action)
	{
		if (action != ActionType.Use || entity is not PlayerEntity player)
			return base.HandleInteraction(entity, action);

		AddTo(player);

		return true;
	}

	/// <inheritdoc />
	public override void GetInteractions(PlayerEntity source, List<InteractionEntry> entries)
	{
		if (Container is Hands)
		{
			entries.Add(ItemActionInteraction.Use);
			entries.Add(InteractionSeparator.Instance);
		}

		base.GetInteractions(source, entries);
	}

	/// <summary>
	/// Adds this item's entry to <paramref name="player"/>'s collection and tells them the result.
	/// </summary>
	public CollectionAddResult AddTo(PlayerEntity player)
	{
		var collection = Collection;
		var entry = Entry;

		if (collection is null || entry is null)
		{
			player.SendServerMessage("Nothing happens.");
			return CollectionAddResult.UnknownEntry;
		}

		var result = collection.TryAdd(player, entry.Id, CreateState(player));

		switch (result)
		{
			case CollectionAddResult.Added:
			{
				player.SendServerMessage($"The {entry.Title} has been added to your {collection.Title} collection.");

				OnAdded(player, collection, entry);

				if (ConsumeOnAdd)
					Delete();

				break;
			}
			case CollectionAddResult.AlreadyOwned:
			{
				player.SendServerMessage($"Your {collection.Title} collection already has the {entry.Title}.");
				break;
			}
			default:
			{
				player.SendServerMessage("Nothing happens.");
				break;
			}
		}

		return result;
	}

	/// <summary>
	/// Creates the entry's starting state when it's added, or <c>null</c> for none.
	/// </summary>
	protected virtual byte[] CreateState(PlayerEntity player) => null;

	/// <summary>
	/// Called after the entry was added, before the item is deleted.
	/// </summary>
	protected virtual void OnAdded(PlayerEntity player, Collection collection, CollectionEntry entry)
	{
	}

	/// <inheritdoc />
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1); /* version */
		writer.Write(EntryId);
	}

	/// <inheritdoc />
	public override void Deserialize(ref SpanReader reader)
	{
		base.Deserialize(ref reader);

		var version = reader.ReadInt16();

		switch (version)
		{
			case 1:
			{
				EntryId = reader.ReadUInt32();
				break;
			}
		}
	}
}
