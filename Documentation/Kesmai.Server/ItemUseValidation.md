# Item Use Validation

Item-use validation answers two separate questions:

1. May this entity use the item?
2. What should happen when an actual attempt is denied?

Keeping those questions separate allows tooltips, equipment bonuses, AI, and other background calculations to check an item without displaying messages or causing a fumble.

## API Overview

| API | Use it for |
| --- | --- |
| `ValidateUse(MobileEntity entity)` | Defining an item's requirements and optional denial reason. |
| `CanUse(MobileEntity entity)` | Checking eligibility without feedback or other side effects. |
| `entity.TryUse(item)` | Performing a non-combat attempt and displaying its denial reason. |
| `entity.TryUseForCombat(item)` | Performing a combat attempt; denial displays its reason and fumbles the item. |

Derived items with additional use requirements should override `ValidateUse`. Call `CanUse` when only a side-effect-free boolean result is needed; it automatically invokes the item's most-derived `ValidateUse` implementation. Derived items without additional requirements do not need to override either method.

## Defining Item Requirements

Always check the base result first. This preserves binding restrictions and requirements inherited from the item's base class.

```csharp
public override ItemUseResult ValidateUse(MobileEntity entity)
{
    var result = base.ValidateUse(entity);

    if (!result.IsAllowed)
        return result;

    if (entity is not PlayerEntity player)
        return ItemUseResult.Denied();

    if (player.Profession != Profession.Wizard)
        return ItemUseResult.Denied("Only wizards may use this staff.");

    if (player.Level < 20)
        return ItemUseResult.Denied(Color.Red, "You must be level 20 to use this staff.");

    return ItemUseResult.Allowed;
}
```

Localized reasons are also supported:

```csharp
return ItemUseResult.Denied(new LocalizationEntry(6300001));
return ItemUseResult.Denied(Color.Red, new LocalizationEntry(6300001));
```

A reason is optional. Prefer a useful reason when a player can correct the problem, such as a missing level, profession, alignment, unlock, or ownership requirement.

## Checking Without Feedback

Use `CanUse` when merely asking whether an item is eligible:

```csharp
if (item.CanUse(player))
    entries.Add(new LocalizationEntry("You can use this item."));
```

This is appropriate for tooltips, AI decisions, previews, stat modifier calculations, and other queries that may happen frequently.

## Performing an Attempt

For an actual non-combat interaction, use `TryUse`:

```csharp
if (!player.TryUse(item))
    return false;

ActivateItem(player, item);
return true;
```

If validation fails, `TryUse` displays the reason returned by the item. It does not fumble or move the item.

Combat code uses `TryUseForCombat`:

```csharp
if (!attacker.TryUseForCombat(weapon) || attacker.CheckFumble(weapon))
    return true;
```

When use requirements fail, `TryUseForCombat` displays the reason and fumbles the item. `CheckFumble` still handles the separate chance that an otherwise usable weapon is randomly or mechanically fumbled.

## Use Requirements and Stat Modifiers

Equipment stat eligibility uses the same pure validation by default. Guard the modifier calculation with `CanApplyStatModifiers` so an item that cannot be used does not contribute its continuous modifiers:

```csharp
protected override StatModifierSet GetStatModifiers(MobileEntity wearer)
{
    var modifiers = base.GetStatModifiers(wearer);

    if (CanApplyStatModifiers(wearer))
        modifiers.Add(EntityStat.MaxHealth, 10);

    return modifiers;
}
```

Override `CanApplyStatModifiers` only when an item may be used but its stat bonus has an additional, distinct condition. The method must remain free of messages and other side effects. See [Equipment Stat Modifiers](StatModifiers.md) for the complete snapshot API.

## Pitfalls to Avoid

### Sending messages from validation

Do not call `SendMessage`, `SendCombatMessage`, or `SendLocalizedMessage` from `ValidateUse`. A harmless `CanUse` query could otherwise display messages repeatedly. Return the message in `ItemUseResult.Denied`.

### Calling `base.CanUse` from an override

Do not do this:

```csharp
public override ItemUseResult ValidateUse(MobileEntity entity)
{
    if (!base.CanUse(entity))
        return ItemUseResult.Denied();

    return ItemUseResult.Allowed;
}
```

`CanUse` calls `ValidateUse`, so this recurses into the override. Call `base.ValidateUse` and preserve its result.

### Using combat validation everywhere

`TryUseForCombat` fumbles on denial. Do not use it for equipping, inspecting, activating, or calculating an item. Use `TryUse` for a real non-combat attempt and `CanUse` for a query.

### Duplicating requirements

Do not independently reimplement the same profession, level, ownership, or alignment rule in `CanApplyStatModifiers`. Put the shared rule in `ValidateUse`; the default modifier eligibility will reuse it.

### Mutating state during validation

Validation may run during equipment refreshes and other background operations. Do not bind or move items, update quests, consume resources, start cooldowns, roll randomness, or change stats from `ValidateUse`.
