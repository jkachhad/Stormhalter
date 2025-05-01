using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public partial class OchreEgg : ItemEntity, ITreasure
{
	/// <inheritdoc />
	public override int LabelNumber => 6000033;

	/// <inheritdoc />
	public override uint BasePrice => 700;

	/// <inheritdoc />
	public override int Weight => 15;

	/// <inheritdoc />
	public override int Category => 3;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="OchreEgg"/> class.
	/// </summary>
	public OchreEgg() : base(169)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="OchreEgg"/> class.
	/// </summary>
	public OchreEgg(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200219)); /* [You are looking at] [a large ochre egg mottled with dark brown spots.] */
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