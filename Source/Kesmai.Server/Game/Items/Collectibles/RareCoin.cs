using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

/// <summary>
/// A rare coin. Using it adds it to the account's <see cref="RareCoins"/> collection, and the coin is used up.
/// </summary>
public class RareCoin : CollectibleItem
{
	/// <inheritdoc />
	public override int LabelNumber => 6000028;

	/// <inheritdoc />
	public override int Weight => 1;

	/// <inheritdoc />
	public override int Category => ItemCategory.Miscellaneous.Id;

	/// <inheritdoc />
	public override string CollectionId => RareCoins.CollectionId;

	/// <summary>
	/// Initializes a new instance of the <see cref="RareCoin"/> class for a coin of <see cref="RareCoins"/>.
	/// </summary>
	public RareCoin(uint entryId) : base(73, entryId)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RareCoin"/> class.
	/// </summary>
	public RareCoin(Serial serial) : base(serial)
	{
	}
}
