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
| `ItemEntity` (every item) | `Weight`, even before the item is identified (the tooltip shows the whole stack's weight); `BasePrice` as the item's value once identified; thieves see the actual price (`BasePrice` plus `Price`) instead |
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

The tooltip shows damage 2–10 with the slashing damage icon, attack bonus 3, and heavy shield penetration. The sword skill (inherited from `Sword`) and the blue glow appear as badges on the tooltip's right border. If `Flags` includes `Lawful`, `Chaotic`, or `Neutral`, the tooltip shows that alignment in its color (cyan, yellow, or lime) after the other standard lines.

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

Every item needs to be identified before the values above are sent; until then, the client is only told that the item is unidentified and how much it weighs. Once someone identifies it, the values appear without any extra code.

Items bought from a merchant count as identified, and a new character's starting gear is identified, so players see those stats right away. Currency is also an exception: it never needs identifying. If you write an item that players should always see in full, override `RequiresIdentification`:

```csharp
public override bool RequiresIdentification => false;
```

Such an item always counts as identified, including for any description text that checks `Identified`.

## Things to Avoid

- **Don't override `GetClientProperties` to send the values above.** They're already sent. Sending them again is redundant and easy to get out of step with the shared rules, such as leaving off zero values or hiding values on unidentified items.
- **Exception: a value that changes during combat.** If a member such as `MinimumDamage` adds a short-lived bonus (a proc or a charge-up), override `GetClientProperties`, call `base` first, and set the stable value, so the tooltip doesn't change every swing. Remove the property when the stable value is zero, to match the shared rules.
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
- A bonus that doesn't apply yet, for example one that needs a higher level or another profession, can still be shown. Leave it out to hide it, or add it with `StatModifierFlags.Inactive` to list it in gray. An inactive bonus is never given to the player:

  ```csharp
  var isKnight = wearer is PlayerEntity { Profession.Info.Name: "Knight" };

  // shown in blue to a knight, and in gray to everyone else.
  modifiers.Add(EntityStat.Barrier, 2, flags: isKnight ? StatModifierFlags.None : StatModifierFlags.Inactive);
  ```
- The tooltip shows the common stats: mitigation (for wearer-dependent bonuses; the item's own mitigation uses the members above), magic damage and critical strike, max health/mana/stamina, regeneration, Barrier, Strength, Dexterity, protections, Lightning Resistance, Spell Focus, Spell Clearcast, Mana Recovery Delay, and the fire and ice protection limits from `AddMaximumValue`. A bonus to any other stat still works in game but doesn't appear yet. Ask the core team if you need one added.

A bonus here is separate from the matching item member. A robe with `ManaRegeneration => 1` and an extra +2 in `GetStatModifiers` shows two regeneration lines.

## The Description on the Tooltip

The tooltip shows a short description of the item: just the item text, such as "an iron shortsword.". It leaves out "You are looking at" and the lines about the item's properties, such as "The combat adds for this weapon are +3.", because the tooltip already shows those values. It leaves out quality too, because the tooltip shows that on its own.

Write it in `GetTooltipDescription`, next to `GetDescription`. `GetDescription` is still what players see when they look at the item. Keep the two separate: `GetTooltipDescription` must not call `GetDescription`.

```csharp
public override void GetDescription(List<LocalizationEntry> entries)
{
    entries.Add(new LocalizationEntry(6200000, 6200024)); /* [You are looking at] [an iron shortsword.] */

    if (Identified)
        entries.Add(new LocalizationEntry(6250020)); /* The shortsword appears quite ordinary. */
}

public override void GetTooltipDescription(List<LocalizationEntry> entries)
{
    entries.Add(new LocalizationEntry(6200024)); /* an iron shortsword. */
}
```

Always use a localization entry from `Content/Localization/Enu.xml`, never text written in code such as `new LocalizationEntry("a ring that emits an aura of magic.")`. If the item text only exists in code, add an entry for it without "You are looking at". An entry can take values, such as a name:

```xml
<entry index="6200397">the body of {0}.</entry>
```

```csharp
entries.Add(new LocalizationEntry(6200397, creature.Name.WithArticle())); /* the body of {0}. */
```

Some things to know:

- An item without `GetTooltipDescription` uses its parent class's text. If neither has one, the tooltip shows no description.
- If the text depends on the item's state, such as a stack amount or whether it has been repaired, use the same conditions as in `GetDescription`.
- Pass numbers you want shown as text, such as `Level.ToString()`. A plain number argument is read as a reference to another localization entry, so `new LocalizationEntry(130900110, 130900050, ...)` puts entry 130900050's text in place of `{0}`.
- A line that the tooltip doesn't show anywhere else yet, such as a pet's level, can go in `GetTooltipDescription` for now, as a localization entry too. Values like these will move to their own tooltip lines later.
- Text that has a property inside the same sentence, such as "a black broadsword. The weapon is lawful.", is shown as written.

## Numbers for a Segment's Text

Each segment has its own block of 1,000,000 numbers in `Enu.xml`, so text added for one segment can't collide with another segment or with the shared entries. A block starts at 100,000,000 plus the segment's ID times 1,000,000. The segment's ID is the `index` it has in its facet. The first entry in each block is the segment's name.

| ID | Segment | Block |
|---|---|---|
| 1 | Kesmai | 101000000–101999999 |
| 2 | Leng | 102000000–102999999 |
| 3 | Axe Glacier | 103000000–103999999 |
| 4 | Oakvael | 104000000–104999999 |
| 5 | Underkingdom | 105000000–105999999 |
| 11 | Annwn | 111000000–111999999 |
| 15 | Mu | 115000000–115999999 |
| 26 | Bloodlands | 126000000–126999999 |
| 28 | Razalas Peak | 128000000–128999999 |
| 29 | Blackburrow | 129000000–129999999 |
| 30 | Keshma | 130000000–130999999 |

Each block has the same sections. Here they are for Keshma:

| Numbers | Section |
|---|---|
| 130000000 | The segment's name |
| 130000001–130099999 | Item names |
| 130100000–130199999 | Item text, as players see it when they look at the item |
| 130200000–130299999 | Speech and quests |
| 130300000–130399999 | Messages |
| 130400000–130899999 | Reserved for later |
| 130900000–130999999 | Tooltip descriptions |

Add a new entry under its section's comment in `Enu.xml`, using the next free number in that section:

```xml
  <!-- Keshma: Tooltip Descriptions (130900000-130999999) -->
  <entry index="130900000">a jagged Keshmian blade.</entry>
```

```csharp
public override void GetTooltipDescription(List<LocalizationEntry> entries)
{
    entries.Add(new LocalizationEntry(130900000)); /* a jagged Keshmian blade. */
}
```

Some things to know:

- Write the number directly, as above. Don't work it out from the item's segment while the game runs, because an item that is inside a container may not have a segment.
- Entries that segments added before these blocks existed keep their numbers.
- A new segment gets its block when it is given an ID. Add its section comments to `Enu.xml` in the same layout.

## Adding Your Own Tooltip Lines

For anything that isn't a stat, such as a special power, a recharge timer, or a set bonus, add your own line by overriding `GetTooltipEntries`. Always call `base` first, because it adds the standard lines:

- the stored spell on wands and staves ("Spell: Fireball");
- charges ("Charges: 2 / 5");
- Enchanted and Conjured;
- the item values that aren't shown as icons or badges: hindrance ("Hindrance: 2"), regeneration ("+1 Mana Regeneration"), and "Restricts spellcasting";
- the alignment of a lawful, chaotic, or neutral weapon, last.

The weapon skill and the owner of a bindable item have no lines; they appear only as badges (see [Badges on the Tooltip's Border](#badges-on-the-tooltips-border)).

```csharp
public override void GetTooltipEntries(TooltipContext context, Tooltip tooltip)
{
    base.GetTooltipEntries(context, tooltip);

    // 6600100 is an entry you add, for example "Power: {0}".
    tooltip.AddLocalizedText(6600100, Color.Cyan, TooltipTextStyle.Passive, 0, Power.ToString());
}
```

Things to know:

- Put the wording in the Tooltips section of `Content/Localization/Enu.xml`, using a free number in the 66xxxxx range, and pass only the changing values as arguments.
- Pass numbers you want shown as text (`Power.ToString()`). A plain number argument is read as a reference to another localization entry, which is how the stored spell's name is shown.
- `context.Observer` is the player looking at the item, and `context.Wearer` is the player or creature carrying it. When someone looks at another player's equipment, the tooltip is calculated for the wearer and shown to the observer, with an "Equipped by" line naming the wearer. For an item nobody carries, the wearer is the observer. Use `Wearer` for what the item does for whoever holds it, and `Observer` for what the viewer is allowed to know. Don't change the item or send messages while building the tooltip.
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

## Badges on the Tooltip's Border

A badge is a small icon on the tooltip's left or right border. Players see the icons at a glance and hold Shift to read a label next to each one. Some come with every item:

- on the left, two chained links for the owner of a bindable item, labeled with the owner's name or "Unbound";
- on the right, the weapon's skill, and below it one badge for each of these weapon flags: two-handed, silver, blue glowing, throwable, returning, must throw, quick throw, snake staff, and the alignment (lawful, chaotic, or neutral). The weapon type and damage type flags get no badge, because the skill badge and the damage icon already show them. These come from the weapon's `Skill` and `Flags` automatically, so you don't add them yourself.

Add your own with `AddBadge`. A badge has a side, an icon, an optional color, and a label:

```csharp
public override void GetTooltipEntries(TooltipContext context, Tooltip tooltip)
{
    base.GetTooltipEntries(context, tooltip);

    // 6600101 is an entry you add, for example "Glows amber".
    tooltip.AddBadge(TooltipBadgeSide.Right, TooltipBadgeIcons.Glow, Color.Orange, 6600101);
}
```

Things to know:

- The label follows the same rules as tooltip lines: a localization entry, with numbers passed as text.
- The color tints the icon; the round background behind it keeps its own look. Leave the color out to show the icon in white.
- Badges on each side are listed from the top. Pass an order after the label number to place yours: higher numbers come first. The owner badge uses -30000.
- `TooltipBadgeIcons` has `Owner`, `Skill`, and `Glow` for now; more icons will be added as the art is made.
- Badges only appear once the item is identified, like tooltip lines.

## Sending Your Own Values

Sometimes an item has a value the shared classes don't know about. If it already has a property ID, you can send it yourself by overriding `GetClientProperties`. Always call `base` first so the standard values stay on the tooltip. [Equipment Stat Modifiers](StatModifiers.md#client-item-properties) covers the full API and value types.

Each new kind of value (a recharge time, for example) needs a new property ID, and the game client has to be updated to understand it. Talk to the core team before adding one.
