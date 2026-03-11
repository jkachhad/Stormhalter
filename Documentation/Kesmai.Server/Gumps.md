# Stormhalter Gumps

This guide explains how gumps work, when to use each pattern, and how to add new ones safely.

## Overview

- Gumps are built on the server as a control tree made from `Gump`, `GumpControl`, and subclasses.
- The server compiles the control tree into layout data and sends it to the client.
- The client renders the layout, captures user input, and sends response packets back.
- The server resolves the response to the active gump instance by serial.

## What a gump is

A gump is a server-driven UI window. The server builds the UI, sends it to the client, and receives a response back when the player interacts with it.

Typical uses include:

- Spell prompts
- Confirmation dialogs
- Read-only windows such as books, scrolls, or character information
- Data-driven modal windows backed by templates

## End-to-end flow

1. The server creates a `Gump` or `LocalizedGump`.
2. The server sends it with `SendGump(...)`.
3. The client shows the window.
4. The player clicks a button, types text, changes a dropdown, or closes the window.
5. The client sends a response or close packet.
6. The server dispatches that event back to the active gump instance.

Key behavior:

- The UI is server authoritative.
- Client interaction does not automatically decide the final lifecycle.
- The server usually chooses whether to close the gump, keep it open, or replace it with another one.

## Core types

### `Gump`

`Gump` is the base server-side window type.

It usually provides:

- A unique serial so the response can be matched to the correct window
- Window behavior such as `Overlay`, `CanDrag`, `CanResize`, `CanClose`, and `IsModal`
- Metadata such as `Title`, `Wiki`, and `OnEnterKey`
- A control tree made from child controls

In practice, use `Gump` when you want to build the UI directly in C#.

### `GumpControl`

`GumpControl` is the base type for controls inside a gump.

Examples of controls commonly used in the gump system:

- `StackPanel`
- `Grid`
- `Canvas`
- `ScrollViewer`
- `TextBlock`
- `TextBox`
- `Button`
- `TextureButton`
- `CheckBox`
- `DropDownButton`
- `Image`
- `TabControl`

These controls usually compile into layout XML that the client understands.

### `LocalizedGump`

`LocalizedGump` is a specialized gump for template-based UI.

Use it when:

- The general layout already exists as a client template
- The server mostly needs to send data and bind response actions
- You want a clean separation between layout and gameplay logic

This is the pattern used by Stormhalter examples such as `LocateSpellGump` and `PeekSpellGump`.

### `GumpResponseArgs`

`GumpResponseArgs` is the server-side response object passed into handlers.

It typically contains:

- `Name`: the name of the control or action that triggered the response
- `Texts`: text input values by control name
- `DropDowns`: selected dropdown indexes by control name
- `Client`: the client that sent the response

## Packet pipeline

The packet flow looks like this.

### Server to client

- `GumpShowPacket`: sends a fully compiled gump layout
- `LocalizedGumpShowPacket`: sends a layout shell, template name, and model data
- `GumpUpdateLayoutPacket`: replaces a named control subtree
- `GumpUpdatePropertyPacket`: changes one property on one named control
- `GumpClosePacket`: closes an active gump from the server side

### Client to server

- `ClientGumpResponse`: includes gump serial, action name, text input values, and dropdown selections
- `ClientGumpClose`: includes the serial of the gump that was closed

## Two main patterns

### Pattern 1: Build the whole UI in C#

Use a plain `Gump` when the window structure is easier to express directly in code.

Stormhalter examples:

