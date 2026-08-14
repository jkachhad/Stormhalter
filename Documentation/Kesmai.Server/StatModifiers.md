# Equipment Stat Modifiers

This guide explains how to give equipment, buffs, and debuffs continuous stat modifiers using the source-aware API.

## Overview

Each active modifier source—such as an item, buff, or debuff—supplies a complete snapshot of its stat modifiers. The wearer stores that snapshot under the instance that supplied it.

When a source changes, the system:

1. removes the exact snapshot previously registered by that source;
2. calculates a new snapshot;
3. applies the new values.

When the source is removed, the stored snapshot is removed without recalculating it. This prevents stale modifiers and arithmetic drift when a value depends on quality, level, profession, skills, alignment, segment, equipment, or status sources.

## API at a Glance

| API | Purpose |
| --- | --- |
| `GetStatModifiers(MobileEntity wearer)` | Returns the item's complete continuous stat snapshot. Override this when creating equipment bonuses. |
| `StatModifierSet.Add(...)` | Adds an ordinary `EntityStat` modifier to the snapshot. |
| `StatModifierSet.AddMaximumValue(...)` | Changes the maximum-value constraint of an `EntityStat`. |
| `UpdateStatModifiers()` | Replaces this source's active snapshot after one of its dependencies changes. It does nothing while the source is inactive. |
| `MobileEntity.UpdateStatModifiers()` | Refreshes every registered item and status source for the wearer. |
| `IStatModifierSource` | Identifies an item or status that owns a replaceable modifier snapshot. |
| `CanApplyStatModifiers(MobileEntity wearer)` | Determines whether the item may currently provide its stat modifiers. It uses side-effect-free item validation by default. Override it only when modifier eligibility differs from use eligibility. |
| `ActivateBonus(...)` / `InactivateBonus(...)` | Lifecycle operations used by equipment containers. Normal item code should not call these to refresh a bonus. |

The system resolves the wearer from the item's `Parent`. Items do not need to store their own wearer reference.

Sources are compared by object identity. Always replace and remove a modifier snapshot with the same item or status instance that registered it.

## Basic Example

This ring always grants five Strength while equipped:

```csharp
public class ExampleStrengthRing : Ring
{
    public ExampleStrengthRing() : base(1)
    {
    }

    public ExampleStrengthRing(Serial serial) : base(serial)
    {
    }

    protected override StatModifierSet GetStatModifiers(MobileEntity wearer)
    {
        var modifiers = base.GetStatModifiers(wearer);
        modifiers.Add(EntityStat.Strength, 5);
        return modifiers;
    }
}
```

Always call `base.GetStatModifiers(wearer)`. Base equipment classes may already provide protection, regeneration, or other modifiers.

## Multiple Modifiers and Modifier Types

One snapshot can contain any number of entries:

```csharp
protected override StatModifierSet GetStatModifiers(MobileEntity wearer)
{
    var modifiers = base.GetStatModifiers(wearer);

    modifiers.Add(EntityStat.MaxHealth, 20);
    modifiers.Add(EntityStat.FireProtection, 5);
    modifiers.Add(EntityStat.MagicDamageDealtIncrease, 10, ModifierType.AdditivePercent);

    return modifiers;
}
```

The default modifier type is `ModifierType.Constant`. Specify another type only when the stat calculation requires percentage behavior.

Repeated entries are preserved. This is important for multiplicative modifiers, where two entries may not be equivalent to one combined entry.

## Changing a Stat's Maximum Value

Use `AddMaximumValue` when the item changes an attribute's allowed maximum rather than its current calculated value:

```csharp
protected override StatModifierSet GetStatModifiers(MobileEntity wearer)
{
    var modifiers = base.GetStatModifiers(wearer);

    modifiers.Add(EntityStat.FireProtection, 10);
    modifiers.AddMaximumValue(EntityStat.FireProtection, 10);

    return modifiers;
}
```

Do not implement maximum-value changes by modifying `MaximumValue` directly in activation hooks. The snapshot system must own both application and removal.

## Quality-Dependent Bonus

The built-in `Quality` setter automatically refreshes an active item's snapshot:

```csharp
protected override StatModifierSet GetStatModifiers(MobileEntity wearer)
{
    var modifiers = base.GetStatModifiers(wearer);
    var bonus = Math.Max(0, Quality.Value - 1);

    if (bonus > 0)
        modifiers.Add(EntityStat.HealthRegenerationRate, bonus);

    return modifiers;
}
```

There is no need to call `InactivateBonus` and `ActivateBonus` around a quality change.

## Refreshing a Custom Item Property

If a custom property affects the snapshot, refresh the item in that property's setter:

