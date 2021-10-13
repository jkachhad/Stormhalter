using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class WaveRobe : Robe, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 2400;

		/// <inheritdoc />
		public override int Weight => 1600;

		/// <inheritdoc />
		public override int ProtectionFromFire => 5;

		/// <inheritdoc />
		public override int ProtectionFromIce => 5;

		/// <inheritdoc />
		public override int ManaRegeneration => 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="WaveRobe"/> class.
		/// </summary>
		public WaveRobe() : base(263)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200198)); /* [You are looking at] [a blue robe with a cresting wave.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250030)); /* The robe looks thick and heavy. */
		}
	}
}