using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Kesmai.WorldForge.Windows;

namespace Kesmai.WorldForge;

public class Alignment
{
	public static Alignment Lawful = new Alignment(0, "Lawful", false, Color.Cyan);
	public static Alignment Neutral = new Alignment(1, "Neutral", false, Color.Lime);
	public static Alignment Chaotic = new Alignment(2, "Chaotic", true, Color.Yellow);
	public static Alignment Evil = new Alignment(3, "Evil", true, Color.Red);

	public static Alignment[] All =
	{
		Lawful,
		Neutral,
		Chaotic,
		Evil,
	};

	public static Alignment Defensive = new Alignment(10, "Defensive", true, Color.Magenta);

	private static readonly List<Alignment> _actions = new List<Alignment>
	{
		Lawful,
		Neutral,
		Chaotic,
		Evil,
		Defensive
	};

	/// <summary>
	///     Finds the Alignment from the specified index.
	/// </summary>
	public static Alignment Find(byte index)
	{
		return _actions.FirstOrDefault(a => a.Id == index);
	}

	/// <summary>
	///     Implements the greater than operator.
	/// </summary>
	public static bool operator >(Alignment a1, Alignment a2)
	{
		return a1.Id > a2.Id;
	}

	/// <summary>
	///     Converts the specified alignment to a byte value.
	/// </summary>
	public static implicit operator byte(Alignment alignment)
	{
		if (alignment != null)
			return alignment.Id;

		return 255;
	}

	/// <summary>
	///     Converts the specified byte value to an alignment.
	/// </summary>
	public static implicit operator Alignment(byte value)
	{
		return Find(value);
	}

	/// <summary>
	///     Implements the greater than operator.
	/// </summary>
	public static bool operator <(Alignment a1, Alignment a2)
	{
		return a1.Id < a2.Id;
	}

	/// <summary>
	///     Gets the alignment id.
	/// </summary>
	public byte Id { get; }

	/// <summary>
	///     Gets the alignment description.
	/// </summary>
	public string Description { get; }

	/// <summary>
	///     Gets the alignment color.
	/// </summary>
	public Color Color { get; }

	/// <summary>
	///     Gets a value indicating if this alignment is hostile.
	/// </summary>
	public bool IsHostile { get; }

	/// <summary>
	///     Initializes a new instance of the <see cref="Alignment" /> class.
	/// </summary>
	public Alignment(byte id, string description, bool isHostile, Color color)
	{
		Id = id;
		Description = description;
		IsHostile = isHostile;
		Color = color;
	}
}
	
public class AlignmentItemsSource : IItemsSource
{
	public ItemCollection GetValues()
	{
		return new ItemCollection
		{
			{ Alignment.Lawful, "Lawful" }, 
			{ Alignment.Neutral, "Neutral" },
			{ Alignment.Chaotic, "Chaotic" }, 
			{ Alignment.Evil, "Evil" }
		};
	}
}