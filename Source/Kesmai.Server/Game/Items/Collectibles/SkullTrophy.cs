using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

/// <summary>
/// A trophy skull. Using it adds it to the account's <see cref="TrophySkulls"/> collection; the skull
/// stays with the player as a trophy.
/// </summary>
public class SkullTrophy : CollectibleItem
{
	/// <inheritdoc />
	public override int LabelNumber => 6000085;

	/// <inheritdoc />
	public override int Weight => 40;

	/// <inheritdoc />
	public override int Category => ItemCategory.Miscellaneous.Id;

	/// <inheritdoc />
	public override string CollectionId => TrophySkulls.CollectionId;

	/// <inheritdoc />
	protected override bool ConsumeOnAdd => false;

	/// <summary>
	/// Initializes a new instance of the <see cref="SkullTrophy"/> class for a skull of <see cref="TrophySkulls"/>.
	/// </summary>
	public SkullTrophy(uint entryId) : base(47, entryId)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SkullTrophy"/> class.
	/// </summary>
	public SkullTrophy(Serial serial) : base(serial)
	{
	}
}