```csharp
private int _power;

[CommandProperty(AccessLevel.GameMaster)]
public int Power
{
    get => _power;
    set
    {
        if (_power == value)
            return;

        _power = value;
        UpdateStatModifiers();
        Delta(ItemDelta.Update);
    }
}

protected override StatModifierSet GetStatModifiers(MobileEntity wearer)
{
    var modifiers = base.GetStatModifiers(wearer);
    modifiers.Add(EntityStat.MaxMana, Power);
    return modifiers;
}
```

`UpdateStatModifiers` safely does nothing if the item is not active.

## Wearer-Dependent Bonus

The wearer passed to `GetStatModifiers` may be used to calculate the snapshot:

```csharp
protected override StatModifierSet GetStatModifiers(MobileEntity wearer)
{
    var modifiers = base.GetStatModifiers(wearer);

    if (wearer is PlayerEntity player && player.Profession == Profession.Wizard)
        modifiers.Add(EntityStat.MaxMana, player.Level * 2);

    return modifiers;
}
```

Core gameplay refreshes equipment after level, profession, alignment, relevant skill, segment, facet, and equipment changes. If new wearer state affects equipment, its change path must call `wearer.UpdateStatModifiers()`.

Use an ordinary equality expression for professions. `Profession` is not a compile-time constant, so this property-pattern form does not compile:

```csharp
// Do not use this.
if (wearer is PlayerEntity { Profession: Profession.Wizard })
{
}
```

## Segment-Dependent Bonus

Items may inspect the segment occupied by the wearer:

```csharp
protected override StatModifierSet GetStatModifiers(MobileEntity wearer)
{
    var modifiers = base.GetStatModifiers(wearer);

    if (wearer.Segment != null && wearer.Segment.Index == 3)
        modifiers.Add(EntityStat.MagicDamageDealtIncrease, 10);

    return modifiers;
}
```

Equipment is refreshed after segment and facet changes, so the old segment snapshot is removed and the new one is applied.

## Bonus Depending on Other Equipment

Calculate set bonuses from the wearer's current equipment:

```csharp
protected override StatModifierSet GetStatModifiers(MobileEntity wearer)
{
    var modifiers = base.GetStatModifiers(wearer);

    if (wearer is PlayerEntity player &&
        player.Paperdoll.Helmet is ValorHelm &&
        player.Paperdoll.Gauntlets is HonorGaunts)
    {
        modifiers.Add(EntityStat.MaxHealth, 25);
    }

    return modifiers;
}
```

Paperdoll, ring, and hand transactions refresh equipped sources after the transaction. Do not activate, inactivate, or directly modify one item from another item.

## Eligibility Checks

See [Item Use Validation](ItemUseValidation.md) for the full public API, interaction examples, and pitfalls. This section focuses on how use eligibility affects continuous equipment stats.

Use `ValidateUse` when the same eligibility rule controls both item use and stat modifiers. Return the reason instead of sending a message inside validation:

```csharp
public override ItemUseResult ValidateUse(MobileEntity entity)
{
    var result = base.ValidateUse(entity);

    if (!result.IsAllowed)
        return result;

    return entity.Alignment == Alignment.Lawful
        ? ItemUseResult.Allowed
        : ItemUseResult.Denied("Only lawful characters may use this item.");
}

protected override StatModifierSet GetStatModifiers(MobileEntity wearer)
{
    var modifiers = base.GetStatModifiers(wearer);

    if (CanApplyStatModifiers(wearer))
        modifiers.Add(EntityStat.Strength, 3);

    return modifiers;
}
```

`CanUse` is a side-effect-free boolean wrapper around `ValidateUse`; do not override it. The default `CanApplyStatModifiers` uses this same validation, so shared profession, level, alignment, unlock, binding, and ownership requirements belong in `ValidateUse`. Override `CanApplyStatModifiers` only when modifier eligibility is intentionally different.

Keep this check free of messages, timers, random rolls, subscriptions, and other side effects because it may run often during refreshes.

## Lifecycle Side Effects

`OnActivateBonus` and `OnInactivateBonus` still have a purpose. Use them for behavior that happens once when the item becomes active or inactive, such as adding an item source to a status effect.

Likewise, `OnWield` and `OnUnwield` remain appropriate for wield messages, cooldowns, event subscriptions, and other actual wield events.

Do not also modify an `EntityStat` in those hooks if it is returned by `GetStatModifiers`.

