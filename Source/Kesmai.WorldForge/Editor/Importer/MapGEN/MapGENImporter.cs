using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommonServiceLocator;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Editor
{
#if (CanImport)
	
	public class MapGENImporter : ISegmentImporter
	{
		public static MapGENArea DefaultArea = new MapGENArea("0, 0, 255, 255, 15, 129, 282, 231, 207, 0, 1, 104, 204, 0");
		
		public ObservableCollection<MapGENRegion> Regions { get; set; } = new ObservableCollection<MapGENRegion>();

		private Random _random = new Random();
		private string _name;
		private List<MapGENOverride> _overrides;
		private MapGENTile[,] _tiles;

		public void Import(string path)
		{
			var definitionFile = new FileInfo(path);

			var directory = definitionFile.Directory.FullName;
			var segmentName = _name = definitionFile.Name.Replace(".def", String.Empty);

			var overridesFile = new FileInfo($@"{directory}\{segmentName}.ovr");
			var terrainFile = new FileInfo($@"{directory}\{segmentName}.txt");

			var line = String.Empty;

			var id = 1;
			
			/**/
			using (var defStream = new FileStream(definitionFile.FullName,
				FileMode.Open, FileAccess.Read, FileShare.None))
			using (var defReader = new StreamReader(defStream))
			{
				while ((line = defReader.ReadLine()) != null)
				{
					if (line.StartsWith(";"))
						continue;

					Regions.Add(new MapGENRegion(line)
					{
						Id = id++,
						Import = true,
					});
				}
			}

			var mapTiles = _tiles = new MapGENTile[255, 255];

			int yCoord = 0;
			int yMax = 0, xMax = 0;

			/**/
			using (var stream = new FileStream(terrainFile.FullName,
				FileMode.Open, FileAccess.Read, FileShare.None))
			using (var reader = new StreamReader(stream))
			{
				while ((line = reader.ReadLine()) != null)
				{
					for (int xCoord = 0; xCoord < line.Length; xCoord++)
					{
						mapTiles[xCoord, yCoord] = TranslateTile(xCoord, yCoord, line[xCoord]);

						if (xCoord > xMax)
							xMax = xCoord;
					}

					yCoord++;

					if (yCoord > yMax)
						yMax = yCoord;
				}
			}

			xMax++;

			int xMin = 0;
			int yMin = 0;

			/**/
			using (var ovrStream = new FileStream(overridesFile.FullName,
				FileMode.Open, FileAccess.Read, FileShare.None))
			using (var ovrReader = new StreamReader(ovrStream))
			{
				_overrides = new List<MapGENOverride>();

				while ((line = ovrReader.ReadLine()) != null)
				{
					if (line.StartsWith(";"))
						continue;

					if (line.StartsWith("#W"))
					{
						string[] wArg = line.Replace("#W", "").Split(new char[] { ',' });

						int xW = int.Parse(wArg[0]);
						int yW = int.Parse(wArg[1]);

						var tile = mapTiles[xW, yW];

						tile.WallConnect = true;
						continue;
					}

					var ovr = new MapGENOverride(line);

					if (ovr.X < xMin)
						xMin = ovr.X;

					if (ovr.Y < yMin)
						yMin = ovr.Y;

					_overrides.Add(ovr);
				}
			}
			
			/**/
			for (int i = 0; i < yMax; i++)
			{
				for (int j = 0; j < xMax; j++)
				{
					var tile = mapTiles[j, i];
					var area = Regions.FirstOrDefault(a => a.Contains(j, i)) ?? DefaultArea;

					MapGENTile tileW = null, tileE = null, tileN = null, tileNW = null, tileS = null, tileNE = null, tileSE = null;

					if (j > 0) tileW = mapTiles[j - 1, i];
					if (j < xMax) tileE = mapTiles[j + 1, i];
					if (i > 0) tileN = mapTiles[j, i - 1];
					if (i > 0 && j > 0) tileNW = mapTiles[j - 1, i - 1];
					if ( i > 0 && i < yMax && j < xMax ) tileNE = mapTiles[j + 1, i - 1];
					if (i < yMax) tileS = mapTiles[j, i + 1];
					if (j > 0 && j < xMax && i >= 0 && i < ( yMax - 1 ) ) tileSE = mapTiles[j - 1, i + 1];

					bool needW = (tileW != null && (tileW.Wall || tileW.WallConnect));
					bool needE = (tileE != null && (tileE.Wall || tileE.WallConnect));
					bool needN = (tileN != null && (tileN.Wall || tileN.WallConnect));
					bool needS = (tileS != null && (tileS.Wall || tileS.WallConnect));

					if (tile != null && tile.Wall)
					{
						if ( tile.Tiles[1] == 0 && tile.Tiles[1] != area.WallSolid ) 
							tile.Tiles[1] = area.WallVertical;

						if ( tile.Tiles[0] == 0 )
							tile.Tiles[0] = area.Floor;

						if (needW || needE)
							tile.Tiles[1] = area.WallHorizontal;

						if (needN)
							tile.Tiles[1] = area.WallVertical;

						if (needN && needW)
						{
							tile.Tiles[1] = area.WallVertical;
							tile.Tiles[2] = area.WallHorizontal;
						}

						if (needE && needS && !needN && !needW)
							tile.Tiles[1] = area.WallNE;

						if ( tileW != null && !needW && !tileW.IsEmpty && tileS != null && !needS && !tileS.IsEmpty)
							tile.Tiles[0] = area.Floor;

						if ( tileN != null && !needN && !tileN.IsEmpty && tileE != null && !needE && !tileE.IsEmpty)
							tile.Tiles[0] = area.Floor;

						if (tileW != null && !needW && tileW.IsEmpty && tileS != null && !needS && tileS.IsEmpty)
							tile.Tiles[0] = 0;

						if (tileN != null && !needN && tileN.IsEmpty && tileE != null && !needE && tileE.IsEmpty)
							tile.Tiles[0] = 0;

						if ( tileNE != null && tileN != null && tileE != null && tileNE.IsEmpty && tileN.IsEmpty && tileE.IsEmpty )
							tile.Tiles[0] = 0;

						if ( tileS != null && tileS.Wall && tileN != null && !tileN.Wall && !tileN.WallConnect )
							tile.Tiles[0] = tileN.Tiles[0];

						if ( needW && tileN != null && tileE != null && !tileE.Wall && !tileE.WallConnect )
							tile.Tiles[0] = tileN.Tiles[0];

						if (_name == "Rift Glacier")
						{
							if (tileS != null && needN && !needS && !needW)
								tile.Tiles[0] = tileS.Tiles[0];

							if (tileSE == null)
								tile.Tiles[0] = 0;

							if (tileNE == null && needS)
								tile.Tiles[0] = 0;
						}
					}
					else if (tile != null && tile.Ruins)
					{
						if (needN)
							tile.Tiles[2] = area.RuinsVert;

						if (needW)
							tile.Tiles[2] = area.RuinsHorz;
					}
				}
			}

			for (int i = yMin; i < yMax; i++)
			{
				for (int j = xMin; j < xMax; j++)
				{
					var tile = default(MapGENTile);

					if (i >= 0 && j >= 0)
						tile = mapTiles[j, i];
					else
						tile = new MapGENTile(new int[] { 0, 0, 0, 0 });

					var ovr = GetTileOverride(j, i);

					if (ovr != null)
						ovr.Merge(tile);
				}
			}

			/**/
			foreach (var area in Regions)
				Convert(area);
		}
		
		public SegmentRegion Convert(MapGENRegion source)
		{
			var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();

			Terrain getTerrain(int index)
			{
				var terrain = terrainManager.FromTIF(index);

				if (terrain != null || terrainManager.TryGetValue(index, out terrain))
					return terrain;

				return default(Terrain);
			}

			var result = source.Region = new SegmentRegion(source.Id)
			{
				Name = source.Name,
				Elevation = source.Elevation
			};

			for (var x = source.Left; x <= source.Right; x++)
			for (var y = source.Top; y <= source.Bottom; y++)
			{
				var ax = x - source.Left;
				var ay = y - source.Top;

				var sourceTile = _tiles[x, y];
				
				var ovr = GetTileOverride(x, y);
				
				if (ovr != null) 
					ovr.Merge(sourceTile);
				
				if (sourceTile != null)
				{
					var resultTile = new SegmentTile(ax, ay);
					var sourceTerrain = sourceTile.GetTerrain();

					foreach (var id in sourceTerrain)
					{
						var terrain = getTerrain(id);

						if (terrain != default(Terrain) && id != 212)
							resultTile.AddComponent(new StaticComponent(terrain.ID));
					}

					result.SetTile(ax, ay, resultTile);
				}
			}
			
			foreach (var convert in Conversions.All)
				convert.Process(this, source, result);
			
			return result;
		}
		
		public MapGENOverride GetTileOverride(int x, int y)
		{
			return _overrides.FirstOrDefault(ovr => ovr.X == x && ovr.Y == y);
		}

		public MapGENTile TranslateTile(int x, int y, char value)
		{
			var area = Regions.FirstOrDefault(a => a.Contains(x, y)) ?? DefaultArea;
			var genTile = new MapGENTile(new int[] { 0, 0, 0, 0 });

			switch (value)
			{
				// Grass & Trees
				case 'Q':
				{
					genTile = new MapGENTile( new int[] { 3, 0, 0, 0 } );
					break;
				}
				case ':':
				{
					if ( _name == "Rift Glacier" )
						genTile = new MapGENTile( new int[] { 2, 0, 0, 0 } );
					else
						genTile = new MapGENTile(new int[] { 3, 0, 0, 0 });

					break;
				}
				case '#': genTile = new MapGENTile(new int[] { 4, 0, 0, 0 }); break;
				case '3':
				{
					if ( _name == "Rift Glacier" )
					{
						genTile = new MapGENTile( new int[] { 2, 835, 617, 0 } );
						break;
					}

					goto case '4';
				}
				case '4':
					{
						if (_name == "Shukumei")
							genTile = new MapGENTile(new int[] { 74, 847, 0, 0 });
						else if( _name == "Rift Glacier" )
							genTile = new MapGENTile( new int[] { 2, 835, 0, 0 } );
						else
							genTile = new MapGENTile(new int[] { 2, 0, 0, 0 }); 

						break;
					}
				case '7':
				{
					if ( _name == "Shukumei" )
						genTile = new MapGENTile( new int[] { 74, 278, 0, 0 } ); 
					else
						genTile = new MapGENTile( new int[] { 2, 278, 0, 0 } ); 

					
					break;
				}
				case '8': genTile = new MapGENTile(new int[] { 8, 278, 0, 0 }); break;
				case '!': genTile = new MapGENTile(new int[] { 8, 0, 0, 0 }); break;
				case 'K': genTile = new MapGENTile( new int[] { 31, 0, 0, 0 } ); break;
				case 'C': genTile = new MapGENTile(new int[] { 44, 0, 0, 0 }); break;
				case 'D': genTile = new MapGENTile(new int[] { 46, 0, 0, 0 }); break;
				case '0': genTile = new MapGENTile(new int[] { 11, 0, 0, 0 }); break; /**/
				case '5': genTile = new MapGENTile( new int[] { 13, 0, 0, 0 } ); break;
				case 'L': genTile = new MapGENTile( new int[] { 15, 0, 0, 0 } ); break;
				case '*': genTile = new MapGENTile(new int[] { 7, 0, 0, 0 }); break;
				case 'B': genTile = new MapGENTile( new int[] { 2, 265, -1, -1} ); break;
				case '9': genTile = new MapGENTile( new int[] { 2, 824, -1, -1 } ); break;
				case 'S': genTile = new MapGENTile( new int[] { 184, -1, -1, -1 } ); break;
				case '1':
				{
					if ( _name == "Rift Glacier" )
					{
						genTile = new MapGENTile( new int[] { 7, 835, 617, 0 } );
						break;
					}

					goto case '2';
				}
				case '2':
					{
						if (_name == "Shukumei")
							genTile = new MapGENTile(new int[] { 74, 822, 0, 0 });
						else if( _name == "Rift Glacier" )
							genTile = new MapGENTile( new int[] { 2, 835, 617, 0 } );
						else
							genTile = new MapGENTile(new int[] { 2, 822, 0, 0 });

						break;
					}
				case 'R': genTile = new MapGENTile(new int[] { 71, 0, 0, 0 }); break;

				case ';':
					{
						if (_name == "Shukumei" )
						{
							int rand = _random.Next(100);

							if ( rand < 25 )
								genTile = new MapGENTile( new int[] { 73, 847, 0, 0 } );	// 4
							else if ( rand < 50 )
								genTile = new MapGENTile( new int[] { 73, 822, 0, 0 } ); // 2
							else if ( rand < 70 )
								genTile = new MapGENTile( new int[] { 72, 0, 0, 0 } );	// "
							else if ( rand < 90 )
								genTile = new MapGENTile( new int[] { 73, 0, 0, 0 } );	// '
							else if ( rand < 97 )
								genTile = new MapGENTile( new int[] { 72, 824, 0, 0 } );	// dead tree
							else if ( rand < 100 )
								genTile = new MapGENTile( new int[] { 72, 230, 602, 0 } );	// cherry tree
							else
								genTile = new MapGENTile( new int[] { 72, 0, 0, 0 } );	// "
						}
						else if ( _name == "Rift Glacier" )
							genTile = new MapGENTile( new int[] { 2, 235, 0, 0 } );
						else
							genTile = new MapGENTile( new int[] { 8, 235, 0, 0 } );

						break;
					}
				case 'X': genTile = new MapGENTile(new int[] { 12, 0, 0, 0 }); break;
				// Walls & Doors, Structural
				case '6':
				{
					if ( _name == "Rift Glacier" )
					{
						genTile = new MapGENTile( new int[] { 13, 0, 0, 0 } );
					}
					else
					{
						genTile = new MapGENTile( new int[] { 11, 418, 0, 0 } );
					}
					break;
				}

				case '.': genTile = new MapGENTile(new int[] { 0, 19, 0, 0 }); break;
				case '-': genTile = new MapGENTile(new int[] { 0, 826, 0, 0 }); break;
				case '"': genTile = new MapGENTile(new int[] { area.Floor, 0, 0, 0 }); break;
				case '\'': genTile = new MapGENTile(new int[] { area.Floor2, 0, 0, 0 }); break;
				case 'F': genTile = new MapGENTile(new int[] { 0, 0, 0, 0 }, true, false, false); break;
				case '<': genTile = new MapGENTile(new int[] { 0, area.WallSolid, 0, 0 }, false, true, false); break;
				case ',': genTile = new MapGENTile(new int[] { area.Floor, 405, area.RuinsHorz, 0 }, false, true, true); break;
				case '%': genTile = area.DoorHorz.Copy(); break;
				case '&': genTile = area.DoorVert.Copy(); break;
				case '>':
					{
						if (_name == "Shukumei" || _name == "Rift Glacier" )
						{
							genTile = new MapGENTile( new int[] { area.Floor, 79, 0, 0 }, false, true, false );
						}
						else
						{
							genTile = new MapGENTile( new int[] { area.Floor, 224, 0, 0 }, false, true, false );
						}
						
						break;
					} 
				case '@': genTile = new MapGENTile(new int[] { area.Floor, 0, 516, 530 }, false, true, false); break;
				case '?': genTile = new MapGENTile(new int[] { 10, 0, 0, 0 }); break;
				case ')':
				{
					if ( _name == "Shukumei" )
					{
						genTile = new MapGENTile( new int[] { 1, 402, 0, 0 } );
					}
					else
					{
						genTile = new MapGENTile( new int[] { 25, 402, 0, 0 } );
					}

					break;
				}

				case 'z': genTile = new MapGENTile( new int[] { 1, 0, 0, 0 } ); break;

			}

			if ( genTile.WallConnect )
				genTile.Tiles[0] = area.Floor;

			return genTile;
		}
	}
	
#endif
}