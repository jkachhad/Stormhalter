using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class WoodenShield : Shield
{
	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 25;

	/// <summary>
	/// Gets the weight for this instance.
	/// </summary>
	public override int Weight => 1000;

	/// <summary>
	/// Gets the base armor bonus provided by this <see cref="IArmored" />.
	/// </summary>
	public override int BaseArmorBonus => 1;


	public override int ProjectileProtection => 1;

	/// <summary>
	/// Initializes a new instance of the <see cref="WoodenShield"/> class.
	/// </summary>
	public WoodenShield() : base(100)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="WoodenShield"/> class.
	/// </summary>
	public WoodenShield(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200031); /* [a small wooden shield.]*/

		if (Identified)
			yield return LocalizationEntry.Get(6250023); /* The shield appears quite ordinary. */
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
