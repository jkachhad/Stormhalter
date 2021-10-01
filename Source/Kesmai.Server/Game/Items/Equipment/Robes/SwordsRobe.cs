using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class SwordsRobe : Robe, ITreasure
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
		/// Initializes a new instance of the <see cref="SwordsRobe"/> class.
		/// </summary>
		public SwordsRobe() : base(295)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200205)); /* [You are looking at] [a blue and black robe adorned with silver swords and the circular emblem of the Ghods.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250030)); /* The robe looks thick and heavy. */
		}
	}
}