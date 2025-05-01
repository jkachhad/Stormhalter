using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public partial class SpikedFlail : Flail
{
	/// <inheritdoc />
	public override int LabelNumber => 6000063;

	/// <inheritdoc />
	public override uint BasePrice => 35;

	/// <inheritdoc />
	public override int Weight => 1800;
		
	/// <inheritdoc />
	public override int MinimumDamage => 3;

	/// <inheritdoc />
	public override int MaximumDamage => 12;

	/// <inheritdoc />
	public override int BaseArmorBonus => 1;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="SpikedFlail"/> class.
	/// </summary>
	public SpikedFlail() : base(197)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="SpikedFlail"/> class.
	/// </summary>
	public SpikedFlail(Serial serial) : base(serial)
	{
	}
		
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200166)); /* [You are looking at] [a heavy flail with a huge spiked ball on a steel chain.] */

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