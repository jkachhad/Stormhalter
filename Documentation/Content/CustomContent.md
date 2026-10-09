# Overriding Content in the Client

This guide explains how to make your own copy of the game client load art, sounds and definitions from loose files, so you can try new or changed content in the game before it is in a client build. Nothing here changes what other players see; it only affects the client on your computer.

## The Short Version

- Make a folder called `Custom` in the client's install folder (the folder with the client program in it).
- Put your data files in `Custom/Data`, using the names in the table below. They use the same format as the files in `Content/Data`.
- An entry with a new `id` adds an asset. An entry with the same `id` as an existing one replaces it.
- Images referenced from your data files can be loose `.png` files, and sounds loose `.wav` files. Give the path with its extension, relative to the install folder: `Custom\Terrain\lava.png`.
- The client reads these files once, when it starts. Restart it after every change.

## The Custom Folder

When the client starts, it loads its own data files first, then the `*-External.xml` files from this repository, and then, if a `Custom` folder exists, these files:

| File | What it holds | Format |
| --- | --- | --- |
| `Custom/Data/Terrain.xml` | Terrain graphics | as `Content/Data/Terrain-External.xml` |
| `Custom/Data/Items.xml` | Item graphics | as `Content/Data/Items-External.xml` |
| `Custom/Data/Body.xml` | Body (creature) graphics | as `Content/Data/Body-External.xml` |
| `Custom/Data/Audio.xml` | Sound definitions | as `Content/Data/Audio-External.xml` |
| `Custom/Data/Spells.xml` | Spell icons | `<spell id="…">` entries with a `<texture>` and `<source>`, like items |
| `Custom/Data/Library.xml` | Books and scrolls | `<book>` and `<scroll>` entries |

Every file is optional; leave out the ones you don't need. Note that the names have no `-External` suffix.

Each file has a single root element (the existing files use `<data>`) with one entry per asset inside it. The client goes through the entries in order, and when two have the same `id`, the one loaded last wins. Because the `Custom` files load last, they win over everything else. That gives you two uses:

- **Add something new**: use an `id` nobody else uses. The server decides which terrain, item or sound `id` appears in the game, so to see a new one you also need a server (for example a local test server) that sends that `id`.
- **Replace something that exists**: copy its entry from the existing data file and change it, keeping the `id`. The whole entry is replaced, so include every element you want to keep, not only the ones you changed.

A file with an XML error stops the client at startup. If the client won't start after a change, check the file you edited first.

## Loose Images

In terrain, item, body and spell entries, a `<texture>` that ends in `.png` is read straight from that file instead of from the client's content archives. The path is relative to the install folder:

```xml
<data>
  <!-- Replaces terrain 624 with a test image. -->
  <terrain id="624" action="0">
    <sprite order="1" underpile="true">
      <texture>Custom\Terrain\Trees.png</texture>
      <source>(0,0,100,100)</source>
    </sprite>
    <average>(52,105,23,255)</average>
  </terrain>
</data>
```

A `<texture>` without the extension (`Tenser\Terrain\Trees`) still loads the image from the client build, so a custom entry can also point at existing art, for example to try a different `source` rectangle.

Two differences from images in a client build:

- **Transparency comes from the PNG itself.** Magenta (`255,0,255`) is only made transparent when an image is built into the client. A loose image is used as it is, so a magenta background stays visible. Save the image with a transparent background (an alpha channel) instead.
- **The file must exist.** A missing image stops the client while it loads, so check the path if the client won't start.

## Loose Sounds

In `Custom/Data/Audio.xml`, a `content` path that ends in `.wav` is read from that file, relative to the install folder:

```xml
<data>
  <!-- Replaces the bola throw sound. -->
  <definition id="30007" name="Bola Throw" content="Custom\Audio\Bola_-_Throw.wav" category="2" />
</data>
```

Use uncompressed (PCM) `.wav` files. A path without the extension still loads the sound from the client build.

## Loose Files Win Over the Archives

The client looks for every file in its install folder before it looks in its content archives (`Data.bin`, `Kesmai.bin`, `Stormhalter.bin` and `UI.bin`). A loose file at the same path as a file inside an archive is used in its place. This is how the `Custom` folder is found, and it works for any other file too. For example, a loose `Data\Terrain-External.xml` in the install folder is read instead of the one in the archives.

Prefer the `Custom` folder for everyday testing: it only replaces the entries you put in it, while a loose copy of a whole data file hides every later change to that file in new client builds. Images and sounds inside the archives are stored in a compiled form (`.xnb`), so a loose `.png` or `.wav` at an archive path does not replace them; point a custom entry at the loose file instead.

## Cleaning Up

Delete or rename the `Custom` folder to go back to the normal game. Client updates replace the content archives but leave the `Custom` folder alone, so remember to remove your test overrides once your art is in a client build, or they will keep hiding the official version.

## See Also

- [Adding Art and Sounds](AddingArt.md): where art and sound files go in this repository, the data file formats, and previewing terrain in WorldForge.
