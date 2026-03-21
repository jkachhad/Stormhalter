using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class UncutEmerald : Gem
{
	/// <inheritdoc />
	public override int Weight => 5;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="UncutEmerald"/> class.
	/// </summary>
	public UncutEmerald(uint basePrice) : base(178, basePrice)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="UncutEmerald"/> class.
	/// </summary>
	public UncutEmerald(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200132); /* [a massive uncut emerald, as green as spring grass.] */
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
