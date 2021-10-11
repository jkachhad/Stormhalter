using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class SilverGreatAxe : Axe, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1;

		/// <inheritdoc />
		public override int Weight => 5760;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.VeryHeavy;

		/// <inheritdoc />
		public override int MinimumDamage => 4;

		/// <inheritdoc />
		public override int MaximumDamage => 14;

		/// <inheritdoc />
		public override int BaseArmorBonus => 1;

		/// <inheritdoc />
		public override int BaseAttackBonus => 4;

		/// <inheritdoc />
		public override Skill Skill => Skill.Greatsword;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Silver | WeaponFlags.Slashing 
		                                     | WeaponFlags.Lawful | WeaponFlags.BlueGlowing;

        /// <inheritdoc />
		public override bool CanBind => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="SilverGreatAxe"/> class.
		/// </summary>
		public SilverGreatAxe() : base(125)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200145)); /* [You are looking at] [a huge battle axe with shining silver blades. The axe is emitting a faint blue glow. The weapon is lawful.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250003)); /* The combat adds for this weapon are +4. */
		}
	}
}