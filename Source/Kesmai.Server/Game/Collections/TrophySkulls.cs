using Kesmai.Server.Engines.Collections;

namespace Kesmai.Server.Game;

/// <summary>
/// One skull of the <see cref="TrophySkulls"/> collection. Each concrete class is an entry.
/// </summary>
/// <remarks>
/// <see cref="Id"/> is saved with players' collections: never change or reuse one.
/// </remarks>
public abstract class TrophySkull
{
	public abstract uint Id { get; }
	public abstract string Title { get; }
	public abstract string Description { get; }

	/// <summary>
	/// Gets the item sprite used as the icon.
	/// </summary>
	public abstract int ItemId { get; }

	/// <summary>
	/// Gets the creature body the client shows as the trophy's large image.
	/// </summary>
	public abstract int Body { get; }

	public virtual ItemQuality Quality => ItemQuality.Uncommon;
}

/// <summary>
/// Trophy skulls, shared by an account and kept secret until found. The entries are discovered by
/// reflection: every <see cref="TrophySkull"/> class is one.
/// </summary>
public class TrophySkulls : ReflectionCollection<TrophySkull>
{
	public const string CollectionId = "stormhalter.skulls";

	public TrophySkulls() : base(CollectionId)
	{
		Title = "Trophy Skulls";
		View = "Skulls";

		Scope = CollectionScope.Account;
		Visibility = CollectionVisibility.Secret;

		Order = 30;
	}

	protected override CollectionEntry CreateEntry(TrophySkull skull)
	{
		var entry = new CollectionEntry(skull.Id)
		{
			ItemId = skull.ItemId,
			Title = skull.Title,
			Description = skull.Description,
			Quality = skull.Quality,
		};

		entry.Properties.Set("body", skull.Body);

		return entry;
	}
}

public class BearSkullTrophy : TrophySkull
{
	public override uint Id => 1;
	public override string Title => "Bear Skull";
	public override string Description => "Taken from a great bear of the northern woods.";
	public override int ItemId => 34;
	public override int Body => 55;
}

public class TigerSkullTrophy : TrophySkull
{
	public override uint Id => 2;
	public override string Title => "Tiger Skull";
	public override string Description => "The fangs are still sharp.";
	public override int ItemId => 36;
	public override int Body => 36;
}

public class GriffinSkullTrophy : TrophySkull
{
	public override uint Id => 3;
	public override string Title => "Griffin Skull";
	public override string Description => "Half eagle, half lion, and all of it heavy.";
	public override int ItemId => 35;
	public override int Body => 25;
	public override ItemQuality Quality => ItemQuality.Rare;
}

public class WyvernSkullTrophy : TrophySkull
{
	public override uint Id => 4;
	public override string Title => "Wyvern Skull";
	public override string Description => "Long and narrow, with venom grooves along the teeth.";
	public override int ItemId => 47;
	public override int Body => 65;
	public override ItemQuality Quality => ItemQuality.Rare;
}

public class DragonSkullTrophy : TrophySkull
{
	public override uint Id => 5;
	public override string Title => "Dragon Skull";
	public override string Description => "A trophy few can claim.";
	public override int ItemId => 47;
	public override int Body => 92;
	public override ItemQuality Quality => ItemQuality.Epic;
}

public class IceDragonSkullTrophy : TrophySkull
{
	public override uint Id => 6;
	public override string Title => "Ice Dragon Skull";
	public override string Description => "Rime still forms in its eye sockets.";
	public override int ItemId => 47;
	public override int Body => 120;
	public override ItemQuality Quality => ItemQuality.Legendary;
}
