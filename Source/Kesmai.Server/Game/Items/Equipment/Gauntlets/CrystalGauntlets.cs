using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class CrystalGauntlets : Gauntlets, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000041;

        /// <inheritdoc />
		public override uint BasePrice => 1;

        /// <inheritdoc />
		public override int Weight => 200;

        /// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.VeryHeavy;

        /// <inheritdoc />
		public override int MinimumDamage => 1;

        /// <inheritdoc />
		public override int MaximumDamage => 6;

        /// <inheritdoc />
		public override int BaseArmorBonus => 3;

        /// <inheritdoc />
		public override int BaseAttackBonus => 4;

        /// <inheritdoc />
		public override WeaponFlags Flags => base.Flags | WeaponFlags.BlueGlowing | WeaponFlags.Lawful;

        /// <inheritdoc />
		public override bool CanBind => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="CrystalGauntlets"/> class.
		/// </summary>
		public CrystalGauntlets() : base(71)
		{
		}

        /// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200172)); /* [You are looking at] [a pair of gauntlets made of flat black steel lamallae with rosy crystalline claws projecting from the knuckles.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250095)); /* The combat adds for the gauntlets are +4. */
		}
	}
}