- `BookGump` in [Book.cs](/root/stormsmith/Stormhalter/Source/Kesmai.Server/Game/Items/Books/Book.cs#L196)
- `ScrollGump` in [Book.cs](/root/stormsmith/Stormhalter/Source/Kesmai.Server/Game/Items/Books/Book.cs#L267)

This pattern is a good fit when:

- The content is assembled from server objects
- You want full control over the control tree
- The UI is mostly read-only or straightforward

### Pattern 2: Use a template plus data

Use `LocalizedGump` when the client template already exists and the server only needs to provide values and handle named actions.

Stormhalter examples:

- `LocateSpellGump` in [InstantLocateSpell.cs](/root/stormsmith/Stormhalter/Source/Kesmai.Server/Spells/Knight/InstantLocateSpell.cs#L164)
- `PeekSpellGump` in [PeekSpell.cs](/root/stormsmith/Stormhalter/Source/Kesmai.Server/Spells/Wizard/PeekSpell.cs#L123)

This pattern is a good fit when:

- The window is modal or form-like
- The user submits text or presses Enter
- You want to keep C# focused on behavior, not layout

## Sending and closing gumps

The standard pattern is:

```csharp
source.CloseGumps<MyGump>();
source.SendGump(new MyGump(...));
```

Why this is useful:

- It prevents duplicate copies of the same window
- It avoids stale responses from an older gump instance
- It keeps the UI state easy to reason about

Stormhalter already does this for:

- `LocateSpellGump`
- `PeekSpellGump`
- `BookGump`
- `ScrollGump`

## Building a plain `Gump`

Here is the basic shape:

```csharp
public class ExampleGump : Gump
{
    public ExampleGump()
    {
        Title = "Example";
        Overlay = true;
        CanDrag = true;
        Width = 420;
        Height = 240;

        var root = new StackPanel
        {
            Margin = new Rectangle(8, 8, 8, 8),
        };

        root.Children.Add(new TextBlock
        {
            Text = "Enter a value:",
        });

        root.Children.Add(new TextBox
        {
            Name = "valueInput",
        });

        root.Children.Add(new Button
        {
            Name = "save",
            Content = new TextBlock { Text = "Save" },
        });

        Children.Add(root);
    }
}
```

Useful rules:

- Give every interactive control a stable `Name`
- Use simple, descriptive names such as `save`, `cancel`, `valueInput`
- Keep the construction order easy to follow
- Reuse style names instead of hardcoding every visual detail when styles already exist

## Handling responses in a plain `Gump`

There are two common approaches.

### Option A: Override `OnResponse`

```csharp
protected override void OnResponse(Client source, GumpResponseArgs args)
{
    switch (args.Name)
    {
        case "save":
        {
            if (args.Texts.TryGetValue("valueInput", out var value))
            {
                // Process value.
            }

            source.CloseGump(this);
            break;
        }
        case "cancel":
        {
            source.CloseGump(this);
            break;
        }
    }
}
```

This is useful when:

- The gump has several possible actions
- You want all behavior centralized in one method

### Option B: Attach button actions and call the base handler

Buttons can have delegates attached, and `base.OnResponse(...)` dispatches to the matching button by name.

```csharp
protected override void OnResponse(Client source, GumpResponseArgs args)
{
    base.OnResponse(source, args);
}
```

Important:

- If you override `OnResponse` and do not call `base.OnResponse(source, args)`, those default button handlers will not run unless you dispatch them yourself.

## Building a `LocalizedGump`

This pattern is smaller because the client owns most of the layout.

Stormhalter’s `LocateSpellGump` is the clearest example:

```csharp
public class LocateSpellGump : LocalizedGump
{
    public override string Template => "Server-Locate";

    public LocateSpellGump(MobileEntity caster, InstantLocateSpell spell)
    {
        Style = "Client-GameContent-Modal";
        Overlay = true;
        OnEnterKey = "locate";

        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;

        SetResponseAction("locate", Locate);
    }
}
```

Reference: [InstantLocateSpell.cs](/root/stormsmith/Stormhalter/Source/Kesmai.Server/Spells/Knight/InstantLocateSpell.cs#L164)

What each part does:

- `Template` chooses the client-side UI template
- `Style` applies a shared frame or visual treatment
- `Overlay` makes it behave like a modal overlay
- `OnEnterKey` maps the keyboard Enter key to a named action
- `SetResponseAction(...)` binds that action to server logic

## Returning template data

A `LocalizedGump` usually overrides `GetData()` and returns an anonymous object.

Example:

```csharp
public override dynamic GetData()
{
    return new
    {
        Source = (_spell.Item != default) ? "item" : "spell",
        SourceId = (_spell.Item != default) ? _spell.Item.ItemId : _spell.SpellId
    };
}
```

This data becomes the model for the template. It is a good pattern when one template needs small variations based on the context.

## Handling responses in a `LocalizedGump`

Stormhalter uses named response actions.

Example:

```csharp
private void Locate(GumpResponseArgs args)
{
    var client = args.Client;

    if (args.Texts.TryGetValue("locateQuery", out var query))
    {
        _spell.Query(query);

        if (_spell.Item is ICharged charged)
            charged.ChargesCurrent--;
    }

    client.CloseGump<LocateSpellGump>();
}
```

Reference: [InstantLocateSpell.cs](/root/stormsmith/Stormhalter/Source/Kesmai.Server/Spells/Knight/InstantLocateSpell.cs#L195)

This shows the normal pattern:

- Read text input from `args.Texts`
- Execute gameplay logic
- Close the gump when the action is complete

`PeekSpellGump` follows the same shape with `"peek"` and `"peekQuery"`.

## Handling close events

Sometimes closing the UI should also cancel game logic.

`LocateSpellGump` demonstrates that:

```csharp
protected override void OnClose(Client source)
{
    base.OnClose(source);

    if (_caster.Spell != _spell)
        return;

    _spell.Cancel();
}
```

Reference: [InstantLocateSpell.cs](/root/stormsmith/Stormhalter/Source/Kesmai.Server/Spells/Knight/InstantLocateSpell.cs#L209)

Use this pattern when the UI is tied to:

- An active spell cast
- A delayed action
- A multi-step workflow that should not remain live after the window closes

## Dynamic updates without reopening

Assuming Stormhalter matches `Kesmai.Server`, gumps can also be updated in place instead of being fully reopened.

Typical update patterns are:

### Update a single property

```csharp
source.UpdateGumpControlProperty(this, "save", nameof(Button.IsEnabled), "false");
```

### Replace a named control subtree

```csharp
source.UpdateGumpControl(this, new TextBlock
{
    Name = "statusMessage",
    Text = "Saved successfully.",
});
```

This is useful when:

- You want to disable a button after submission
- You want to show validation or status text
- You want to refresh only part of the layout

Rules to follow:

- The target control must already exist and have a stable `Name`
- Use `nameof(...)` for property names when possible
- Keep replacement controls compatible with the original layout
- If the name does not match an active control, the update is ignored

## Recommended naming habits

Use names that explain intent, not implementation details.

Good examples:

- `save`
- `cancel`
- `search`
- `statusMessage`
- `nameInput`
- `qualityDropDown`

Avoid vague names like:

- `button1`
- `textboxA`
- `thing`

Stable names matter because responses and dynamic updates depend on them.

## Practical design advice

When creating a new gump:

- Decide first whether it should be a plain `Gump` or a `LocalizedGump`
- Close old copies before opening a new one
- Name all interactive controls clearly
- Set `OnEnterKey` for dialog-style prompts
- Keep response handling small and explicit
- Close the gump when the task is complete unless persistence is intentional
- If closing the window should cancel gameplay state, override `OnClose`
- Prefer in-place updates for small feedback changes if the shared gump API is available

## Agent checklist

- Choose `Gump` versus `LocalizedGump` before writing the UI
- Give every interactive control a stable `Name`
- Validate input read from `args.Texts` and `args.DropDowns`
- Decide close behavior explicitly
- Use `OnEnterKey` for prompt-style dialogs
- Prefer targeted updates over full rebuilds when that API is available
- Keep response names deterministic and easy to trace
- Make sure delayed or async callbacks still target the correct client and active gump

## Common pitfalls

- Missing `Name` on an interactive control, so the server cannot route the response cleanly
- Assuming a text box value always exists, even though empty text inputs may be omitted
- Forgetting `base.OnResponse(...)` when relying on button delegate dispatch
- Reopening a gump unnecessarily when a targeted update would be clearer
- Closing the client window visually without cleaning up server-side game state
- Replacing a control with a new subtree that does not reuse the correct `Name`

## Suggested starter examples

### Small prompt window

```csharp
public class RenameItemGump : Gump
{
    public RenameItemGump()
    {
        Title = "Rename Item";
        Overlay = true;
        CanDrag = true;
        Width = 420;
        Height = 240;

        var root = new StackPanel
        {
            Margin = new Rectangle(8, 8, 8, 8),
        };

        root.Children.Add(new TextBlock
        {
            Text = "Enter a new name:",
        });

        root.Children.Add(new TextBox
        {
            Name = "nameInput",
        });

        root.Children.Add(new Button
        {
            Name = "save",
            Content = new TextBlock { Text = "Save" },
        });

        Children.Add(root);
    }
}
```

### Template-based modal window

```csharp
public class ExampleLocalizedGump : LocalizedGump
{
    public override string Template => "Server-Example";

    public ExampleLocalizedGump()
    {
        Style = "Client-GameContent-Modal";
        Overlay = true;
        OnEnterKey = "confirm";

        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;

        SetResponseAction("confirm", Confirm);
    }

    public override dynamic GetData()
    {
        return new
        {
            Title = "Example"
        };
    }

    private void Confirm(GumpResponseArgs args)
    {
        args.Client.CloseGump<ExampleLocalizedGump>();
    }
}
```

## Real Stormhalter references

If you want working examples from this repo, start here:

- [InstantLocateSpell.cs](/root/stormsmith/Stormhalter/Source/Kesmai.Server/Spells/Knight/InstantLocateSpell.cs)
- [PeekSpell.cs](/root/stormsmith/Stormhalter/Source/Kesmai.Server/Spells/Wizard/PeekSpell.cs)
- [Book.cs](/root/stormsmith/Stormhalter/Source/Kesmai.Server/Game/Items/Books/Book.cs)

Those files show:

- Opening and closing gumps
- Template-based dialogs
- Reading text responses
- Building content-oriented windows in C#
- Cancelling spell state when a gump closes
