using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class StrongShieldRing : ShieldRing
{
	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 2000;

	/// <summary>
	/// Gets or sets the shield-value provided by this ring.
	/// </summary>
	public override int Shield => 6;

	/// <summary>
	/// Initializes a new instance of the <see cref="StrongShieldRing"/> class.
	/// </summary>
	public StrongShieldRing() : base(501)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="StrongShieldRing"/> class.
	/// </summary>
	public StrongShieldRing(Serial serial) : base(serial)
	{
	}
		
	/// <summary>
	/// Gets the description for this instance.
	/// </summary>
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200046)); /* [You are looking at] [a small iron ring with a large black stone.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250037)); /* The ring contains a strong spell of Shield. */
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

#if (Beta)
		ItemId = 501;
#endif
	}
}