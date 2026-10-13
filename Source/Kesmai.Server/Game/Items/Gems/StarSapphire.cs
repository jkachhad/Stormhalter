using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class StarSapphire : Gem
{
	/// <inheritdoc />
	public override int Weight => 5;

	/// <summary>
	/// Initializes a new instance of the <see cref="StarSapphire"/> class.
	/// </summary>
	public StarSapphire(IEntityProvider entityProvider, EntityDependencies entityDependencies, uint basePrice) : base(entityProvider, entityDependencies, 166, basePrice)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="StarSapphire"/> class.
	/// </summary>

	public StarSapphire(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200128)); /* [You are looking at] [a dazzling diamond.] */
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
