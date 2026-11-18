# Collections

A collection is a set of things players can collect. Players see their collections in the Collections window, opened from the Titan bar: one card per collection with its progress, and a page per collection with everything they've found.

The library is the collection that ships with the game: every book and scroll a player has read is in it, and they can open it again from there. This page shows how to add a book to it, how to add a collection of your own, in this repository or in a segment, and the items that fill it.

## Where a collection can live

A collection is a class. Put it in this repository under `Source/Kesmai.Server`, or in a segment's own repository with the rest of that segment's code. The server finds it by itself when it starts; there's nothing to register.

## Choosing how a collection behaves

Every collection makes three choices:

| Setting | Options | What it means for players |
| --- | --- | --- |
| `Scope` | `Account` | Everything collected is shared by all characters on the account. |
| | `Player` | Each character has their own collection. |
| `Visibility` | `Public` | Players see every entry, including the ones they haven't found. |
| | `Hidden` | Missing entries show as blank "???" slots, so players know how many are left but not what they are. |
| | `Secret` | Missing entries aren't shown at all, and there's no total. A secret collection only appears once the player finds its first entry. |
| `View` | a view name | How the client draws the collection. Leave it out to get the standard view: a searchable list, with the selected entry's details beside it. The library has its own view, `Library`. A view the client doesn't know also gets the standard view, so a segment's collection always uses it. |

Each entry can be collected once. Players can't remove entries.

> **Choose the scope before players start collecting.** Progress is saved under the account or the character, so once a collection has saved progress, the server refuses to start if its `Scope` changes, rather than lose everyone's entries. Change it back, or give the collection a new id to start it over empty.

## Adding a book or scroll to the library

