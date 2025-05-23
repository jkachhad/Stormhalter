using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class SaffronRobe : Robe, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 2400;

	/// <inheritdoc />
	public override int Weight => 1600;

	/// <inheritdoc />
	/// <remarks>Robes have a default <see cref="Hindrance"/> value of 1.</remarks>
	public override int Hindrance => 0;

	/// <inheritdoc />
	public override int ProtectionFromFire => 5;

	/// <inheritdoc />
	public override int ProtectionFromIce => 5;

	/// <inheritdoc />
	public override int ManaRegeneration => 1;

	/// <summary>
	/// Initializes a new instance of the <see cref="SaffronRobe"/> class.
	/// </summary>
	public SaffronRobe() : base(236)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="SaffronRobe"/> class.
	/// </summary>
	public SaffronRobe(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200184)); /* [You are looking at] [a saffron robe made of a fine cloth.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250100)); /* The robe is extremely light. */
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