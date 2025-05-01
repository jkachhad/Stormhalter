using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class IvoryBrooch : StunDeathProtectionAmulet
{
	/// <summary>
	/// Gets the label number.
	/// </summary>
	public override int LabelNumber => 6000020;

	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 3000;

	/// <summary>
	/// Gets the weight.
	/// </summary>
	public override int Weight => 100;
		
	public IvoryBrooch() : this(3)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="IvoryBrooch"/> class.
	/// </summary>
	public IvoryBrooch(Serial serial) : base(serial)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="IvoryBrooch"/> class.
	/// </summary>
	public IvoryBrooch(int charges = 3) : base(301, charges)
	{
	}
		
	/// <summary>
	/// Gets the description for this instance.
	/// </summary>
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200071)); /* [You are looking at] [an ivory brooch of seven intertwined dragons.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250052)); /* The amulet contains the spell of Protection from Stun and Death. */
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