using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class LizardScales : Armor
{
	/// <inheritdoc />
	public override int LabelNumber => 6000096; /* vest */

	/// <inheritdoc />
	public override uint BasePrice => 30;

	/// <inheritdoc />
	public override int Weight => 1500;

	/// <inheritdoc />
	public override int Hindrance => 1;

	/// <inheritdoc />
	public override int SlashingProtection => 2;

	/// <inheritdoc />
	public override int PiercingProtection => 2;

	/// <inheritdoc />
	public override int BashingProtection => 2;

	/// <inheritdoc />
	public override int ProjectileProtection => 2;

	/// <inheritdoc />
	public override int ProtectionFromIce => 5;

	/// <summary>
	/// Initializes a new instance of the <see cref="LizardScales"/> class.
	/// </summary>
	public LizardScales() : base(248)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="LizardScales"/> class.
	/// </summary>
	public LizardScales(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200174)); /* [You are looking at] [a vest made from the scales of a lizard.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250097)); /* The vest appears quite ordinary. */
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