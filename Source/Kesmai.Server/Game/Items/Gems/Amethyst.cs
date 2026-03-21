using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class Amethyst : Gem
{
	/// <inheritdoc />
	public override int Weight => 5;
			
	/// <summary>
	/// Initializes a new instance of the <see cref="Amethyst"/> class.
	/// </summary>
	public Amethyst(uint basePrice) : base(179, basePrice)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Amethyst"/> class.
	/// </summary>
	public Amethyst(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200133); /* [an intensely violet amethyst.] */
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