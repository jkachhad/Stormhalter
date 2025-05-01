using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class IceDragonScaleArmor : Armor, ITreasure
{
	/// <inheritdoc />
	public override int LabelNumber => 6000076; /* scales */

	/// <inheritdoc />
	public override uint BasePrice => 3200;

	/// <inheritdoc />
	public override int Weight => 1600;

	/// <inheritdoc />
	public override int Hindrance => 2;

	/// <inheritdoc />
	public override int SlashingProtection => 5;

	/// <inheritdoc />
	public override int PiercingProtection => 5;

	/// <inheritdoc />
	public override int BashingProtection => 5;

	/// <inheritdoc />
	public override int ProjectileProtection => 5;


	/// <inheritdoc />
	public override int ProtectionFromConcussion => 30;
		
	/// <inheritdoc />
	public override int ProtectionFromFire => 15;

	/// <inheritdoc />
	public override int ProtectionFromIce => 15;

	/// <summary>
	/// Initializes a new instance of the <see cref="IceDragonScaleArmor"/> class.
	/// </summary>
	public IceDragonScaleArmor() : base(219)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="IceDragonScaleArmor"/> class.
	/// </summary>
	public IceDragonScaleArmor(Serial serial) : base(serial)
	{
	}
		
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200181)); /* [You are looking at] [a vest made from the milky white scales of an ice dragon.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250096)); /* The armor appears to have some magical properties. */
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