using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class Kimono : Robe, ITreasure
{
	/// <inheritdoc />
	public override int LabelNumber => 6000056;

	/// <inheritdoc />
	public override uint BasePrice => 2400;
		
	/// <inheritdoc />
	public override int Weight => 1000;
		
	/// <inheritdoc />
	/// <remarks>Robes have a default <see cref="Hindrance"/> value of 1.</remarks>
	public override int Hindrance => 0;
		
	/// <inheritdoc />
	public override int ProtectionFromFire => 10;
		
	/// <inheritdoc />
	public override int ProtectionFromIce => 10;

	/// <summary>
	/// Initializes a new instance of the <see cref="Kimono"/> class.
	/// </summary>
	public Kimono() : base(269)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Kimono"/> class.
	/// </summary>
	public Kimono(Serial serial) : base(serial)
	{
	}
		
	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200202); /* [a satiny kimono of brilliant green silk in which golden threads weave a pattern of tiny dragons.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250101); /* The kimono is extremely light. */
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
