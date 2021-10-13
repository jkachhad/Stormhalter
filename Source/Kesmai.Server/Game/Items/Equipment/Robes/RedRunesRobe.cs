using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class RedRunesRobe : Robe, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 2400;

		/// <inheritdoc />
		public override int Weight => 1600;

		/// <inheritdoc />
		public override int Hindrance => 1;

		/// <inheritdoc />
		public override int ProtectionFromFire => 5;

		/// <inheritdoc />
		public override int ProtectionFromIce => 5;

		/// <inheritdoc />
		public override int ManaRegeneration => 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="RedRunesRobe"/> class.
		/// </summary>
		public RedRunesRobe() : base(240)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200187)); /* [You are looking at] [a heavy black robe with a border of red runes.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250030)); /* The robe looks thick and heavy. */
		}
	}
}