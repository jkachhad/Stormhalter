using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class YasnakiDagger : Dagger, IReturningWeapon, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1;

		/// <inheritdoc />
		public override int BaseAttackBonus => 5;

		/// <inheritdoc />
		public override WeaponFlags Flags => base.Flags | WeaponFlags.Silver | WeaponFlags.Returning;

		/// <inheritdoc />
		public override bool CanBind => true;

		/// <inheritdoc />
		protected override int PoisonedItemId => 314;

		/// <summary>
		/// Initializes a new instance of the <see cref="YasnakiDagger"/> class.
		/// </summary>
		public YasnakiDagger() : base(308)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200164)); /* [You are looking at] [a fine throwing dagger with the symbol of the yasnaki marking the hilt.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250002)); /* The combat adds for this weapon are +5. */
		}
	}
}