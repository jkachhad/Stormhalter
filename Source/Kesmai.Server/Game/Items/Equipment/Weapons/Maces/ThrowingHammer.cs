using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class ThrowingHammer : Mace
{
	/// <inheritdoc />
	public override int LabelNumber => 6000047;

	/// <inheritdoc />
	public override uint BasePrice => 20;

	/// <inheritdoc />
	public override int Weight => 2000;

	/// <inheritdoc />
	public override int Category => 2;

	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 8;

	/// <inheritdoc />
	public override int BaseArmorBonus => 1;

	/// <inheritdoc />
	public override Skill Skill => Skill.Mace;

	/// <inheritdoc />
	public override WeaponFlags Flags => WeaponFlags.QuickThrow | WeaponFlags.Throwable | WeaponFlags.Bashing;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="ThrowingHammer"/> class.
	/// </summary>
	public ThrowingHammer() : base(75)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ThrowingHammer"/> class.
	/// </summary>
	public ThrowingHammer(Serial serial) : base(serial)
	{
	}
	
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200138)); /* [You are looking at] [a large steel throwing hammer.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250079)); /* The hammer appears quite ordinary. */
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