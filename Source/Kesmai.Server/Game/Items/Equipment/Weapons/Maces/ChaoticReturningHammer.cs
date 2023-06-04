using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public partial class ChaoticReturningHammer : Mace, IReturningWeapon, ITreasure
{
	/// <inheritdoc />
	public override int LabelNumber => 6000047;

	/// <inheritdoc />
	public override uint BasePrice => 1;

	/// <inheritdoc />
	public override int Weight => 2000;

	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 8;

	/// <inheritdoc />
	public override int BaseArmorBonus => 2;

	/// <inheritdoc />
	public override int BaseAttackBonus => 3;

	/// <inheritdoc />
	public override Skill Skill => Skill.Mace;

	/// <inheritdoc />
	public override WeaponFlags Flags => WeaponFlags.Returning | WeaponFlags.Throwable
	                                                           | WeaponFlags.Bashing;

	/// <inheritdoc />
	public override bool CanBind => true;

	/// <summary>
	/// Initializes a new instance of the <see cref="ReturningHammer"/> class.
	/// </summary>
	public ChaoticReturningHammer() : base(75)
	{
	}
	public ChaoticReturningHammer(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200386)); /* [You are looking at] [a large steel throwing hammer. The hammer is emitting a soft yellow glow. The weapon is chaotic.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250007)); /* The combat adds for this weapon are +3. */
	}
}