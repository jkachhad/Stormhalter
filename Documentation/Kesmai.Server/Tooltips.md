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

Descriptions are sent as ordinary item properties and cached on the client.

## Adding Rich Content

Override `WriteTooltip` for information that should be assembled when a player asks to see the tooltip.

```csharp
using System.Drawing;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

public override void WriteTooltip(Tooltip tooltip, MobileEntity beholder)
{
    base.WriteTooltip(tooltip, beholder);

    tooltip.AddLocalizedText(
        6500001,
        Color.Cyan,
        TooltipTextStyle.Passive,
        -5000,
        "Power",
        Power);
}
```

Always call `base.WriteTooltip`. The base item adds common details such as remaining charges, enchanted or conjured state, and binding ownership.

The `beholder` is the character viewing the item. Use it when the displayed result genuinely depends on the viewer, such as profession, level, or eligibility. Tooltip generation should only read state; it should not send messages, roll random values, start timers, or change the item.

## Available Rich Entries

### Text

```csharp
tooltip.AddText("A temporary warning", Color.Orange, TooltipTextStyle.Harmful);
```

Raw text is useful for highly dynamic values. Prefer localized text for normal player-facing wording.

### Localized text

```csharp
tooltip.AddLocalizedText(6500001, TooltipTextStyle.Passive, 0, "Charges", 5);
```

Arguments can be text or localization numbers. This is the preferred option when the wording belongs in the localization data.

### Image

```csharp
tooltip.AddImage("UI/Textures/Example", order: -100);
```

The texture must exist in client content. An optional source rectangle can select part of a texture.

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

Items require identification by default. Until an item is identified, the server does not call `WriteTooltip`, and the client displays an Unidentified marker instead of sensitive details.

Override `RequiresIdentification` only when the item's rich information is intentionally public:

```csharp
public override bool RequiresIdentification => false;
```

Do not work around identification by placing secret values into always-visible client properties or descriptions.

## Keeping Cached Tooltips Fresh

Rich tooltip responses are cached. If a property used by `WriteTooltip` changes, invalidate the cache:

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

If the same change also affects the normal item model, send its normal item delta as well. A regular item update does not automatically invalidate rich tooltip content.

Viewer-dependent content also needs invalidation when the relevant viewer state changes. If a tooltip depends on a new character property, make sure that property's update path clears affected tooltip caches.

## Standard Equipment Panels

Weapons, armor, shields, gauntlets, and other equipment already have specialized client panels. Their values come from the server item's `WriteProperties` method and the matching client model's `ReadProperties` method.

Use an existing property and panel when it already represents the same gameplay concept. Adding a new typed property or a new panel requires coordinated Kesmai server and client changes; it cannot be completed in a Stormhalter server item alone.

The `GetStatModifiers` snapshot is not automatically displayed in tooltips today. Do not duplicate every stat modifier as hard-coded tooltip text in anticipation of that feature. Structured transport is planned separately in the Kesmai repository.

## Choosing the Right Technique

| Need | Recommended approach |
| --- | --- |
| Stable lore or usage description | `GetDescription` with localization |
| Existing weapon, armor, or protection value | Existing typed item property and client panel |
| Dynamic supplemental value | `WriteTooltip` |
| Viewer-dependent value | `WriteTooltip` using `beholder` |
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
- Keep server `WriteProperties` and client `ReadProperties` in the same field order.
- Reuse existing client panels when their semantics match.
- Keep gameplay modifiers in `GetStatModifiers`.
