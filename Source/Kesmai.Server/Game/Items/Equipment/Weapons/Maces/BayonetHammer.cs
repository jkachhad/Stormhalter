using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class BayonetHammer : Mace
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000047;

		/// <inheritdoc />
		public override uint BasePrice => 30;

		/// <inheritdoc />
		public override int Weight => 2000;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 10;

		/// <inheritdoc />
		public override int BaseArmorBonus => 1;

		/// <inheritdoc />
		public override Skill Skill => Skill.Mace;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.Throwable | WeaponFlags.Slashing | WeaponFlags.Bashing;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="BayonetHammer"/> class.
		/// </summary>
		public BayonetHammer() : base(147)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200149)); /* [You are looking at] [a steel bayonet fitted with a hammer head.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250087)); /* The bayonet appears quite ordinary. */
		}
	}
}
