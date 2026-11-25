namespace Kesmai.Server.Items;

/// <summary>
/// A keepsake's item. Using it in hand adds its keepsake to the player's Keepsakes collection and uses it up.
/// </summary>
/// <remarks>
/// It shows its keepsake's sprite and item name, so one item covers every keepsake. Staff can create one with
/// <c>add Keepsake og.kesmai-coin-pouch</c>, where <c>og.kesmai-coin-pouch</c> is the keepsake's id.
/// </remarks>
public class Keepsake : CollectibleItem
{
	/// <inheritdoc />
	public override string CollectionId => KeepsakeCollection.CollectionId;

	/// <summary>
	/// Gets the keepsake this item adds, or <c>null</c>.
	/// </summary>
	public KeepsakeDefinition Definition
	{
		get
		{
			if (Collection is KeepsakeCollection keepsakes && keepsakes.TryGetInstance(EntryId, out var keepsake))
				return keepsake;

			return null;
		}
	}

	/// <inheritdoc />
	public override int LabelNumber
	{
		get
		{
			var keepsake = Definition;

			if (keepsake is not null && keepsake.LabelNumber > 0)
				return keepsake.LabelNumber;

			return KeepsakeCollection.DefaultLabelNumber;
		}
	}

	/// <inheritdoc />
	/// <remarks>Its name and story show to anyone, as a library book's do, so there's nothing to identify.</remarks>
	public override bool RequiresIdentification => false;

	/// <inheritdoc />
	public override int Weight => 2;

	/// <inheritdoc />
	public override int Category => ItemCategory.Miscellaneous.Id;

	public Keepsake(string entryId) : base(KeepsakeCollection.DefaultItemId, entryId)
	{
	}

	public Keepsake(Serial serial) : base(serial)
	{
	}
}
