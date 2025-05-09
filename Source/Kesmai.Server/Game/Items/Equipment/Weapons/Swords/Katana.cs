﻿using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class Katana : Sword
{
	/// <inheritdoc />
	public override int LabelNumber => 6000054;

	/// <inheritdoc />
	public override uint BasePrice => 30;

	/// <inheritdoc />
	public override int Weight => 1450;

	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 8;

	/// <inheritdoc />
	public override int BaseArmorBonus => 1;

	/// <inheritdoc />
	public override Skill Skill => Skill.Sword;

	/// <inheritdoc />
	public override WeaponFlags Flags => WeaponFlags.Slashing;

	/// <summary>
	/// Initializes a new instance of the <see cref="Katana"/> class.
	/// </summary>
	public Katana() : base(148)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Katana"/> class.
	/// </summary>
	public Katana(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200150)); /* [You are looking at] [a sharp steel katana.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250088)); /* The katana appears quite ordinary. */
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