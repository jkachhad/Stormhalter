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
| `Armor` or `Shield` | `BaseArmorBonus`, `SlashingProtection`, `PiercingProtection`, `BashingProtection`, `ProjectileProtection`, `MeleeDamageMitigation`, `RangedDamageMitigation`, `ProjectileDamageMitigation` |
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

## Example: Armor With Damage Mitigation

Set melee, ranged, and projectile damage mitigation with the mitigation members, not in `GetStatModifiers`. The tooltip shows them as a row of melee, ranged, and projectile icons, and the wearer gets the same mitigation in game:

```csharp
public class WardenScales : Armor, ITreasure
{
    public override int SlashingProtection => 3;
    public override int ProjectileProtection => 2;

    public override int MeleeDamageMitigation => 4;
    public override int RangedDamageMitigation => 4;
    public override int ProjectileDamageMitigation => 2 + Quality;

    // constructors and serialization omitted
}
```

Only a bonus that depends on the wearer, such as one that grows with their skill or level, belongs in `GetStatModifiers`. It shows as its own mitigation line below the icon row (see [Bonuses From GetStatModifiers](#bonuses-from-getstatmodifiers)).

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

Items bought from a merchant count as identified, and a new character's starting gear is identified, so players see those stats right away. Currency is also an exception: it never needs identifying. If you write an item that players should always see in full, override `RequiresIdentification`:

```csharp
public override bool RequiresIdentification => false;
```

Such an item always counts as identified, including for any description text that checks `Identified`.

## Things to Avoid

- **Don't override `GetClientProperties` to send the values above.** They're already sent. Sending them again is redundant and easy to get out of step with the shared rules, such as leaving off zero values or hiding values on unidentified items.
- **Don't add the same bonus twice.** Protection, regeneration, and damage mitigation from the members above already apply in game. If you also add, say, `EntityStat.FireProtection` in `GetStatModifiers`, the player gets both bonuses and sees both on the tooltip. Only use `GetStatModifiers` for an extra, conditional bonus on top of the base value.
- **Don't use `Delta(ItemDelta.Update)` to refresh a tooltip.** It resends the whole item. Use `InvalidateProperties()` instead.

## Bonuses From GetStatModifiers

Extra bonuses you add in `GetStatModifiers` also show on the tooltip, as their own bulleted lines below the item's stats. For example:

```csharp
protected override StatModifierSet GetStatModifiers(MobileEntity wearer)
{
    var modifiers = base.GetStatModifiers(wearer);

    modifiers.Add(EntityStat.Strength, 3);
    modifiers.Add(EntityStat.MagicDamageDealtIncrease, 10);

    return modifiers;
}
```

That shows "+3 Strength" and "+10% Magic Damage Dealt". Some things to know:

- Several bonuses to the same stat are added together into one line.
- Percentage stats, such as magic damage dealt and critical strike chance, show a `%`.
- A bonus that hurts the player, such as a negative value or magic damage dealt reduction, shows in red.
- Like the other values, bonuses only appear once the item is identified.
- The tooltip shows the common stats: mitigation (for wearer-dependent bonuses; the item's own mitigation uses the members above), magic damage and critical strike, max health/mana/stamina, regeneration, Barrier, Strength, Dexterity, protections, Lightning Resistance, Spell Focus, and the fire and ice protection limits from `AddMaximumValue`. A bonus to any other stat still works in game but doesn't appear yet. Ask the core team if you need one added.

A bonus here is separate from the matching item member. A robe with `ManaRegeneration => 1` and an extra +2 in `GetStatModifiers` shows two regeneration lines.

## Adding Your Own Tooltip Lines

For anything that isn't a stat, such as a special power, a recharge timer, or a set bonus, add your own line by overriding `GetTooltipEntries`. Always call `base` first, because it adds the standard lines:

- the stored spell on wands and staves ("Spell: Fireball");
- charges ("Charges: 2 / 5");
- Enchanted and Conjured;
- Owner or Unbound for bindable items.

```csharp
public override void GetTooltipEntries(PlayerEntity beholder, Tooltip tooltip)
{
    base.GetTooltipEntries(beholder, tooltip);

    // 6600100 is an entry you add, for example "Power: {0}".
    tooltip.AddLocalizedText(6600100, Color.Cyan, TooltipTextStyle.Passive, 0, Power.ToString());
}
```

Things to know:

- Put the wording in the Tooltips section of `Content/Localization/Enu.xml`, using a free number in the 66xxxxx range, and pass only the changing values as arguments.
- Pass numbers you want shown as text (`Power.ToString()`). A plain number argument is read as a reference to another localization entry, which is how the stored spell's name is shown.
- `beholder` is the player looking at the item. You can use it to show different lines to different players, but don't change the item or send messages while building the tooltip.
- The last argument before your values is the line's order: higher numbers appear first, and lines with the same number keep the order you add them in.
- The style sets how the line looks: each line gets a small bullet and a default color for its style.

  | Style | Use it for | Default color |
  | --- | --- | --- |
  | `Normal` | plain information | white |
  | `Active` | something the player can use or trigger | green |
  | `Passive` | an always-on effect | orange |
  | `Harmful` | a drawback or curse | red |
  | `Disabled` | an effect that is currently unavailable | gray |
  | `Consume` | an effect used up on use (looks like `Active`) | green |

  Pass `Color.White` to use the style's color, or any other color to override it, as the example does with `Color.Cyan`.
- The lines only appear once the item is identified.

Players' clients keep the tooltip until something changes. When a value your lines use changes during play, call `InvalidateTooltip()` so it refreshes:

```csharp
public int Power
{
    get => _power;
    set
    {
        if (_power == value)
            return;

        _power = value;

        // the tooltip shows the power.
        InvalidateTooltip();
    }
}
```

Charges, Enchanted, Conjured and the owner already refresh the tooltip when they change. When you change charges, use the `ChargesCurrent` property, not the field behind it.

## Sending Your Own Values

Sometimes an item has a value the shared classes don't know about. If it already has a property ID, you can send it yourself by overriding `GetClientProperties`. Always call `base` first so the standard values stay on the tooltip. [Equipment Stat Modifiers](StatModifiers.md#client-item-properties) covers the full API and value types.

Each new kind of value (weight, for example) needs a new property ID, and the game client has to be updated to understand it. Talk to the core team before adding one.
