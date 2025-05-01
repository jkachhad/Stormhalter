using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class BlackSteelRapier : Rapier, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 1;

	/// <inheritdoc />
	public override ShieldPenetration Penetration => ShieldPenetration.Medium;

	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 10;
		
	/// <inheritdoc />
	public override int BaseAttackBonus => 4;

	public override int BaseArmorBonus => 3;

	/// <inheritdoc />
	public override WeaponFlags Flags => base.Flags | WeaponFlags.BlueGlowing;

	/// <inheritdoc />
	public override bool CanBind => true;

	/// <summary>
	/// Initializes a new instance of the <see cref="BlackSteelRapier"/> class.
	/// </summary>
	public BlackSteelRapier() : base(309)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="BlackSteelRapier"/> class.
	/// </summary>
	public BlackSteelRapier(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200280)); /* [You are looking at] [an impossibly thin black blade mounted on a silver hilt.  The rapier is emitting a faint blue glow.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250003)); /* The combat adds for this weapon are +4. */
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