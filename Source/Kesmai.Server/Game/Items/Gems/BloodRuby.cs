using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class BloodRuby : Gem
{
	/// <inheritdoc />
	public override int Weight => 5;
			
	/// <summary>
	/// Initializes a new instance of the <see cref="BloodRuby"/> class.
	/// </summary>
	public BloodRuby(uint basePrice) : base(164, basePrice)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="BloodRuby"/> class.
	/// </summary>
	public BloodRuby(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200126)); /* [You are looking at] [a round, blood-red ruby.] */
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