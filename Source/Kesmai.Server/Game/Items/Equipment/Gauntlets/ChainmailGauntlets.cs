using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class ChainmailGauntlets : Gauntlets, ITreasure
{
	/// <inheritdoc />
	public override int LabelNumber => 6000041;

	/// <inheritdoc />
	public override uint BasePrice => 75;

	/// <inheritdoc />
	public override int Weight => 200;

	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 4;

	/// <summary>
	/// Initializes a new instance of the <see cref="ChainmailGauntlets"/> class.
	/// </summary>
	public ChainmailGauntlets() : base(23)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ChainmailGauntlets"/> class.
	/// </summary>
	public ChainmailGauntlets(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200168)); /* [You are looking at] [a pair of chainmail gauntlets.] */
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