```csharp
protected override StatModifierSet GetStatModifiers(MobileEntity wearer)
{
    var modifiers = base.GetStatModifiers(wearer);
    modifiers.Add(EntityStat.Strength, StrengthBonus);
    return modifiers;
}

protected override void OnActivateBonus(MobileEntity entity)
{
    base.OnActivateBonus(entity);
    AddStatusSource(entity);
}

protected override void OnInactivateBonus(MobileEntity entity)
{
    RemoveStatusSource(entity);
    base.OnInactivateBonus(entity);
}
```

## Buffs and Debuffs

A `SpellStatus` supplies one complete snapshot for all of its spell and item sources. This example applies Strength once while at least one spell source remains:

```csharp
protected override StatModifierSet GetStatModifiers(MobileEntity target)
{
    var modifiers = base.GetStatModifiers(target);

    if (Spells.Count > 0)
        modifiers.Add(EntityStat.Strength, 6);

    return modifiers;
}
```

Multiple casters do not automatically multiply the modifier. The status decides whether sources are fixed, additive, or strongest-wins when it builds the snapshot.

A debuff uses negative values:

```csharp
protected override StatModifierSet GetStatModifiers(MobileEntity target)
{
    var modifiers = base.GetStatModifiers(target);
    modifiers.Add(EntityStat.Strength, -6);
    modifiers.Add(EntityStat.HealthRegenerationRate, -2);
    return modifiers;
}
```

When a source is added, refreshed, or removed, the status replaces its previous aggregate snapshot. When the status expires, the exact stored snapshot is removed. Do not recalculate an amount to subtract in `OnRemoved`.

Keep messages, sounds, timers, and other lifecycle behavior in `OnAcquire`, `OnRemoved`, `OnSourceAdded`, and `OnSourceRemoved`. Those hooks must not directly mutate stats represented by the snapshot.

## Dynamic Combat Properties

Not every calculated equipment value belongs in a snapshot. Properties read only when combat occurs can remain ordinary calculated properties, for example:

- minimum and maximum damage;
- swing speed;
- proc chance;
- armor protection and blocking calculations;
- tooltip-only values.

Refresh a stat snapshot only when the item contributes to an `EntityStat` or an attribute maximum. Dynamic properties may still require an item delta so the tooltip is updated.

## BaseDodge Compatibility

`StatModifierSet.BaseDodge` is a temporary compatibility path because dodge is not yet represented by an `EntityStat`. Use it only for an equipped item's continuous dodge contribution:

```csharp
protected override StatModifierSet GetStatModifiers(MobileEntity wearer)
{
    var modifiers = base.GetStatModifiers(wearer);
    modifiers.BaseDodge += 1;
    return modifiers;
}
```

Do not add new direct `wearer.BaseDodge += value` and `-= value` pairs for equipment. When Dodge becomes an `EntityStat`, this special snapshot field will be replaced by a normal `Add` call.

## Pitfalls to Avoid

### Direct add/remove pairs

Do not write equipment stats like this:

```csharp
protected override void OnActivateBonus(MobileEntity entity) =>
    entity.Stats[EntityStat.MaxHealth].Add(Bonus, ModifierType.Constant);

protected override void OnInactivateBonus(MobileEntity entity) =>
    entity.Stats[EntityStat.MaxHealth].Remove(Bonus, ModifierType.Constant);
```

If `Bonus` changes between activation and inactivation, the wrong value is removed. Return it from `GetStatModifiers` instead.

### Replaying activation to refresh stats

Do not call `InactivateBonus` followed by `ActivateBonus` when a property changes. That replays unrelated lifecycle behavior. Call `UpdateStatModifiers()`.

### Depending on activation-hook setup

The initial snapshot is registered before `OnActivateBonus` runs, and it is removed before `OnInactivateBonus` runs. Do not make `GetStatModifiers` depend on state initialized or cleared by those hooks. Store required item state before equip or refresh it explicitly after that state changes.

### Omitting the base snapshot

Failing to call `base.GetStatModifiers(wearer)` silently drops modifiers declared by base equipment classes.

### Side effects during calculation

`GetStatModifiers` can run because of unrelated equipment or wearer changes. It must be deterministic and safe to call repeatedly. Do not send messages, roll randomness, create timers, subscribe to events, or mutate other items from it.

The same rule applies to `SpellStatus.GetStatModifiers`. Read authoritative status sources and wearer state rather than another source's already-calculated stat value, which would make refresh results order dependent.

### Returning only the value that changed

Every call must return the item's complete snapshot. `Replace` removes the entire old snapshot before applying the new one.

### Forgetting a refresh trigger

The system cannot detect arbitrary custom dependencies. A custom item property must call `UpdateStatModifiers`; a new wearer dependency must call `MobileEntity.UpdateStatModifiers` from its change path.

### Recalculating during removal

Do not compute what should be subtracted during unequip. The collection already retains and removes the exact registered snapshot.
