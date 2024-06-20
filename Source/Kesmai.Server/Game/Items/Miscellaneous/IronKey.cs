﻿using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public partial class IronKey : MeleeWeapon, ITreasure
{
	/// <inheritdoc />
	public override int LabelNumber => 6000055;

	/// <inheritdoc />
	public override uint BasePrice => 700;

	/// <inheritdoc />
	public override int Weight => 15;

	/// <inheritdoc />
	public override int Category => 14;
	
	public override int MinimumDamage => 1;
	
	public override int MaximumDamage => 1;
	
	public override ShieldPenetration Penetration => ShieldPenetration.Full;
	
	public override Skill Skill => Skill.Thieving;
	
	public override WeaponFlags Flags => WeaponFlags.Bashing | WeaponFlags.Neutral;

	/// <summary>
	/// Initializes a new instance of the <see cref="IronKey"/> class.
	/// </summary>
	public IronKey() : base(284)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="IronKey"/> class.
	/// </summary>
	public IronKey(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200333)); /* [You are looking at] [an iron key with silver runes.] */
	}
		
	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(BinaryWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1);	/* version */
	}

	/// <summary>
	/// Deserializes this instance from persisted binary data.
	/// </summary>
	public override void Deserialize(BinaryReader reader)
	{
		base.Deserialize(reader);

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