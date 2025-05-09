using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class WoodenFlail : Flail
{
	/// <inheritdoc />
	public override uint BasePrice => 25;

	/// <inheritdoc />
	public override int Weight => 1800;
		
	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 10;

	/// <inheritdoc />
	public override int BaseArmorBonus => 1;

	/// <summary>
	/// Initializes a new instance of the <see cref="WoodenFlail"/> class.
	/// </summary>
	public WoodenFlail() : base(183)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="WoodenFlail"/> class.
	/// </summary>
	public WoodenFlail(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200158)); /* [You are looking at] [a wooden flail.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250090)); /* The flail appears quite ordinary. */
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