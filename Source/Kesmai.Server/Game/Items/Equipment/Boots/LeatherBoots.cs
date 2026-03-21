using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class LeatherBoots : Boots
{
	/// <inheritdoc />
	public override uint BasePrice => 20;

	/// <inheritdoc />
	public override int Weight => 480;

	/// <inheritdoc />
	public override int Hindrance => 4;

	/// <summary>
	/// Initializes a new instance of the <see cref="LeatherBoots"/> class.
	/// </summary>
	public LeatherBoots() : base(121)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="LeatherBoots"/> class.
	/// </summary>
	public LeatherBoots(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200206); /* [a pair of soft leather boots.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250102); /* The boots appear quite ordinary. */
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
