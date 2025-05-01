using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class SwordOfLight : Sword, ITreasure
{
		
	/// <inheritdoc />
	/// <remarks>
	/// The individual components give 1700. If the item were to be broken by a player,
	/// it would award better experience.
	/// </remarks>
	public override uint BasePrice => 1000;

	/// <inheritdoc />
	public override int Weight => 4160;
		
	/// <inheritdoc />
	public override ShieldPenetration Penetration => ShieldPenetration.VeryHeavy;

	/// <inheritdoc />
	public override int MinimumDamage => 15;

	/// <inheritdoc />
	public override int MaximumDamage => 25;

	/// <inheritdoc />
	public override int BaseArmorBonus => 4;

	/// <inheritdoc />
	public override int BaseAttackBonus => 6;

	/// <inheritdoc />
	public override Skill Skill => Skill.Greatsword;

	/// <inheritdoc />
	public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.BlueGlowing | WeaponFlags.Slashing | WeaponFlags.Lawful;

	/// <inheritdoc />
	public override bool CanBind => true;

	/// <summary>
	/// Initializes a new instance of the <see cref="SwordOfLight"/> class.
	/// </summary>
	public SwordOfLight() : base(331)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SwordOfLight"/> class.
	/// </summary>
	public SwordOfLight(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200379)); /* [You are looking at] [a sword that shines as bright as the sun.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6300426)); /* The combat adds for this weapon are +6. */
	}
}