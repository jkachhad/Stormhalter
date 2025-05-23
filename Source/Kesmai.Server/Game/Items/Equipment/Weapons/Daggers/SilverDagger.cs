using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class SilverDagger : Dagger, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 350;

	/// <inheritdoc />
	public override int BaseAttackBonus => 2;

	/// <inheritdoc />
	public override WeaponFlags Flags => base.Flags | WeaponFlags.Silver;

	/// <inheritdoc />
	protected override int PoisonedItemId => 311;

	/// <summary>
	/// Initializes a new instance of the <see cref="SilverDagger"/> class.
	/// </summary>
	public SilverDagger() : base(172)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="SilverDagger"/> class.
	/// </summary>
	public SilverDagger(Serial serial) : base(serial)
	{
	}
	
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200156)); /* [You are looking at] [a silver dagger.] */

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