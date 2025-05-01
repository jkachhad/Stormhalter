using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public partial class FlawlessSapphire : Gem
{
	/// <inheritdoc />
	public override int Weight => 5;

	/// <summary>
	/// Initializes a new instance of the <see cref="FlawlessSapphire"/> class.
	/// </summary>
	[WorldForge]
	public FlawlessSapphire(uint basePrice) : base(177, basePrice)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="FlawlessSapphire"/> class.
	/// </summary>
	public FlawlessSapphire(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200131)); /* [You are looking at] [a huge flawless sapphire of deepest blue.] */
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