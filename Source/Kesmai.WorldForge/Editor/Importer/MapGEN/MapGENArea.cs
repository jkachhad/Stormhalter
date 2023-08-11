using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge.Editor;
#if (CanImport)
public class MapGENTile
{
	public int[] Tiles;
	public bool Wall;
	public bool WallConnect;
	public bool Ruins;
		
	public bool IsEmpty
	{
		get
		{
			for (int i = 0; i < Tiles.Length; i++)
			{
				if (Tiles[i] > 0)
					return false;
			}

			return true;
		}
	}
		
	public MapGENTile(int[] tiles) : this( tiles, false, false, false )
	{
	}

	public MapGENTile(int[] tiles, bool wall, bool wallConnect, bool ruins)
	{
		Tiles = tiles;

		Wall = wall;
		WallConnect = wallConnect;
		Ruins = ruins;
	}

	public MapGENTile Copy()
	{
		return new Editor.MapGENTile( Tiles, Wall, WallConnect, Ruins );
	}

	public IEnumerable<int> GetTerrain()
	{
		for (int i = 0; i < Tiles.Length; i++)
		{
			if (Tiles[i] > 0)
				yield return Tiles[i];
		}
	}
}

public class MapGENOverride
{
	public int X, Y;
	public int[] Tiles;

	public MapGENOverride(string line)
	{
		string[] args = line.Split(new char[] { ',' });

		X = int.Parse(args[0]);
		Y = int.Parse(args[1]);

		Tiles = new int[4];

		Tiles[0] = int.Parse(args[2]);
		Tiles[1] = int.Parse(args[3]);
		Tiles[2] = int.Parse(args[4]);
		Tiles[3] = int.Parse(args[5]);
	}

	public void Merge(MapGENTile tile)
	{
		for (int i = 0; i < 4; i++)
		{
			if (Tiles[i] == -1)
				continue;

			tile.Tiles[i] = Tiles[i];
		}
	}
}

public class MapGENArea
{
	public Rectangle Rect;

	public int Floor, Floor2;
	public int WallHorizontal;
	public int WallVertical;
	public int WallNE;
	public int WallSolid;
	public int RuinsHorz, RuinsVert;

	public MapGENTile DoorHorz, DoorVert;

	public static MapGENTile[] Doors;
		
	/// <summary>
	/// Gets or sets the left region bound.
	/// </summary>
	public int Left { get; set; }

	/// <summary>
	/// Gets or sets the top region bound.
	/// </summary>
	public int Top { get; set; }

	/// <summary>
	/// Gets or sets the right region bound.
	/// </summary>
	public int Right { get; set; }

	/// <summary>
	/// Gets or sets the bottom region bound.
	/// </summary>
	public int Bottom { get; set; }

	/// <summary>
	/// Gets the width.
	/// </summary>
	public int Width { get; set; }

	/// <summary>
	/// Gets the height.
	/// </summary>
	public int Height { get; set; }
		
	/// <summary>
	/// Gets the count.
	/// </summary>
	public int Count => Width * Height;
		
	/// <summary>
	/// Gets or sets the elevation.
	/// </summary>
	public int Elevation { get; set; }

	static MapGENArea()
	{
		Doors = new MapGENTile[14];

		Doors[0] = new MapGENTile(new int[] { 0, 0, 507, 0 }, false, true, false);	// Dungeon Door, Horz
		Doors[1] = new MapGENTile(new int[] { 0, 0, 609, 0 }, false, true, false);	// Dungeon Door, Vert
		Doors[2] = new MapGENTile(new int[] { 0, 0, 513, 0 }, false, true, false);	// Town Door, Horz
		Doors[3] = new MapGENTile(new int[] { 0, 0, 631, 0 }, false, true, false);	// Town Door, Vert

		Doors[4] = new MapGENTile(new int[] { 0, 650, 368, 0 }, false, true, false);	// Dungeon Door, Horz
		Doors[5] = new MapGENTile(new int[] { 0, 649, 367, 0 }, false, true, false);	// Dungeon Door, Vert

		Doors[6] = new MapGENTile(new int[] { 0, 115, 518, 501 }, false, true, false);	// Fancy Door, Horz
		Doors[7] = new MapGENTile(new int[] { 0, 217, 628, 621 }, false, true, false);	// Fancy Door, Vert

		Doors[8] = new MapGENTile(new int[] { 0, 374, 372, 0 }, false, true, false);	// Torii Door, Horz
		Doors[9] = new MapGENTile(new int[] { 0, 373, 375, 0 }, false, true, false);	// Torii Door, Vert

		Doors[10] = new MapGENTile( new int[] { 0, 194, 192, 0 }, false, true, false );	// Torii Door, Horz
		Doors[11] = new MapGENTile( new int[] { 0, 193, 191, 0 }, false, true, false );	// Torii Door, Vert

	}

	public MapGENArea(string line)
	{
		string[] args = line.Split( new char[] { ',' } );
		// x1, y1, x2, y2, floorTile

		int x1 = int.Parse( args[0] );
		int y1 = int.Parse( args[1] );
		int x2 = int.Parse( args[2] );
		int y2 = int.Parse( args[3] );
		Rect = new Rectangle(x1, y1, x2 - x1, y2 - y1);

		Left = Rect.Left;
		Top = Rect.Top;
		Right = Rect.Right;
		Bottom = Rect.Bottom;
			
		Width = Rect.Width;
		Height = Rect.Height;

		Floor = int.Parse(args[4]);

		WallHorizontal = int.Parse(args[5]);
		WallVertical = int.Parse(args[6]);
		WallNE = int.Parse(args[7]);
		WallSolid = int.Parse(args[8]);

		DoorHorz = Doors[int.Parse(args[9])].Copy();
		DoorVert = Doors[int.Parse( args[10] )].Copy();

		RuinsHorz = int.Parse(args[11]);
		RuinsVert = int.Parse(args[12]);

		Floor2 = int.Parse(args[13]);
	}

	public bool Contains( int x, int y )
	{
		if ( x < Rect.Left || x > Rect.Right )
			return false;

		if ( y < Rect.Top || y > Rect.Bottom )
			return false;

		return true;
	}
}
#endif