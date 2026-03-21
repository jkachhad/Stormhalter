using System.Drawing;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public abstract class Gem : ItemEntity, ITreasure
{
	protected uint _basePrice;
		
	/// <summary>
	/// Gets the label number.
	/// </summary>
	public override int LabelNumber => 6000042;

	/// <summary>
	/// Gets the item category.
	/// </summary>
	public override int Category => 3;

	/// <summary>
	/// Gets the base price of this gem.
	/// </summary>
	public override uint BasePrice => _basePrice;

	/// <summary>
	/// Initializes a new instance of the <see cref="Gem"/> class.
	/// </summary>
	protected Gem(int gemID, uint basePrice) : base(gemID)
	{
		_basePrice = basePrice;
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Gem"/> class.
	/// </summary>
	protected Gem(Serial serial) : base(serial)
	{
	}

	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1);	/* version */

		writer.Write(_basePrice);
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
				_basePrice = reader.ReadUInt32();
				break;
			}
		}
	}
}
