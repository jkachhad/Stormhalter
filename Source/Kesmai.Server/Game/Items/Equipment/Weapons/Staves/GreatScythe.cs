using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class GreatScythe : Staff
	{
		/// <inheritdoc />
		public override uint BasePrice => 10000;

		/// <inheritdoc />
		public override int Weight => 1400;

        /// <inheritdoc />
        public override ShieldPenetration Penetration => ShieldPenetration.VeryHeavy;

        /// <inheritdoc />
		public override int MinimumDamage => 8;

		/// <inheritdoc />
		public override int MaximumDamage => 15;

        /// <inheritdoc />
        public override bool CanBind => true;

		/// <inheritdoc />
		public override int BaseArmorBonus => 3;

		/// <inheritdoc />
		public override int BaseAttackBonus => 6;

        /// <inheritdoc />
        public override int ProjectileProtection  => 1;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Bashing | WeaponFlags.Lawful;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="GreatScythe"/> class.
		/// </summary>
		public GreatScythe() : base(339)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="GreatScythe"/> class.
		/// </summary>
		public GreatScythe(Serial serial) : base(serial)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200354)); /* [You are looking at] [a bloodied great scythe with magical runes embued within.] */

            if (Identified)
				entries.Add(new LocalizationEntry(6300426)); /* The combat adds for this weapon are +6. */
        }
	}
}