# Adding Art and Sounds

This guide explains how to add new images and sounds to the game: where to put the files, how to point terrain, items, bodies and sounds at them, and how to preview them in WorldForge.

## The Short Version

- Put images (`.png`) in your own folder under `Content`, for example `Content/YourName/Terrain/lava.png`.
- Put sounds (`.wav`) under `Content/Audio`, for example `Content/Audio/Traps/Spike_Trap.wav`.
- Paint anything that should be see-through in pure magenta (red 255, green 0, blue 255).
- Refer to the file from the data files in `Content/Data` by its path under `Content`, without the extension: `YourName\Terrain\lava`. The path must match the file name exactly, including upper and lower case.
- That's all. There is no list to register the file in: the next client build includes every image under `Content` and every sound under `Content/Audio`.

## Where Files Go

| What | Where | Example |
| --- | --- | --- |
| Images (terrain, items, monsters, bodies) | `Content/<artist>/…` (one folder per artist, any subfolders) | `Content/Tenser/Terrain/Trees.png` |
| Sounds | `Content/Audio/<category>/…` | `Content/Audio/Mobs/Ghost_-_Wander_-_01.wav` |
| Terrain definitions | `Content/Data/Terrain-External.xml` | |
| Item graphics | `Content/Data/Items-External.xml` | |
| Body (creature) graphics | `Content/Data/Body-External.xml` | |
| Sound definitions | `Content/Data/Audio-External.xml` | |

Images must be PNG files and sounds WAV files. An image can be a whole sheet: the data files pick the part they need with a `source` rectangle.

## Transparency

The game replaces pure magenta (`255,0,255`) with transparency. Use it for the background around a sprite and for any holes in it. Colors that are only close to magenta stay visible, so make sure your editor doesn't blend the edges of the background (turn off anti-aliasing for that color).

## Pointing at Your Files

Paths are relative to `Content` and leave out the extension. Use `\` or `/`; both work.

**Terrain**, in `Terrain-External.xml`. `source` is the rectangle in the image as `(x,y,width,height)`:

```xml
<terrain id="624" action="0">
  <sprite order="1" underpile="true">
    <texture>Tenser\Terrain\Trees</texture>
    <source>(0,0,100,100)</source>
  </sprite>
  <average>(52,105,23,255)</average>
</terrain>
```

**Items**, in `Items-External.xml`:

```xml
<!-- Heron Main-Gauche -->
<item id="900" category="14">
  <offset>(-5,-5)</offset>
  <texture>Tenser/Items/daggers</texture>
  <source>(55,0,55,55)</source>
</item>
```

**Sounds**, in `Audio-External.xml`. `content` is the path of the `.wav` file under `Content`:

```xml
<definition id="30007" name="Bola Throw" content="Audio\Actions\Bola_-_Throw" category="2" />
```

The comments at the top of each data file describe the remaining attributes (terrain actions and draw order, item categories, body palettes, sound categories).

### Upper and lower case matter

The game looks files up in its content archives, and that lookup is case-sensitive, even on Windows. `Tenser\Terrain\Trees` does not find `tenser/terrain/trees.png`. Copy the path from the file's real name to be safe. Spaces in names work (`Tenser\Terrain\wall brazier`), but names without spaces are easier to type correctly.

## Previewing in WorldForge

WorldForge can show your art before it is in a client build:

1. Start WorldForge once. It creates a `.storage` folder in the folder it runs from (normally next to `Kesmai.WorldForge.exe`) with a file called `CustomArt.cfg`.
2. Replace the first line of `CustomArt.cfg` with the full path of your local `Content` folder, for example `C:\Stormsmith\Stormhalter\Content`.
3. Restart WorldForge. It now reads `Data\Terrain-External.xml` from that folder and loads any image it refers to straight from your files.

WorldForge shows these loose images as they are, so the magenta background is still visible there. The game makes it transparent once the image is in a client build.

To try your art in the game client itself before a client build, see [Overriding Content in the Client](CustomContent.md).

## When It Reaches the Game

The client's content archives (`Stormhalter.bin` and the others) are built from these folders. Every `.png` under `Content` and every `.wav` under `Content/Audio` is included automatically, so a new image or sound ships with the next client build. WorldForge no longer asks to add new textures to a content list.

A handful of older files in the repository are deliberately held back from the game. If a file is in the right place, its path matches exactly, and it still doesn't appear after a client build, ask a maintainer whether it is one of them.
