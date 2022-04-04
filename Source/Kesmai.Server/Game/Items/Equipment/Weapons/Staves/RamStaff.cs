using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class RamStaff : Staff
	{
        /// <inheritdoc />
        public override int LabelNumber => 6000088;

		/// <inheritdoc />
		public override uint BasePrice => 800;

		/// <inheritdoc />
		public override int Weight => 1400;

        /// <inheritdoc />
        public override ShieldPenetration Penetration => ShieldPenetration.Medium;

		/// <inheritdoc />
		public override int Category => 1;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 8;

        /// <inheritdoc />
        public override bool CanBind => true;

		/// <inheritdoc />
		public override int BaseArmorBonus => 4;

		/// <inheritdoc />
		public override int BaseAttackBonus => 2;

        /// <inheritdoc />
        public override int ProjectileProtection  => 1;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Bashing | WeaponFlags.Lawful;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="RamStaff"/> class.
		/// </summary>
		public RamStaff() : base(901)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200353)); /* [You are looking at] [a staff large gnarled staff with a ram skull attached.] */

            if (Identified)
				entries.Add(new LocalizationEntry(6250080)); /* The combat adds for this weapon are +2. */
		}
	}
}