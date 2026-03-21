using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class PlatedShield : Shield
{
	/// <inheritdoc />
	public override uint BasePrice => 150;

	/// <inheritdoc />
	public override int Weight => 2000;

	/// <inheritdoc />
	public override int Category => 1;
		
	/// <inheritdoc />
	public override int BaseArmorBonus => 1;

	/// <inheritdoc />
	public override int ProjectileProtection => 2;

	/// <summary>
	/// Initializes a new instance of the <see cref="PlatedShield"/> class.
	/// </summary>
	public PlatedShield() : base(76)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="PlatedShield"/> class.
	/// </summary>
	public PlatedShield(Serial serial) : base(serial)
	{
	}
		
	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200032); /* [a wooden shield faced with thin steel plates.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250024); /* The shield provides fairly good protection. */
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
