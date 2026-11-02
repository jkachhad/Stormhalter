# Notes for coding agents

This repository is public. Write commits, pull request descriptions, comments and `Documentation/` for human content authors, in plain language with examples.

## What's here
- `Source/Kesmai.Server` holds the game scripts: items, spells, creatures, commands and more. They are compiled into the game server when it is built, so they use .NET 11 and the latest C#.
- `Source/Kesmai.Shared` is compiled into both the server and the game client.
- `Source/Kesmai.WorldForge` is the map editor, which ships with the game client. It links the prebuilt DigitalRune DLLs in `Dependencies/DigitalRune`, which don't update on their own.
- `Content/Localization/Enu.xml` holds the server's messages. A new message shows as `#<index>` in the game client until the next client content release.
- Guides for content authors are in `Documentation/`.

## Working here
- This repository has no test project. Tests for these scripts live with the server.
- A change that also needs server engine work uses the same branch name in each repository, so the engine's tests build against this branch.
- Open pull requests against `master`. Maintainers merge them.

## Conventions
- Commit subjects are plain imperative sentences, with no type prefix and no trailing period. Example: "Prompt the player when Identify asks for a target". WorldForge commits start with "WorldForge: ".
- Files use LF line endings, and some start with a BOM. Keep both as they are, and change only the lines you edit.
- In `Source/Kesmai.Server` and `Source/Kesmai.Shared`:
  - Avoid the null-conditional operators `?.` and `?[]`.
  - Use the `is` operator; `is not` is allowed for type checks.
  - Prefer `var` unless the type isn't obvious from the right-hand side.
  - Use expression-bodied members for single-line methods and properties.
  - Build strings with interpolation (`$"..."`).
- Extend an existing mechanism before adding a parallel one. Keep small fixes inline instead of adding helper methods.
- Say "agent" or "automated player", never "bot".
