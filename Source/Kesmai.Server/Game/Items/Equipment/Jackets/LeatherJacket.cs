using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class LeatherJacket : Jacket
{
	/// <inheritdoc />
	public override uint BasePrice => 20;

	/// <inheritdoc />
	public override int Weight => 1200;

	/// <inheritdoc />
	public override int Hindrance => 1;

	/// <summary>
	/// Initializes a new instance of the <see cref="LeatherJacket"/> class.
	/// </summary>
	public LeatherJacket() : base(244)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LeatherJacket"/> class.
	/// </summary>
	public LeatherJacket(Serial serial) : base(serial)
	{
	}
	
	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200012); /* [a jacket made of a strange leather.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250011); /* The jacket is nothing special. */
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
