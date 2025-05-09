using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class PinkPearls : LocateAmulet
{
	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 200;

	/// <summary>
	/// Gets the weight.
	/// </summary>
	public override int Weight => 100;
		
	public PinkPearls() : this(3)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="PinkPearls"/> class.
	/// </summary>
	public PinkPearls(int charges = 3) : base(3, charges)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="PinkPearls"/> class.
	/// </summary>
	public PinkPearls(Serial serial) : base(serial)
	{
	}

	/// <summary>
	/// Gets the description for this instance.
	/// </summary>
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200011)); /* [You are looking at] [a necklace of lustrous pink pearls.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250010)); /* The amulet contains the spell of Locate. */
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