using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class JadeAmulet : LocateAmulet
{
	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 400;

	/// <summary>
	/// Gets the weight.
	/// </summary>
	public override int Weight => 100;
		
	public JadeAmulet() : this(3)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="JadeAmulet"/> class.
	/// </summary>
	public JadeAmulet(int charges = 3) : base(119, charges)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="JadeAmulet"/> class.
	/// </summary>
	public JadeAmulet(Serial serial) : base(serial)
	{
	}
		
	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200068); /* [a gold chain with a jade pendant.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250010); /* The amulet contains the spell of Locate. */
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