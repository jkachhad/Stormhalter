using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;

namespace Kesmai.Server.Items;

public abstract class Robe : Equipment
{
	/// <summary>
	/// Gets the label number.
	/// </summary>
	public override int LabelNumber => 6000074;

	/// <summary>
	/// Gets the hindrance penalty for this <see cref="Armor"/>.
	/// </summary>
	/// <remarks>
	/// All robes have some hindrance with the exception of <see cref="Kimono">Kimonos</see>.
	/// </remarks>
	public override int Hindrance => 1;

	/// <summary>
	/// Gets the item category.
	/// </summary>
	public override int Category => 10;

	/// <summary>
	/// Initializes a new instance of the <see cref="Robe"/> class.
	/// </summary>
	protected Robe(int robeID) : base(robeID)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Robe"/> class.
	/// </summary>
	protected Robe(Serial serial) : base(serial)
	{
	}

	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1); /* version */
	}

	/// <summary>
	/// Deserializes this instance from persisted binary data.
	/// </summary>
	public override void Deserialize(ref SpanReader reader)
	{
		base.Deserialize(ref reader);

		var version = reader.ReadInt16();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}
}