using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class PurpleSilkRobe : Robe, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 2400;

	/// <inheritdoc />
	public override int Weight => 1600;

	/// <inheritdoc />
	public override int ProtectionFromFire => 5;

	/// <inheritdoc />
	public override int ProtectionFromIce => 5;

	/// <inheritdoc />
	public override int ManaRegeneration => 1;

	/// <summary>
	/// Initializes a new instance of the <see cref="PurpleSilkRobe"/> class.
	/// </summary>
	public PurpleSilkRobe() : base(286)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="PurpleSilkRobe"/> class.
	/// </summary>
	public PurpleSilkRobe(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200040); /* [a robe of imperial purple silk embroidered in gold thread with a chrysanthemum.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250030); /* The robe looks thick and heavy. */
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
