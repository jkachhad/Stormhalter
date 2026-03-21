using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class RawSapphire : Gem
{
	/// <inheritdoc />
	public override int Weight => 5;

	/// <summary>
	/// Initializes a new instance of the <see cref="RawSapphire"/> class.
	/// </summary>
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
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200300); /* [an enormous raw sapphire. A good jeweler could cut this into something exquisite.] */
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
