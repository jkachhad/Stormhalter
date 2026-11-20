# Updates

Stormhalter's patch notes, one file per patch. Players read them at https://www.stormhalter.com/updates/. The page is
rebuilt at every server start, and a patch shows once the server or client it ships is live, so notes merged before a
release stay hidden until the restart that releases it.

Each segment keeps its own notes in its own repository's `Updates/` folder, with a section per segment.

## A patch

Name the file after the patch's full version, for example `Updates/0.119.2.0.md`, and write it in the pull request
that raises the version:

```
---
version: 0.119.2.0
date: 2026-11-20
---
## Client
- The item tooltip settings on the `UI` tab are now together on the `Item Tooltips` line.
  - `Delay` sets how long the mouse rests on an item before its tooltip opens.

## Server
- Fixed an issue where absorbing a Dexterity Ring into a class ring didn't raise your Dexterity until you put the class ring on again.

## Collaborators
- `EntityStat.Dodge` gives dodge from items and statuses. Read `Documentation/Kesmai.Server/StatModifiers.md`.
```

`date` is the day the patch goes live, at the 22:00 Pacific restart.

The page labels each patch from its version; you never write a label:

| Version | Label |
| --- | --- |
| Ends in .0 (`0.119.2.0`) | **Release**: everyone updates |
| A revision with a `Client` or `WorldForge` section | **Optional**: a client revision you can skip |
| A revision with a `Server`, `Staff` or `Collaborators` section | **Hotfix**: a server revision, nothing to download |

## Content between patches

Content that goes live on its own gets a file named after the day and a short title, for example
`Updates/2026-11-02-winter-art.md`, with `title:` instead of `version:`. The page labels it an **Update**.

## Sections

One `## <section>` for each part that changed, in this order, leaving out the rest:

1. `Client`: windows, settings, chat, macros, the updater.
2. `Content`: art, sounds and other game content.
3. `WorldForge`: the map editor, which ships with the Windows client.
4. `Server`: gameplay (menus, NPCs, pets, spells, items, quests) and its fixes.
5. `Staff`: commands only game masters use.
6. `Collaborators`: changes segment authors have to act on, with the type names and the document to read. This is the
   only section that names code.

## Writing the notes

- One change per `- ` bullet, on one line. Put details under it, indented two spaces, at most three levels deep.
- Say what players see or do differently, and name things the way the game shows them.
- Put commands, menu entries, keys and typed text in backticks, for example `` `Sell ▸` ``. Write an example as
  `(i.e …)`.
- Start a fix with "Fixed an issue where …".
- Keep it short. A bullet that needs more than two details is usually two changes.
