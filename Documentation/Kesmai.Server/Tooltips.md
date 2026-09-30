# Item Tooltips

Tooltips give players a quick explanation of an item when they hover over it. A tooltip can show the item's name and quality, its description, equipment values, and extra details such as charges or ownership.

This guide explains how Stormhalter item authors can supply that information and keep it up to date.

## What Builds a Tooltip

A finished tooltip combines three sources:

- **The item description** supplies localized descriptive text.
- **The client item type** supplies familiar panels such as weapon damage, armor, and elemental protection.
- **Rich tooltip content** supplies optional text, images, progress bars, and separators requested from the server when the player hovers over the item.

These sources solve different problems. Descriptions are best for lore and stable explanatory text. Existing equipment panels are best for standard numeric item properties. Rich content is best for supplemental or player-dependent information.

## Adding a Description

Override `GetDescription` and add localization entries. Call the base method so inherited text is retained.

```csharp
public override void GetDescription(List<LocalizationEntry> entries)
{
    base.GetDescription(entries);
    entries.Add(new LocalizationEntry(6250000));
}
```

Use `GetDescriptionPrefix` or `GetDescriptionSuffix` only when the text intentionally belongs before or after the normal description. Localized entries are preferred over hard-coded English because they use the game's normal localization pipeline.

Descriptions are sent as the item's `Description` property and cached on the client. They are resent automatically when the property snapshot changes.

## Adding Rich Content

Override `GetTooltipEntries` for information that should be assembled when a player asks to see the tooltip.

```csharp
using System.Drawing;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

public override void GetTooltipEntries(PlayerEntity beholder, Tooltip tooltip)
{
    base.GetTooltipEntries(beholder, tooltip);

    tooltip.AddLocalizedText(
        6600100,              /* Power: {0} */
        Color.Cyan,
        TooltipTextStyle.Passive,
        -5000,
        Power.ToString());
}
```

Always call `base.GetTooltipEntries`. The base item adds common details such as remaining charges, enchanted or conjured state, and binding ownership.

The `beholder` is the player viewing the item. Use it when the displayed result genuinely depends on the viewer, such as profession, level, or eligibility. Tooltip generation should only read state; it should not send messages, roll random values, start timers, or change the item.

## Available Rich Entries

### Text

```csharp
tooltip.AddText("A temporary warning", Color.Orange, TooltipTextStyle.Harmful);
```

Raw text is useful for highly dynamic values. Prefer localized text for normal player-facing wording.

### Localized text

```csharp
tooltip.AddLocalizedText(6600100, TooltipTextStyle.Passive, 0, Power.ToString()); /* Power: {0} */
```

This is the preferred option for player-facing wording. Arguments fill the `{0}`, `{1}` placeholders and should be dynamic values such as counts or names.

Pass numbers as text, as in `Power.ToString()`. A plain number argument is treated as the index of another localization entry, so passing `5` would show entry 5 instead of the number 5.

### Adding tooltip wording

Tooltip wording has its own **Tooltips** section in `Content/Localization/Enu.xml`, numbered from 6600000. To add a line, give it a free number in that section:

```xml
<!-- Tooltips -->
<entry index="6600000">Charges: {0} / {1}</entry>
...
<entry index="6600100">Power: {0}</entry>
```

Don't reuse the general `{0}` and `{0}: {1}` entries with English words as arguments; that text can't be translated. Players see new entries after their client content is updated.

### Image

```csharp
tooltip.AddImage("UI/Textures/Example", order: -100);
```

The texture must exist in client content. If it doesn't, the player sees the rest of the tooltip without that image. An optional source rectangle can select part of a texture.

### Progress bar

```csharp
tooltip.AddProgressBar("Durability", Durability, MaximumDurability, -100);
```

Use a progress bar for a bounded value where the proportion is more helpful than a plain number.

### Separator

```csharp
tooltip.AddSeparator(-200);
```

Separators visually group related rich entries.

## Ordering

Rich entries with higher `order` values appear first. Entries with the same order keep the order in which they were added.

Use a small set of intentional order bands rather than assigning unrelated arbitrary values. The common base item currently uses:

- `0` for charges;
- `-10000` for enchanted or conjured state;
- `-20000` for binding information.

