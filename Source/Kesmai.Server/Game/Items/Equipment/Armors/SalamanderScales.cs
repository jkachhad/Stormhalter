using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class SalamanderScales : Armor, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000096; /* vest */

		/// <inheritdoc />
		public override uint BasePrice => 40;

		/// <inheritdoc />
		public override int Weight => 1500;

		/// <inheritdoc />
		public override int Hindrance => 1;

		/// <inheritdoc />
		public override int SlashingProtection => 1;

		/// <inheritdoc />
		public override int PiercingProtection => 1;

		/// <inheritdoc />
		public override int BashingProtection => 1;

		/// <inheritdoc />
		public override int ProtectionFromFire => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="SalamanderScales"/> class.
		/// </summary>
		public SalamanderScales() : base(250)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200176)); /* [You are looking at] [a vest made from the scales of a salamander.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250098)); /* The vest appears to have some magical properties. */
		}
	}
}
