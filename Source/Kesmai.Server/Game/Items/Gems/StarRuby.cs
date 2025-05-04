using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class StarRuby : Gem
{
	/// <inheritdoc />
	public override int Weight => 5;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="StarRuby"/> class.
	/// </summary>
	public StarRuby() : this(500u)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="StarRuby"/> class.
	/// </summary>
	[WorldForge]
	public StarRuby(uint basePrice) : base(90, basePrice)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="StarRuby"/> class.
	/// </summary>
	public StarRuby(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200121)); /* [You are looking at] [a highly polished star ruby with a peculiar glow eminating from its core.] */
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