using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

/// <summary>
/// A dragon egg. Using it adds it to the character's <see cref="DragonEggs"/> collection with a random
/// growth rate, and the egg is used up.
/// </summary>
public class DragonEgg : CollectibleItem
{
	/// <inheritdoc />
	public override int LabelNumber => 6000033;

	/// <inheritdoc />
	public override int Weight => 15;

	/// <inheritdoc />
	public override int Category => ItemCategory.Miscellaneous.Id;

	/// <inheritdoc />
	public override string CollectionId => DragonEggs.CollectionId;

	/// <summary>
	/// Initializes a new instance of the <see cref="DragonEgg"/> class for an egg of <see cref="DragonEggs"/>.
	/// </summary>
	public DragonEgg(uint entryId) : base(169, entryId)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DragonEgg"/> class.
	/// </summary>
	public DragonEgg(Serial serial) : base(serial)
	{
	}

	/// <summary>
	/// Each egg grows at 80% to 120% of the normal rate.
	/// </summary>
	protected override byte[] CreateState(PlayerEntity player) => DragonEggs.CreateState(Utility.RandomBetween(80, 120));
}
