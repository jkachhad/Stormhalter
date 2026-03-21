using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Miscellaneous;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

[TypeAlias("Kesmai.Server.Internal.AxeGlacier.Cache+MoonstoneRing")]
public class MoonstoneRing : Ring, ITreasure
{
	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 100;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="MoonstoneRing"/> class.
	/// </summary>
	public MoonstoneRing() : base(57)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MoonstoneRing"/> class.
	/// </summary>
	public MoonstoneRing(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200047); /* [a silver ring set with a pale, milky moonstone.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250039); /* The ring appears to be nothing special. */
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
