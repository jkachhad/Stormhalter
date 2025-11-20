using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class OysterPearl : Gem
{
	/// <inheritdoc />
	public override int LabelNumber => 6000067;
		
	/// <inheritdoc />
	public override int Weight => 5;

	/// <summary>
	/// Initializes a new instance of the <see cref="OysterPearl"/> class.
	/// </summary>
	public OysterPearl(uint basePrice) : base(143, basePrice)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="OysterPearl"/> class.
	/// </summary>
	public OysterPearl(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200229)); /* [You are looking at] [an oyster with a shiny pearl in it.] */
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