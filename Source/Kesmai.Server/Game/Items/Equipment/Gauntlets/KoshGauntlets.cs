using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public partial class KoshGauntlets : Gauntlets, ITreasure
{
	/// <inheritdoc />
	public override int LabelNumber => 6000041;

	/// <inheritdoc />
	public override uint BasePrice => 1;

	/// <inheritdoc />
	public override int Weight => 200;

	/// <inheritdoc />
	public override ShieldPenetration Penetration => ShieldPenetration.Heavy;

	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 4;

	/// <inheritdoc />
	public override int BaseArmorBonus => 1;

	/// <inheritdoc />
	public override int BaseAttackBonus => 2;

	/// <inheritdoc />
	public override WeaponFlags Flags => base.Flags | WeaponFlags.BlueGlowing;

	/// <inheritdoc />
	public override bool CanBind => true;

	/// <summary>
	/// Initializes a new instance of the <see cref="KoshGauntlets"/> class.
	/// </summary>
	public KoshGauntlets() : base(69)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="KoshGauntlets"/> class.
	/// </summary>
	public KoshGauntlets(Serial serial) : base(serial)
	{
	}
	
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200342)); /* [You are looking at] [a pair of gauntlets made from leathery dragon skin with shiny blue-steel plates. The Gauntlets emit a Blue Glow.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250094)); /* The combat adds for the gauntlets are +2. */
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