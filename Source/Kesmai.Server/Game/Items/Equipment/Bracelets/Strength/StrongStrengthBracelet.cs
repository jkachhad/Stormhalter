using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class StrongStrengthBracelet : StrengthBracelet, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 1500;

	/// <inheritdoc />
	public override int Weight => 4;
		
	/// <inheritdoc />
	public override int StrengthBonus => 6;

	/// <summary>
	/// Initializes a new instance of the <see cref="StrongStrengthBracelet"/> class.
	/// </summary>
	public StrongStrengthBracelet() : base(133)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="StrongStrengthBracelet"/> class.
	/// </summary>
	public StrongStrengthBracelet(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200082); /* [a silver bracelet engraved for decoration.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250118); /* The bracelet contains a powerful spell of Strength. */
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
