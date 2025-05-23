using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class BrownRobe : Robe, ITreasure
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
	/// Initializes a new instance of the <see cref="BrownRobe"/> class.
	/// </summary>
	public BrownRobe() : base(265)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="BrownRobe"/> class.
	/// </summary>
	public BrownRobe(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200200)); /* [You are looking at] [a brown robe interwoven with a black pattern.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250030)); /* The robe looks thick and heavy. */
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