using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class TigerSkull : Helmet, ITreasure
{
	/// <inheritdoc />
	public override int LabelNumber => 6000085;

	/// <inheritdoc />
	public override uint BasePrice => 500;

	/// <inheritdoc />
	public override int Weight => 80;

	/// <inheritdoc />
	public override int ProtectionFromDaze => 7;
		
	/// <inheritdoc />
	public override int ProtectionFromFire => 0;
		
	/// <inheritdoc />
	public override int ProtectionFromIce => 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="TigerSkull"/> class.
	/// </summary>
	public TigerSkull() : base(36)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="TigerSkull"/> class.
	/// </summary>
	public TigerSkull(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200212); /* [a tiger skull.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250107); /* The skull is fairly thick. */
	}

	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1); /* version */
	}

	/// <summary>
	/// Deserializes this instance from persisted binary data.
	/// </summary>
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
