using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class RawSapphire : Gem
{
	/// <inheritdoc />
	public override int Weight => 5;

	/// <summary>
	/// Initializes a new instance of the <see cref="RawSapphire"/> class.
	/// </summary>
	[WorldForge]
	public RawSapphire(uint basePrice) : base(361, basePrice)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="RawSapphire"/> class.
	/// </summary>
	public RawSapphire(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200300)); /* [You are looking at] [an enormous raw sapphire. A good jeweler could cut this into something exquisite.] */
	}
	
	/// <inheritdoc />
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1); /* version */
	}

	/// <inheritdoc />
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