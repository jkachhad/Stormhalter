using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class SpectreCloak : Robe, ITreasure
{
	/// <inheritdoc />
	public override int LabelNumber => 6000026;
		
	/// <inheritdoc />
	public override uint BasePrice => 2400;

	/// <inheritdoc />
	public override int Weight => 1000;

	/// <inheritdoc />
	/// <remarks>Robes have a default <see cref="Hindrance"/> value of 1.</remarks>
	public override int Hindrance => 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="SpectreCloak"/> class.
	/// </summary>
	public SpectreCloak() : base(252)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="SpectreCloak"/> class.
	/// </summary>
	public SpectreCloak(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, "an experimental cloak.")); /* [You are looking at] [a long black cloak.] */

		if (Identified)
			entries.Add(new LocalizationEntry("The robe is visible when inspected closely."));
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