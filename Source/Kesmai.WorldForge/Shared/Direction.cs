using System.Diagnostics;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Windows;

namespace Kesmai.WorldForge
{
	[DebuggerDisplay("{GetAlias(0)} ({" + nameof(Index) + "})")]
	public class Direction
	{
		/* 1 2 3
		 * 4 5 6
		 * 7 8 9
		 */

		public static Direction NorthWest = new Direction(1, new Vector2F(-1, -1), new[] {"Northwest", "NW"});
		public static Direction North = new Direction(2, new Vector2F(0, -1), new[] {"North", "N"});
		public static Direction NorthEast = new Direction(3, new Vector2F(1, -1), new[] {"Northeast", "NE"});

		public static Direction West = new Direction(4, new Vector2F(-1, 0), new[] {"West", "W"});
		public static Direction None = new Direction(5, new Vector2F(0, 0), new[] {"None"});
		public static Direction East = new Direction(6, new Vector2F(1, 0), new[] {"East", "E"});

		public static Direction SouthWest = new Direction(7, new Vector2F(-1, 1), new[] {"Southwest", "SW"});
		public static Direction South = new Direction(8, new Vector2F(0, 1), new[] {"South", "S"});
		public static Direction SouthEast = new Direction(9, new Vector2F(1, 1), new[] {"Southeast", "SE"});

		public static Direction[] All =
		{
			NorthWest, North, NorthEast,
			West, None, East,
			SouthWest, South, SouthEast
		};

		public static Direction[] Cardinal = { North, East, West, South };
		
		public static Direction[] Compass = 
		{
			NorthWest, North, NorthEast,
			West, East,
			SouthWest, South, SouthEast
		};

		/// <summary>
		///     Gets the direction from the specified index.
		/// </summary>
		public static Direction GetDirection(int index)
		{
			--index;

			if (index < 0 || index > All.Length)
				return None;

			return All[index];
		}

		/// <summary>
		///     Gets the direction from the specified delta.
		/// </summary>
		public static Direction GetDirection(int dx, int dy)
		{
			return All[(dy + 1) * 3 + dx + 1];
		}
		
		private readonly string[] _alias;

		/// <summary>
		///     Gets or sets the index.
		/// </summary>
		public int Index { get; set; }

		/// <summary>
		///     Gets or sets the directional vector.
		/// </summary>
		public Vector2F Vector { get; set; }

		/// <summary>
		///     Gets a value indicating if this vector is diagonal.
		/// </summary>
		public bool IsDiagonal => Vector.Magnitude != 1.0f;

		/// <summary>
		///     Gets a value indicating the x-offset.
		/// </summary>
		public int Dx => (int)Vector.X;

		/// <summary>
		///     Gets a value indicating the x-offset.
		/// </summary>
		public int Dy => (int)Vector.Y;

		/// <summary>
		///     Gets the opposite direction.
		/// </summary>
		public Direction Opposite => GetDirection(-Dx, -Dy);
		
		/// <summary>
		///     Gets the perpendicular direction.
		/// </summary>
		public Direction Perpendicular => GetDirection(Dy, IsDiagonal ? -Dx : Dx);

		/// <summary>
		///     Initializes a new instance of the <see cref="Direction" /> class.
		/// </summary>
		public Direction(int index, Vector2F direction, string[] alias)
		{
			Index = index;
			Vector = direction;

			_alias = alias;
		}
		
		/// <summary>
		///     Gets the alias for this instance at the specified index.
		/// </summary>
		public string GetAlias(int index = 0)
		{
			if (index >= _alias.Length)
				return string.Empty;

			return _alias[index];
		}
	}
	
	public class CardinalDirectionsItemsSource : IItemsSource
	{
		public ItemCollection GetValues()
		{
			var sizes = new ItemCollection();

			foreach (var direction in Direction.Cardinal)
				sizes.Add(direction, direction.GetAlias(0));

			return sizes;
		}
	}
}