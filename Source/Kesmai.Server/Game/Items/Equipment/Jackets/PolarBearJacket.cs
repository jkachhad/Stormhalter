using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class PolarBearJacket : Jacket, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 20;

	/// <inheritdoc />
	public override int Weight => 1600;

	/// <inheritdoc />
	public override int Hindrance => 0;
		
	/// <inheritdoc />
	public override int ProtectionFromIce => 10;

	/// <summary>
	/// Initializes a new instance of the <see cref="PolarBearJacket"/> class.
	/// </summary>
	public PolarBearJacket() : base(256)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="PolarBearJacket"/> class.
	/// </summary>
	public PolarBearJacket(Serial serial) : base(serial)
	{
	}
	
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200192)); /* [You are looking at] [a coat made from the fur of a huge white yeti.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250099)); /* The jacket appears quite ordinary. */
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