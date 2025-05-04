using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class SmallBrassRing : Ring
{
	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 100;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="SmallBrassRing"/> class.
	/// </summary>
	public SmallBrassRing() : base(95)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="SmallBrassRing"/> class.
	/// </summary>
	public SmallBrassRing(Serial serial) : base(serial)
	{
	}
		
	/// <summary>
	/// Gets the description for this instance.
	/// </summary>
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200051)); /* [You are looking at] [a small brass ring.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250042)); /* The ring appears quite ordinary. */
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