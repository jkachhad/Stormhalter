using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class ManticoraJacket : Jacket
	{
        /// <inheritdoc />
		public override uint BasePrice => 20;

        /// <inheritdoc />
		public override int Weight => 1400;

        /// <inheritdoc />
		public override int Hindrance => 1;

        /// <summary>
		/// Initializes a new instance of the <see cref="ManticoraJacket"/> class.
		/// </summary>
		public ManticoraJacket() : base(272)
		{
		}

        /// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200180)); /* [You are looking at] [a long coat made from the fur of a manticora.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250099)); /* The jacket appears quite ordinary. */
		}
	}
}
