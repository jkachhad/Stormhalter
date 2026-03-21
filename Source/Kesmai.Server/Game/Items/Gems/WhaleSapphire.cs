using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class WhaleSapphire : Gem
{
	/// <inheritdoc />
	public override int Weight => 5;

	/// <summary>
	/// Initializes a new instance of the <see cref="WhaleSapphire"/> class.
	/// </summary>
	public WhaleSapphire(uint basePrice) : base(176, basePrice)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="WhaleSapphire"/> class.
	/// </summary>
	public WhaleSapphire(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200130); /* [a dark-blue sapphire shaped like a whale.] */
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
