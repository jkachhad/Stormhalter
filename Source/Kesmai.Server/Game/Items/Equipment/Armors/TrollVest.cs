using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class TrollVest : Armor
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000096; /* vest */

		/// <inheritdoc />
		public override uint BasePrice => 150;

		/// <inheritdoc />
		public override int Weight => 1800;

		/// <inheritdoc />
		public override int Hindrance => 1;

		/// <inheritdoc />
		public override int SlashingProtection => 2;

		/// <inheritdoc />
		public override int PiercingProtection => 2;

		/// <inheritdoc />
		public override int BashingProtection => 2;

		/// <inheritdoc />
		public override int ProjectileProtection => 2;

		/// <summary>
		/// Initializes a new instance of the <see cref="TrollVest"/> class.
		/// </summary>
		public TrollVest() : base(251)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200025)); /* [You are looking at] [a vest made from a peculiar stone colored leather.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250021)); /* The vest appears quite thick. */
		}
	}
}
