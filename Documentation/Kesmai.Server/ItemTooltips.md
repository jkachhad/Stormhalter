# Item Values on Tooltips

This guide explains how the numbers on an item—damage, armor, protections, hindrance, and so on—reach the player's tooltip, and what you need to do (usually nothing) when you write a new item.

## The Short Version

- Override the usual members (`MinimumDamage`, `Hindrance`, `ProtectionFromFire`, …) exactly as you always have. The tooltip picks them up automatically.
- If one of those values can change while the game is running, call `InvalidateProperties()` right after it changes.
- You don't need to override `GetClientProperties` for any value listed below.

## Values That Show Automatically

The shared item classes send these values for you. Anything that inherits from them—including items in segment repositories—gets the same treatment.

| If your item inherits from… | These values are sent |
| --- | --- |
| `Equipment` (armor, robes, helmets, boots, rings, amulets, gauntlets, …) | `Hindrance`, `RestrictSpellcast`, `ProtectionFromFire`, `ProtectionFromIce`, `ProtectionFromDaze`, `ProtectionFromConcussion`, `HealthRegeneration`, `StaminaRegeneration`, `ManaRegeneration` |
| `Armor` or `Shield` | `BaseArmorBonus`, `SlashingProtection`, `PiercingProtection`, `BashingProtection`, `ProjectileProtection` |
| `Weapon` or `Gauntlets` | Everything in the `Armor` row, plus `Skill`, `MinimumDamage`, `MaximumDamage`, `BaseAttackBonus`, `Flags`, `Penetration`, and `MaxRange` |
| `Weapon` | `HealthRegeneration`, `StaminaRegeneration`, `ManaRegeneration` |

A value of zero (or `false`, or `None`) is simply left off the tooltip, so a robe with no ice protection won't show "Ice protection: 0".

## Example: A New Robe

This robe shows a hindrance of 1 (inherited from `Robe`), fire protection 10, and mana regeneration 2. Nothing else is needed for the tooltip:

```csharp
public class EmberRobe : Robe, ITreasure
{
    public override int ProtectionFromFire => 10;
    public override int ManaRegeneration => 2;

    public EmberRobe() : base(261)
    {
    }

    public EmberRobe(Serial serial) : base(serial)
    {
    }
}
```

The same overrides also give the wearer the actual fire protection and mana regeneration in game. The tooltip and the gameplay effect always come from the same member, so they can't disagree.

## Example: A New Weapon

```csharp
public class StormBlade : Sword, ITreasure
{
    public override int MinimumDamage => 2;
    public override int MaximumDamage => 10;
    public override int BaseAttackBonus => 3;
    public override WeaponFlags Flags => WeaponFlags.Slashing | WeaponFlags.BlueGlowing;
    public override ShieldPenetration Penetration => ShieldPenetration.Heavy;

    public StormBlade() : base(1)
    {
    }

    public StormBlade(Serial serial) : base(serial)
    {
    }
}
```

The tooltip shows the sword skill (inherited from `Sword`), damage 2–10, attack bonus 3, the slashing and blue-glowing flags, and heavy shield penetration.

## When a Value Changes During Play

If a value is fixed (`=> 10`), you're done. If it's calculated from something that changes while the server is running, the tooltip needs a nudge so it refreshes straight away. Call `InvalidateProperties()` right after the change:

```csharp
public class HungryBlade : Sword, ITreasure
{
    private int _kills;

    public override int MaximumDamage => 6 + _kills / 100;

    public void RecordKill()
    {
        _kills++;

        // the maximum damage may have changed, so refresh the tooltip.
        InvalidateProperties();
    }

    // constructors and serialization omitted
}
```

Without that call, players keep seeing the old number until the item is sent to them again, for example when they log in, open their locker, or look at the tile it's on.

You don't need to call it in these cases, because they're already handled:

- **Quality.** Values calculated from `Quality` refresh automatically whenever the quality changes.
- **The wearer.** Values that depend on the wearer's stats or hands refresh whenever the wearer's stats are recalculated.
- **Identification.** Values refresh automatically when an item is identified.

## Unidentified Items

Every item needs to be identified before the values above are sent; until then, the client is only told that the item is unidentified. Once someone identifies it, the values appear without any extra code.

Items bought from a merchant count as identified, so players see their stats right away. Currency is also an exception: it never needs identifying. If you write an item that players should always see in full, override `RequiresIdentification`:

```csharp
public override bool RequiresIdentification => false;
```

Such an item always counts as identified, including for any description text that checks `Identified`.

## Things to Avoid

- **Don't override `GetClientProperties` to send the values above.** They're already sent. Sending them again is redundant and easy to get out of step with the shared rules, such as leaving off zero values or hiding values on unidentified items.
- **Don't add the same bonus twice.** Protection and regeneration from the members above already apply in game. If you also add, say, `EntityStat.FireProtection` in `GetStatModifiers`, the player gets both bonuses and sees both on the tooltip. Only use `GetStatModifiers` for an extra, conditional bonus on top of the base value.
- **Don't use `Delta(ItemDelta.Update)` to refresh a tooltip.** It resends the whole item. Use `InvalidateProperties()` instead.

## Sending Your Own Values

Sometimes an item has a value the shared classes don't know about. If it already has a property ID, you can send it yourself by overriding `GetClientProperties`. Always call `base` first so the standard values stay on the tooltip. [Equipment Stat Modifiers](StatModifiers.md#client-item-properties) covers the full API and value types.

Each new kind of value (charges, weight, and so on) needs a new property ID, and the game client has to be updated to understand it. Talk to the core team before adding one.
