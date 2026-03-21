using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class StrongShieldBracelet : ShieldBracelet, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 2500;

	/// <inheritdoc />
	public override int Weight => 4;
		
	/// <inheritdoc />
	public override int Shield => 6;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="StrongShieldBracelet"/> class.
	/// </summary>
	public StrongShieldBracelet() : base(8)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="StrongShieldBracelet"/> class.
	/// </summary>
	public StrongShieldBracelet(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200074); /* [a silver bracelet made of scales.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250054); /* The bracelet contains a strong spell of Shield. */
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
