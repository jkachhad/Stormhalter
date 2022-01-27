using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class SpikedClub : Mace
	{
		/// <inheritdoc />
		public override uint BasePrice => 20;

		/// <inheritdoc />
		public override int Weight => 4000;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 10;

		/// <inheritdoc />
		public override int BaseArmorBonus => 1;

		/// <inheritdoc />
		public override Skill Skill => Skill.Mace;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.Bashing;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="SpikedClub"/> class.
		/// </summary>
		public SpikedClub() : base(86)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200139)); /* [You are looking at] [a huge wooden club with a metal spike.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250081)); /* The club appears quite ordinary. */
		}
	}
}