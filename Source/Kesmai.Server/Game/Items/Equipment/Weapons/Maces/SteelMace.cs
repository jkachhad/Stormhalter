using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class SteelMace : Mace
{
	/// <inheritdoc />
	public override uint BasePrice => 25;

	/// <inheritdoc />
	public override int Weight => 1800;

	/// <inheritdoc />
	public override ShieldPenetration Penetration => ShieldPenetration.Medium;

	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 6;

	/// <inheritdoc />
	public override int BaseArmorBonus => 1;

	/// <inheritdoc />
	public override Skill Skill => Skill.Mace;

	/// <inheritdoc />
	public override WeaponFlags Flags => WeaponFlags.Bashing;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="SteelMace"/> class.
	/// </summary>
	public SteelMace() : base(89)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="SteelMace"/> class.
	/// </summary>
	public SteelMace(Serial serial) : base(serial)
	{
	}
	
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200018)); /* [You are looking at] [a steel mace.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250017)); /* The mace appears quite ordinary. */
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