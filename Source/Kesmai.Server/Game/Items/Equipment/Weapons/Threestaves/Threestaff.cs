using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public abstract partial class Threestaff : MeleeWeapon
{
	/// <inheritdoc />
	public override int LabelNumber => 6000060;

	/// <inheritdoc />
	public override int Category => 1;

	/// <inheritdoc />
	public override ShieldPenetration Penetration => ShieldPenetration.Light;

	/// <inheritdoc />
	public override Skill Skill => Skill.Threestaff;

	/// <summary>
	/// Initializes a new instance of the <see cref="Threestaff"/> class.
	/// </summary>
	protected Threestaff(int ThreestaffID) : base(ThreestaffID)
	{
	}
}