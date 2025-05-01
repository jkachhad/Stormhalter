using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class HyenaJacket : Jacket
{
	/// <inheritdoc />
	public override uint BasePrice => 20;

	/// <inheritdoc />
	public override int Weight => 1300;

	/// <inheritdoc />
	public override int Category => 10;

	/// <summary>
	/// Initializes a new instance of the <see cref="HyenaJacket"/> class.
	/// </summary>
	public HyenaJacket() : base(258)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="HyenaJacket"/> class.
	/// </summary>
	public HyenaJacket(Serial serial) : base(serial)
	{
	}
        
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200194)); /* [You are looking at] [a jacket made from the skin of a hyena.] */

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