using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class LargePinkPearl : Gem
{
	/// <inheritdoc />
	public override int Weight => 5;

	/// <summary>
	/// Gets the label number.
	/// </summary>
	public override int LabelNumber => 6000069;

	/// <summary>
	/// Initializes a new instance of the <see cref="LargePinkPearl"/> class.
	/// </summary>
	[WorldForge]
	public LargePinkPearl(uint basePrice) : base(351, basePrice)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="LargePinkPearl"/> class.
	/// </summary>
	public LargePinkPearl(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200297)); /* [You are looking at] [an enormous pink pearl.] */
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