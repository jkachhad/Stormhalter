# Collections

A collection is a set of things players can collect, such as rare coins. Players see their collections in the Collections window, opened from the Titan bar: one card per collection with its progress, and a page per collection with everything they've found.

This page shows how to add a collection and the items that fill it.

## Choosing how a collection behaves

Every collection makes three choices:

| Setting | Options | What it means for players |
| --- | --- | --- |
| `Scope` | `Account` | Everything collected is shared by all characters on the account. |
| | `Player` | Each character has their own collection. |
| `Visibility` | `Public` | Players see every entry, including the ones they haven't found. |
| | `Hidden` | Missing entries show as blank "???" slots, so players know how many are left but not what they are. |
| | `Secret` | Missing entries aren't shown at all, and there's no total. A secret collection only appears once the player finds its first entry. |
| `View` | a view name, such as `Coins` | How the client draws the collection. A name the client doesn't know yet gets the standard list. |

Each entry can be collected once. Players can't remove entries.

## Adding a collection from XML

Most collections are a list of entries in an XML file.

**1. Write the data file** in `Content/Data/Collections/`, for example `Content/Data/Collections/Coins.xml`:

```xml
<collection>
	<entry id="1" itemId="73" title="Kesmai Penny" quality="Common"
		   description="A worn copper penny from the markets of Kesmai.">
		<property key="mint">Kesmai</property>
		<property key="era">Second Age</property>
	</entry>
	<entry id="2" itemId="73" title="Leng Silver Mark" quality="Uncommon"
		   description="A heavy silver coin, cold to the touch.">
		<property key="mint">Leng</property>
	</entry>
</collection>
```

| Attribute | Required | Meaning |
| --- | --- | --- |
| `id` | yes | A number unique within the collection. |
| `itemId` | no | The item sprite used as the entry's icon. |
| `title`, `description` | no | Shown in the collection window. |
| `quality` | no | `Poor`, `Common` (the default), `Uncommon`, `Rare`, `Epic`, `Legendary`, `Artifact` or `Mythical`. Colors the title. |
| `order` | no | Display order. Entries are ordered by `id` otherwise. |

`<property>` elements add details the window shows under the entry, such as where a coin was minted. Keys are lowercase words separated by dots (`mint`, `egg.stage`). The optional `type` attribute is `string` (the default), `int`, `float`, `bool`, `datetime` or `timespan`.

> **Never change or reuse an entry `id`.** Players' collections are saved by id. If you remove an entry, players who found it keep it, and it comes back if you add the entry again later.

Files in `Content/Data/Collections/` stay on the server, so players can't read ahead in a hidden or secret collection.

**2. Add the collection class** anywhere under `Source/Kesmai.Server`, for example `Game/Collections/RareCoins.cs`:

```csharp
public class RareCoins : XmlCollection
{
	public const string CollectionId = "stormhalter.coins";

	public RareCoins() : base(CollectionId, @"Data/Collections/Coins.xml")
	{
		Title = "Rare Coins";
		View = "Coins";

		Scope = CollectionScope.Account;
		Visibility = CollectionVisibility.Hidden;
	}
}
```

The server finds the class by itself; there's nothing to register. The collection id is lowercase words separated by dots, and like entry ids, it must never change once players start collecting.

## Adding the items that fill it

Derive from `CollectibleItem`. The item takes its icon, quality and description from its collection entry, and adds the entry when a player uses it in hand.

```csharp
public class RareCoin : CollectibleItem
{
	public override int LabelNumber => 6000028; /* coin */
	public override int Weight => 1;
	public override int Category => ItemCategory.Miscellaneous.Id;

	public override string CollectionId => RareCoins.CollectionId;

	public RareCoin(uint entryId) : base(73, entryId)
	{
	}

	public RareCoin(Serial serial) : base(serial)
	{
	}
}
```

When a player uses the item:

- If they don't have the entry yet, it's added, they're told "The Kesmai Penny has been added to your Rare Coins collection.", and the item is used up.
- If they already have it, they're told so and keep the item.

Change what happens by overriding:

| Override | Use it to |
| --- | --- |
| `ConsumeOnAdd` | Return `false` to keep the item after its entry is added, for a trophy the player keeps carrying. |
| `OnAdded(player, collection, entry)` | Do something extra when the entry is added. |
| `CreateState(player)` | Give the entry a starting state (see below). |

Staff can create a coin with `add RareCoin 3`, where `3` is the entry id.

## Collections built in code

A collection doesn't have to use XML. Derive from `Collection` and return the entries from `Build`:

```csharp
public class Trophies : Collection
{
	public Trophies() : base("stormhalter.trophies")
	{
		Title = "Trophies";
		Visibility = CollectionVisibility.Secret;
	}

	protected override IEnumerable<CollectionEntry> Build()
	{
		yield return new CollectionEntry(1) { Title = "First Blood", ItemId = 34, Quality = ItemQuality.Rare };
		yield return new CollectionEntry(2) { Title = "Dragon Slayer", ItemId = 47, Quality = ItemQuality.Epic };
	}
}
```

`ReflectionCollection<T>` makes one entry for every class of a type the server knows about. Each class needs a constructor without parameters, and `CreateEntry` must give each one a fixed id.

## Adding entries from other code

Any code can add an entry, for example a quest reward:

```csharp
if (CollectionRegistry.TryGetCollection("stormhalter.trophies", out var trophies))
{
	var result = trophies.TryAdd(player, 2);

	if (result == CollectionAddResult.Added)
		player.SendServerMessage("You earned the Dragon Slayer trophy.");
}
```

`TryAdd` returns `Added`, `AlreadyOwned`, `UnknownEntry` (the collection has no such entry) or `Invalid` (no player or account). Collections never send messages or delete items themselves; the code that adds the entry decides.

## Details that change per player

Some entries show details that differ between players, such as how old a dragon egg is.

- **Calculated details:** override `GetOwnerProperties(player, entry, owned)` and return properties built from `owned.Acquired` (when the player got the entry). Nothing extra is saved.
- **Saved details:** give the entry a state when it's added (`CreateState`, or the last argument of `TryAdd`), and change it later with `collection.UpdateState(player, entryId, state)`. Write and read it with `CollectionState.Write` and `CollectionState.Read`, and start it with a version number so you can change its format later.

## Examples in this repository

| Collection | Built from | Scope | Visibility | Items |
| --- | --- | --- | --- | --- |
| `RareCoins` (`Game/Collections/RareCoins.cs`) | `Content/Data/Collections/Coins.xml` | Account | Hidden | `RareCoin`, used up when added |
| `DragonEggs` (`Game/Collections/DragonEggs.cs`) | code (`Build`) | Player | Public | `DragonEgg`, used up; saves a growth rate and shows the egg's age, growth and stage |
| `TrophySkulls` (`Game/Collections/TrophySkulls.cs`) | every `TrophySkull` class | Account | Secret | `SkullTrophy`, kept by the player |

Staff can create them with `add RareCoin 3`, `add DragonEgg 2` and `add SkullTrophy 5`.

## Opening an entry

Players can click an entry the client view makes clickable, which asks the server to open it. Override `OnAccess(player, entry, owned)` in the collection to respond, for example by showing a gump. Players can only open entries they own.
