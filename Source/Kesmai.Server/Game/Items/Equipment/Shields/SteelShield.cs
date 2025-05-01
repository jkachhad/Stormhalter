using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class SteelShield : Shield
{
	/// <inheritdoc />
	public override uint BasePrice => 300;

	/// <inheritdoc />
	public override int Weight => 3000;

	/// <inheritdoc />
	public override int Category => 1;
		
	/// <inheritdoc />
	public override int BaseArmorBonus => 1;

	/// <inheritdoc />
	public override int ProjectileProtection => 3;

	/// <summary>
	/// Initializes a new instance of the <see cref="SteelShield"/> class.
	/// </summary>
	public SteelShield() : base(188)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="SteelShield"/> class.
	/// </summary>
	public SteelShield(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200033)); /* [You are looking at] [a steel shield.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250025)); /* The shield will protect you quite well. */
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