A book or scroll is a class that derives from `PublishedBook` or `PublishedScroll`, in [Stormhalter-Library](https://github.com/jkachhad/Stormhalter-Library) or in a segment. Give it a fixed id that starts with the repository or segment that adds it, and its pages:

```csharp
public class KeshmaHistory : PublishedBook
{
	public KeshmaHistory() : base("ng.keshma-history", new Page(new ParagraphPageElement("Long ago, Keshma...")))
	{
		Title = "A History of Keshma";
		ItemId = 411;
		Author = "Unknown";
		Description = "A short history of the town.";
		Quality = ItemQuality.Uncommon;
	}
}
```

`Title` and `ItemId` (the sprite of the book in hand) are what players see first; set them, or the server warns at startup. The books and scrolls that come with the game take these details from the game's `Library.xml` instead, which also puts them first in the library.

Players add a book to their library by reading it: they hold the book and double-click it, and the book is used up. Staff can create one with `add Book ng.keshma-history`. The same `Book` item shows scrolls.

## Adding a collection from XML

Most collections are a list of entries in an XML file.

**1. Write the data file.** In this repository, put it in `Content/Data/Collections/`, for example `Content/Data/Collections/Figurines.xml`. In a segment, put it anywhere in the segment's repository, for example `Keshma/Collections/Figurines.xml`: the server packs every `.xml` file of a segment into it when it compiles the segment.

```xml
<collection>
	<entry id="stormhalter.tiger-figurine" itemId="50" title="Tiger Figurine" quality="Common"
		   description="A tiger carved from soapstone.">
		<property key="material">Soapstone</property>
	</entry>
	<entry id="stormhalter.griffin-figurine" itemId="180" title="Griffin Figurine" quality="Rare"
		   description="Its wings are inlaid with silver.">
		<property key="material">Ivory</property>
	</entry>
</collection>
```

| Attribute | Required | Meaning |
| --- | --- | --- |
| `id` | yes | A name unique within the collection: lowercase words and numbers separated by dots or dashes. Start it with the repository or segment that adds the entry, such as `stormhalter.tiger-figurine` or `ng.frost-wyrm-skull`. Because of the prefix, entries added from different places can't clash, so you never need to check anyone else's ids. |
| `itemId` | no | The item sprite used as the entry's icon. |
| `title`, `description` | no | Shown in the collection window. |
| `quality` | no | `Poor`, `Common` (the default), `Uncommon`, `Rare`, `Epic`, `Legendary`, `Artifact` or `Mythical`. Colors the title. |
| `order` | no | Display order, a number. Entries are shown in the order of the file otherwise. |

`<property>` elements add details the window shows under the entry, such as what a figurine is made of. Keys are lowercase words separated by dots (`material`, `figurine.size`). The optional `type` attribute is `string` (the default), `int`, `float`, `bool`, `datetime` or `timespan`.

> **Never change or reuse an entry `id`.** Players' collections are saved by id. If you remove an entry, players who found it keep it, and it comes back if you add the entry again later.

Collection files stay on the server, so players can't read ahead in a hidden or secret collection.

**2. Add the collection class**, for example `Game/Collections/CarvedFigurines.cs`:

```csharp
public class CarvedFigurines : XmlCollection
{
	public const string CollectionId = "stormhalter.figurines";

	public CarvedFigurines() : base(CollectionId, @"Data/Collections/Figurines.xml")
	{
		Title = "Carved Figurines";

		Scope = CollectionScope.Account;
		Visibility = CollectionVisibility.Hidden;
	}
}
```

The path is where the server finds the file: `Data/Collections/<file>` for a file in this repository's `Content/Data/Collections/`, or the file's path in the segment's repository, such as `Keshma/Collections/Figurines.xml`. If the file can't be found, the server stops at startup and names the collection. The collection id is lowercase words separated by dots, and like entry ids, it must never change once players start collecting.

## Adding the items that fill it

Derive from `CollectibleItem`. The item takes its icon, quality and description from its collection entry, and adds the entry when a player uses it in hand.

```csharp
public class CarvedFigurine : CollectibleItem
{
	public override int LabelNumber => 6000036; /* figurine */
	public override int Weight => 2;
	public override int Category => ItemCategory.Miscellaneous.Id;

	public override string CollectionId => CarvedFigurines.CollectionId;

	public CarvedFigurine(string entryId) : base(50, entryId)
	{
	}

	public CarvedFigurine(Serial serial) : base(serial)
	{
	}
}
```

When a player uses the item:

- If they don't have the entry yet, it's added, they're told "The Griffin Figurine has been added to your Carved Figurines collection.", and the item is used up.
- If they already have it, they're told so and keep the item.

Change what happens by overriding:

| Override | Use it to |
| --- | --- |
| `ConsumeOnAdd` | Return `false` to keep the item after its entry is added, for a trophy the player keeps carrying. |
| `OnAdded(player, collection, entry)` | Do something extra when the entry is added. |
| `CreateState(player)` | Give the entry a starting state (see below). |

Staff can create one with `add CarvedFigurine stormhalter.griffin-figurine`, where `stormhalter.griffin-figurine` is the entry id. The item shows the entry's icon, so one item class covers every figurine.

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
		yield return new CollectionEntry("stormhalter.first-blood") { Title = "First Blood", ItemId = 34, Quality = ItemQuality.Rare, Order = 1 };
		yield return new CollectionEntry("stormhalter.dragon-slayer") { Title = "Dragon Slayer", ItemId = 47, Quality = ItemQuality.Epic, Order = 2 };
	}
}
```

Entries built in code are shown by their `Order` (0 unless you set it), then by id.

`ReflectionCollection<T>` makes one entry for every class of a type the server knows about, in any repository or segment. The library is one: an entry for every `Publication`. Each class needs a constructor without parameters, and `CreateEntry` must give each one a fixed id.

## Adding entries from other code

Any code can add an entry, for example a quest reward:

```csharp
if (CollectionRegistry.TryGetCollection("stormhalter.trophies", out var trophies))
{
	var result = trophies.TryAdd(player, "stormhalter.dragon-slayer");

	if (result == CollectionAddResult.Added)
		player.SendServerMessage("You earned the Dragon Slayer trophy.");
}
```

`TryAdd` returns `Added`, `AlreadyOwned`, `UnknownEntry` (the collection has no such entry) or `Invalid` (no player or account). Collections never send messages or delete items themselves; the code that adds the entry decides.

## Reacting when an entry is added

`EventSink.CollectionEntryAdded`, `CollectionEntryRemoved` and `CollectionEntryStateChanged` are raised after every change to any collection, for example to grant something when a player completes one:

```csharp
EventSink.CollectionEntryAdded += args =>
{
	if (args.Collection.Id == CarvedFigurines.CollectionId && args.Collection.IsComplete(args.Record))
		args.Player.SendServerMessage("You've found every carved figurine!");
};
```

Check the collection first: the events fire for every collection, the library included. A handler runs in the middle of adding the entry, so keep it short, and don't let it throw: an error there stops the code that added the entry, for example before a `CollectibleItem` is used up.

## Details that change per player

Some entries show details that differ between players, such as how long ago a figurine was found.

- **Calculated details:** override `GetOwnerProperties(player, entry, owned)` and return properties built from `owned.Acquired` (when the player got the entry). Nothing extra is saved.
- **Saved details:** give the entry a state when it's added (`CreateState`, or the last argument of `TryAdd`), and change it later with `collection.UpdateState(player, entryId, state)`. Write and read it with `CollectionState.Write` and `CollectionState.Read`, and start it with a version number so you can change its format later. The library saves each book's current page this way.

## Opening an entry

Players can click an entry the client view makes clickable, which asks the server to open it. Override `OnAccess(player, entry, owned)` in the collection to respond, for example by showing a gump; the library opens the book again. Players can only open entries they own.
