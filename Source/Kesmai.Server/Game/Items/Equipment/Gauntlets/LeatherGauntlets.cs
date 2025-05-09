using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class LeatherGauntlets : Gauntlets, ITreasure
{
	/// <inheritdoc />
	public override int LabelNumber => 6000041;

	/// <inheritdoc />
	public override uint BasePrice => 200;

	/// <inheritdoc />
	public override int Weight => 200;

	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 4;

	/// <inheritdoc />
	public override int BaseAttackBonus => 1;

	/// <summary>
	/// Initializes a new instance of the <see cref="LeatherGauntlets"/> class.
	/// </summary>
	public LeatherGauntlets() : base(1)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="LeatherGauntlets"/> class.
	/// </summary>
	public LeatherGauntlets(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200009)); /* [You are looking at] [a pair of leather gauntlets.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250008)); /* The combat adds for these gauntlets are +1. */
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