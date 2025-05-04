using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class SpikedClub : Mace
{
	/// <inheritdoc />
	public override uint BasePrice => 20;

	/// <inheritdoc />
	public override int Weight => 4000;

	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 10;

	/// <inheritdoc />
	public override int BaseArmorBonus => 1;

	/// <inheritdoc />
	public override int BaseAttackBonus => 2;

	/// <inheritdoc />
	public override Skill Skill => Skill.Mace;

	/// <inheritdoc />
	public override WeaponFlags Flags => WeaponFlags.Bashing;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="SpikedClub"/> class.
	/// </summary>
	public SpikedClub() : base(86)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="SpikedClub"/> class.
	/// </summary>
	public SpikedClub(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200139)); /* [You are looking at] [a huge wooden club with a metal spike.] */

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