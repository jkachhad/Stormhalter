using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public partial class FineCrossbow : Crossbow
{
	/// <inheritdoc />
	public override uint BasePrice => 600;
		
	/// <inheritdoc />
	public override int BaseAttackBonus => 2;

	/// <inheritdoc />
	public override ShieldPenetration Penetration => ShieldPenetration.Light;

	/// <summary>
	/// Initializes a new instance of the <see cref="FineCrossbow"/> class.
	/// </summary>
	public FineCrossbow() : base(230)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="FineCrossbow"/> class.
	/// </summary>
	public FineCrossbow(Serial serial) : base(serial)
	{
	}
		
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200282)); /* [You are looking at] [a fine crossbow.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250080)); /* The combat adds for this weapon are +2. */
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