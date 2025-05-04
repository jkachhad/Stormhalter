using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class LargeEmerald : Gem
{
	/// <summary>
	/// Gets the weight.
	/// </summary>
	public override int Weight => 5;

	/// <summary>
	/// Initializes a new instance of the <see cref="LargeEmerald"/> class.
	/// </summary>
	[WorldForge]
	public LargeEmerald(uint basePrice) : base(61, basePrice)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LargeEmerald"/> class.
	/// </summary>
	public LargeEmerald(Serial serial) : base(serial)
	{
	}

	/// <summary>
	/// Gets the description for this instance.
	/// </summary>
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200117)); /* [You are looking at] [a large green emerald.] */
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