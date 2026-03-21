using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class PearDiamond : Gem
{
	/// <summary>
	/// Gets the weight.
	/// </summary>
	public override int Weight => 5;

	/// <summary>
	/// Initializes a new instance of the <see cref="PearDiamond"/> class.
	/// </summary>
	public PearDiamond(uint basePrice) : base(153, basePrice)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PearDiamond"/> class.
	/// </summary>
	public PearDiamond(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200123); /* [an enormous pear cut diamond of the purest blue-white radiance.] */
	}

	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1);	/* version */
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
