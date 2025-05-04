using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class Tourmaline : Gem
{
	/// <inheritdoc />
	public override int Weight => 5;

	/// <summary>
	/// Gets the label number.
	/// </summary>
	public override int LabelNumber => 6000007;

	/// <summary>
	/// Initializes a new instance of the <see cref="Tourmaline"/> class.
	/// </summary>
	[WorldForge]
	public Tourmaline(uint basePrice) : base(62, basePrice)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TigerEye"/> class.
	/// </summary>
	public Tourmaline(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200118)); /* [You are looking at] [a rainbow hued bar of tourmaline.] */
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