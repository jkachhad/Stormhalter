using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class PantherSapphire : Gem
{
	/// <inheritdoc />
	public override int Weight => 5;

	/// <summary>
	/// Initializes a new instance of the <see cref="PantherSapphire"/> class.
	/// </summary>
	public PantherSapphire(uint basePrice) : base(167, basePrice)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PantherSapphire"/> class.
	/// </summary>
	public PantherSapphire(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200129)); /* [You are looking at] [an icy-blue sapphire cut into the shape of a stalking panther.] */
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