using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class ReturningAxe : Axe, IReturningWeapon, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 1;

	/// <inheritdoc />
	public override int Weight => 2560;

	/// <inheritdoc />
	public override ShieldPenetration Penetration => ShieldPenetration.VeryHeavy;

	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 8;

	/// <inheritdoc />
	public override int BaseArmorBonus => 2;

	/// <inheritdoc />
	public override int BaseAttackBonus => 5;

	/// <inheritdoc />
	public override WeaponFlags Flags => WeaponFlags.BlueGlowing | WeaponFlags.Returning | WeaponFlags.Throwable 
	                                     | WeaponFlags.Slashing | WeaponFlags.Lawful;

	/// <inheritdoc />
	public override bool CanBind => true;

	/// <summary>
	/// Initializes a new instance of the <see cref="ReturningAxe"/> class.
	/// </summary>
	public ReturningAxe() : base(77)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ReturningAxe"/> class.
	/// </summary>
	public ReturningAxe(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200003)); /* [You are looking at] [a heavy battle axe with gleaming steel blades. The axe is emitting a faint blue glow. The weapon is lawful.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250002)); /* The combat adds for this weapon are +5. */
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