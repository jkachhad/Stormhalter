using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class SmallTopaz : Gem
{
	/// <summary>
	/// Gets the weight.
	/// </summary>
	public override int Weight => 5;

	/// <summary>
	/// Initializes a new instance of the <see cref="SmallTopaz"/> class.
	/// </summary>
	[WorldForge]
	public SmallTopaz(uint basePrice) : base(150, basePrice)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SmallTopaz"/> class.
	/// </summary>
	public SmallTopaz(Serial serial) : base(serial)
	{
	}

	/// <summary>
	/// Gets the description for this instance.
	/// </summary>
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200022)); /* [You are looking at] [a small yellow gem.] */
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