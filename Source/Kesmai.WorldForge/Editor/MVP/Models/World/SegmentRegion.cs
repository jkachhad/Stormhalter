using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CommonServiceLocator;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.Editor
{
	[DebuggerDisplay("{Name} [{ID}]")]
	public class SegmentRegion : ObservableObject
	{
		private int _id;
		private string _name;

		private int _elevation;
		
		private double _experienceMultiplier = 1.0f;
		private double _healthMultiplier = 1.0f;

		private double _level = 0;
		
		private int _chunkSize;
		private Dictionary<ChunkCoordinate, SegmentTile[,]> _chunks;

		[Browsable(true)]
		public int ID
		{
			get => _id;
			set => SetProperty(ref _id, value);
		}

		[Browsable(true)]
		public string Name
		{
			get => _name;
			set => SetProperty(ref _name, value);
		}

		[Browsable(true)]
		public int Elevation
		{
			get => _elevation;
			set => SetProperty(ref _elevation, value);
		}

		public double ExperienceMultiplier
		{
			get => _experienceMultiplier;
			set => SetProperty(ref _experienceMultiplier, value);
		}
		
		public double HealthMultiplier
		{
			get => _healthMultiplier;
			set => SetProperty(ref _healthMultiplier, value);
		}
		
		public double Level
		{
			get => _level;
			set => SetProperty(ref _level, value);
		}
		
		public SegmentRegion(int id) : this()
		{
			_id = id;
			_name = $"Region {_id}";
		}
		
		public SegmentRegion(XElement element) : this()
		{
			_id = (int)element.Element("id");
			_name = (string)element.Element("name");
			_elevation = (int)element.Element("height");

			_experienceMultiplier = element.Element("experienceMultiplier", 1.0D);
			_healthMultiplier = element.Element("healthMultiplier", 1.0D);
			_level = element.Element("level", 0);

			foreach (var tileElement in element.Elements("tile"))
			{
				var x = (int)tileElement.Attribute("x");
				var y = (int)tileElement.Attribute("y");

				SetTile(x, y, new SegmentTile(tileElement));
			}
		}

		public SegmentRegion()
		{
			_chunkSize = 100;
			_chunks = new Dictionary<ChunkCoordinate, SegmentTile[,]>();
		}
		
		/// <summary>
		/// Gets an XML element that describes this instance.
		/// </summary>
		public XElement GetXElement()
		{
			var element = new XElement("region");

			element.Add(new XElement("id", ID));
			element.Add(new XElement("name", Name));
			element.Add(new XElement("height", Elevation));

			if (_experienceMultiplier != 1.0D)
				element.Add(new XElement("experienceMultiplier", _experienceMultiplier));

			if (_healthMultiplier != 1.0D)
				element.Add(new XElement("healthMultiplier", _healthMultiplier));
			
			if (_level != 0)
				element.Add(new XElement("level", _level));

			var bounds = GetMinimalBounds();

			if (bounds.Top.HasValue && bounds.Left.HasValue && bounds.Width.HasValue && bounds.Height.HasValue)
			{
				for (var y = bounds.Top.Value; y < (bounds.Top + bounds.Height.Value); y++)
				for (var x = bounds.Left.Value; x < (bounds.Left + bounds.Width.Value); x++)
				{
					var tile = GetTile(x, y);

					if (tile != null)
					{
						var tileElement = tile.GetXElement();

						tileElement.Add(new XAttribute("x", x));
						tileElement.Add(new XAttribute("y", y));

						element.Add(tileElement);
					}
				}
			}

			return element;
		}

		public void UpdateTiles()
		{
			var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
			
			foreach (var tile in GetTiles((t) => true))
				tile.UpdateTerrain(presenter.SelectedFilter);
		}

		public IEnumerable<SegmentTile> GetTiles()
		{
			return GetTiles(tile => true);
		}
		
		public IEnumerable<SegmentTile> GetTiles(Func<SegmentTile, bool> condition)
		{
			var chunks = _chunks.Values.ToArray();
			
			foreach (var chunk in chunks)
			{
				for (var x = 0; x < _chunkSize; x++)
				for (var y = 0; y < _chunkSize; y++)
				{
					var tile = chunk[x, y];

					if (tile != null && condition(tile))
						yield return tile;
				}
			}
		}

		public SegmentTile[,] GetChunk(int x, int y)
		{
			return GetChunk(GetChunkCoordinate(x, y), false);
		}

		private SegmentTile[,] GetChunk(ChunkCoordinate chunkCoordinate, bool create = false)
		{
			if (!_chunks.TryGetValue(chunkCoordinate, out SegmentTile[,] chunk) && create)
				chunk = _chunks[chunkCoordinate] = new SegmentTile[_chunkSize, _chunkSize];

			return chunk;
		}

		private ChunkCoordinate GetChunkCoordinate(int x, int y)
		{
			var chunkX = (int)Math.Floor((double)x / _chunkSize);
			var chunkY = (int)Math.Floor((double)y / _chunkSize);

			return new ChunkCoordinate(chunkX, chunkY);
		}

		public SegmentTile GetTile(SegmentTile source, Direction direction, bool create = false)
		{
			return GetTile(source.X + direction.Dx, source.Y + direction.Dy, create);
		}

		public SegmentTile GetTile(int x, int y, bool create = false)
		{
			var chunkCoordinate = GetChunkCoordinate(x, y);
			var tiles = GetChunk(chunkCoordinate, true);

			var ox = Math.Abs(x - (chunkCoordinate.X * _chunkSize));
			var oy = Math.Abs(y - (chunkCoordinate.Y * _chunkSize));

			var tile = tiles[ox, oy];

			if (tile == null && create)
				tile = CreateTile(x, y);

			return tile;
		}

		public SegmentTile CreateTile(int x, int y)
		{
			return SetTile(x, y, new SegmentTile(x, y));
		}

		public SegmentTile SetTile(int x, int y, SegmentTile tile)
		{
			var coordinate = GetChunkCoordinate(x, y);
			var tiles = GetChunk(coordinate, true);

			var chunkX = (x - coordinate.X * _chunkSize);
			var chunkY = (y - coordinate.Y * _chunkSize);

			tiles[chunkX, chunkY] = tile;

			return tile;
		}
		
		/// <summary>
		/// Deletes the tile at the specified coordinates.
		/// </summary>
		public void DeleteTile(int x, int y)
		{
			SetTile(x, y, null);
		}
		
		private (int? Left, int? Top, int? Width, int? Height) GetMinimalBounds()
		{
			var left = default(int?); var right = default(int?); var top = default(int?); var bottom = default(int?);
			
			foreach (var coordinate in _chunks.Keys)
			{
				var tiles = GetChunk(coordinate);

				for (var y = 0; y < _chunkSize; y++)
				{
					for (var x = 0; x < _chunkSize; x++)
					{
						var tile = tiles[x, y];
						var validTile = (tile != null);

						if (validTile)
						{
							var mx = (coordinate.X * _chunkSize) + x;
							var my = (coordinate.Y * _chunkSize) + y;

							if (!left.HasValue || mx < left) left = mx; 
							else if (!right.HasValue || mx > right) right = mx;

							if (!top.HasValue || my < top) top = my;
							else if (!bottom.HasValue || my > bottom) bottom = my;
						}
					}
				}
			}

			return (left, top, right - left + 1, bottom - top + 1);
		}
	}

	public class JumpSegmentRegionLocation
    {
		public int Region;
		public int X;
		public int Y;
		public JumpSegmentRegionLocation ( int targetRegion, int targetX, int targetY)
        {
			Region = targetRegion;
			X = targetX;
			Y = targetY;
        }
    }

	public class ChunkCoordinate : Tuple<int, int>
	{
		public int X => Item1;
		public int Y => Item2;

		public ChunkCoordinate(int x, int y) : base(x, y)
		{
		}
	}
}
