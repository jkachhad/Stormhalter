using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public partial class WoodenStaff : Staff
{
	/// <inheritdoc />
	public override uint BasePrice => 15;

	/// <inheritdoc />
	public override int Weight => 1400;

	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 8;

	/// <inheritdoc />
	public override int BaseArmorBonus => 2;

	/// <inheritdoc />
	public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Bashing;

	/// <summary>
	/// Initializes a new instance of the <see cref="WoodenStaff"/> class.
	/// </summary>
	public WoodenStaff() : base(102)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="WoodenStaff"/> class.
	/// </summary>
	public WoodenStaff(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200135)); /* [You are looking at] [a wooden staff.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250084)); /* The staff appears quite ordinary. */
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