using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class HummingbirdAmulet : StunDeathProtectionAmulet
{
	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 4500;

	/// <summary>
	/// Gets the weight.
	/// </summary>
	public override int Weight => 100;
		
	public HummingbirdAmulet() : this(1)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="HummingbirdAmulet"/> class.
	/// </summary>
	public HummingbirdAmulet(int charges = 1) : base(305, charges)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="HummingbirdAmulet"/> class.
	/// </summary>
	public HummingbirdAmulet(Serial serial) : base(serial)
	{
	}
		
	/// <summary>
	/// Gets the description for this instance.
	/// </summary>
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200073)); /* [You are looking at] [a crystal hummingbird hanging from a golden chain.] */

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