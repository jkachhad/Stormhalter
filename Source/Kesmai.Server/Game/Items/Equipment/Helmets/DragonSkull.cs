using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class DragonSkull : Helmet, ITreasure
{
	/// <inheritdoc />
	public override int LabelNumber => 6000085;

	/// <inheritdoc />
	public override uint BasePrice => 1000;

	/// <inheritdoc />
	public override int Weight => 80;

	/// <inheritdoc />
	public override int ProtectionFromDaze => 10;
		
	/// <inheritdoc />
	public override int ProtectionFromFire => 15;
		
	/// <inheritdoc />
	public override int ProtectionFromIce => 15;
		
	/// <inheritdoc />
	public override bool ProvidesNightVision => true;

	/// <summary>
	/// Initializes a new instance of the <see cref="DragonSkull"/> class.
	/// </summary>
	public DragonSkull() : base(47)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="DragonSkull"/> class.
	/// </summary>
	public DragonSkull(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200211); /* [a dragon skull.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250106); /* The skull is very thick and seems to have some magical properties. */
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
