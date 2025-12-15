using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class OvalRuby : Gem
{
	/// <inheritdoc />
	public override int Weight => 5;

	/// <summary>
	/// Initializes a new instance of the <see cref="OvalRuby"/> class.
	/// </summary>
	public OvalRuby(uint basePrice) : base(63, basePrice)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="OvalRuby"/> class.
	/// </summary>
	public OvalRuby(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200119)); /* [You are looking at] [an oval shaped ruby.] */
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