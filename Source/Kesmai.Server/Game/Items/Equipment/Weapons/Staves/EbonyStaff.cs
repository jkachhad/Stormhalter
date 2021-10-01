using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class EbonyStaff : Staff, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000088;

		/// <inheritdoc />
		public override uint BasePrice => 1;

		/// <inheritdoc />
		public override int Weight => 1400;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.VeryHeavy;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 8;

		/// <inheritdoc />
		public override int BaseArmorBonus => 4;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Bashing | WeaponFlags.Lawful;

        /// <inheritdoc />
		public override bool CanBind => true;

        /// <inheritdoc />
        public override int ManaRegeneration => 1;

        /// <summary>
		/// Initializes a new instance of the <see cref="EbonyStaff"/> class.
		/// </summary>
		public EbonyStaff() : base(307)
		{
		}

        /// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200163)); /* [You are looking at] [an ebony staff encrusted with diamonds.  The weapon eminates power. The staff is emitting a faint blue glow. The weapon is lawful.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250092)); /* The staff seems to have some magical properties. */
		}
	}
}