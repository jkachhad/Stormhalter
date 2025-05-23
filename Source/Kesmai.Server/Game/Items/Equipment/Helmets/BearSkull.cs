using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class BearSkull : Helmet, ITreasure
{
	/// <inheritdoc />
	public override int LabelNumber => 6000085;

	/// <inheritdoc />
	public override uint BasePrice => 2000;

	/// <inheritdoc />
	public override int Weight => 80;

	/// <inheritdoc />
	public override int ProtectionFromDaze => 13;
		
	/// <inheritdoc />
	public override int ProtectionFromFire => 10;
		
	/// <inheritdoc />
	public override int ProtectionFromIce => 10;
		
	/// <inheritdoc />
	public override bool ProvidesNightVision => true;

	/// <summary>
	/// Initializes a new instance of the <see cref="BearSkull"/> class.
	/// </summary>
	public BearSkull() : base(34)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="BearSkull"/> class.
	/// </summary>
	public BearSkull(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200210)); /* [You are looking at] [a bear skull.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250105)); /* The skull is very thick and contains the spell of Night Vision. */
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