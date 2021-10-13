using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class BlackStaff : Staff
	{
		/// <inheritdoc />
		public override uint BasePrice => 800;

		/// <inheritdoc />
		public override int Weight => 1400;

		/// <inheritdoc />
		public override int Category => 1;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 8;

		/// <inheritdoc />
		public override int BaseArmorBonus => 1;

		/// <inheritdoc />
		public override int BaseAttackBonus => 1;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Bashing;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="BlackStaff"/> class.
		/// </summary>
		public BlackStaff() : base(144)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200147)); /* [You are looking at] [a large black staff covered with mystic runes.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250085)); /* The combat adds for this weapon are +1. */
		}
	}
}