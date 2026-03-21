using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class ChristmasTree : ItemEntity, ITreasure
{
	/// <inheritdoc />
	public override int LabelNumber => 6000093;

	/// <inheritdoc />
	public override uint BasePrice => 500000;

	/// <inheritdoc />
	public override int Weight => 15;

	/// <inheritdoc />
	public override int Category => 3;

	/// <summary>
	/// Initializes a new instance of the <see cref="ChristmasTree"/> class.
	/// </summary>
	public ChristmasTree() : base(324)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="PetrifiedWood"/> class.
	/// </summary>
	public ChristmasTree(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200339); /* [a miniature decorative holiday tree.] */
	}

	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1);	/* version */
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