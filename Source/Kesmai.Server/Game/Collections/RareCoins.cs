using Kesmai.Server.Engines.Collections;

namespace Kesmai.Server.Game;

/// <summary>
/// Rare coins, shared by every character on an account. Missing coins show as blank slots.
/// </summary>
/// <remarks>
/// Entries come from <c>Content/Data/Collections/Coins.xml</c>. <see cref="Items.RareCoin"/> adds them.
/// </remarks>
public class RareCoins : XmlCollection
{
	public const string CollectionId = "stormhalter.coins";

	public RareCoins() : base(CollectionId, @"Data/Collections/Coins.xml")
	{
		Title = "Rare Coins";
		View = "Coins";

		Scope = CollectionScope.Account;
		Visibility = CollectionVisibility.Hidden;

		Order = 10;
	}
}