Client-side panels have their own ordering rules, so a rich-entry order does not move an armor or weapon panel.

## Identification

Items require identification by default. Until an item is identified:

- its tooltip shows an **Unidentified** marker instead of the equipment panels;
- `GetTooltipEntries` is not called, so no rich content is shown;
- its stat modifiers are not sent to the player.

Its description and other item properties are still sent.

Some items have nothing worth hiding, such as coins, food, or keys. Override `RequiresIdentification` to return `false` for these:

```csharp
public override bool RequiresIdentification => false;
```

Such an item always counts as identified: `Identified` returns `true`, the tooltip never says Unidentified, and any `if (Identified)` text in your `GetDescription` is shown. Choose this per item type and keep it fixed; don't switch it on and off at runtime.

Do not work around identification by placing secret values into always-visible client properties or descriptions.

## Keeping Cached Tooltips Fresh

Rich tooltip responses are cached on the client. If a value used by `GetTooltipEntries` changes, invalidate the cache:

```csharp
private int _power;

public int Power
{
    get => _power;
    set
    {
        if (_power == value)
            return;

        _power = value;
        InvalidateTooltip();
    }
}
```

The common details already refresh themselves: remaining and maximum charges on charged items, enchanted and conjured state, and the owner of a bindable item. When you change charges in your own code, use the `ChargesCurrent` property (for example `ChargesCurrent--`), not its backing field, so the tooltip updates.

If the same change also affects an item property, call `InvalidateProperties`; if it affects `GetStatModifiers`, call `UpdateStatModifiers`. A replacement property or modifier snapshot clears the cached tooltip on the client. Scalar item updates such as amount, color, or quality do not themselves invalidate rich content, so call `InvalidateTooltip` when `GetTooltipEntries` also depends on one of those values.

Viewer-dependent content also needs invalidation when the relevant viewer state changes. If a tooltip depends on a new character property, make sure that property's update path clears affected tooltip caches.

## Standard Equipment Panels

Weapons, armor, shields, gauntlets, and other equipment already have specialized client panels. Damage, weapon flags, range, armor values, and concussion protection come from each item class's `GetClientProperties` override. Fire, ice, and daze protection come from the modifiers that `Equipment.GetStatModifiers` supplies, so they are not also sent as properties.

Reuse the shared `ItemPropertyId` and the value type registered for it in `ItemPropertySchema`; never assign an ad hoc ID, reuse a removed ID, or change the type of a published property. See [Client Item Properties](StatModifiers.md#client-item-properties) for the full property and modifier rules.

Use an existing property and panel when it already represents the same gameplay concept. Adding a new typed property or a new panel requires coordinated Kesmai server and client changes; it cannot be completed in a Stormhalter server item alone.

The `GetStatModifiers` snapshot is sent to the client for every observed item. The elemental protection panel reads it; other modifiers are not displayed in tooltips yet. Do not duplicate stat modifiers as hard-coded tooltip text.

## Players Can Turn Tooltips Off

The client's settings include an **Item Tooltips** checkbox, on by default. Players who turn it off see only the item's name when hovering, without the description, equipment panels, or rich content. Don't put information players need to play only in a tooltip; make sure it is also available another way, such as the item's look description.

## Choosing the Right Technique

| Need | Recommended approach |
| --- | --- |
| Stable lore or usage description | `GetDescription` with localization |
| Existing weapon, armor, or protection value | Existing typed item property and client panel |
| Dynamic supplemental value | `GetTooltipEntries` |
| Viewer-dependent value | `GetTooltipEntries` using `beholder` |
| Visual bounded value | `AddProgressBar` |
| Client texture or icon | `AddImage` |
| Continuous gameplay stat bonus | `GetStatModifiers`, not tooltip code |

See [Stat Modifiers](StatModifiers.md) for implementing continuous gameplay bonuses. A tooltip explains a value; it must not apply or remove that value.

## Checklist

- Call the base description or tooltip method.
- Prefer localization for player-facing wording.
- Respect item identification.
- Keep tooltip generation deterministic and free of side effects.
- Invalidate cached rich content whenever one of its inputs changes.
- Keep server property IDs and value types aligned with the shared property schema.
- Reuse existing client panels when their semantics match.
- Keep gameplay modifiers in `GetStatModifiers`.
