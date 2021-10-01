using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class TigerJacket : Jacket
	{
        /// <inheritdoc />
		public override uint BasePrice => 20;

        /// <inheritdoc />
		public override int Weight => 1300;

        /// <summary>
		/// Initializes a new instance of the <see cref="TigerJacket"/> class.
		/// </summary>
		public TigerJacket() : base(255)
		{
		}

        /// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200191)); /* [You are looking at] [a coat made from the fur of a tiger.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250099)); /* The jacket appears quite ordinary. */
		}
	}
}
