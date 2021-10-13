using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class HyenaJacket : Jacket
	{
        /// <inheritdoc />
		public override uint BasePrice => 20;

        /// <inheritdoc />
		public override int Weight => 1300;

        /// <inheritdoc />
		public override int Category => 10;

        /// <summary>
		/// Initializes a new instance of the <see cref="HyenaJacket"/> class.
		/// </summary>
		public HyenaJacket() : base(258)
		{
		}
        
        /// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200194)); /* [You are looking at] [a jacket made from the skin of a hyena.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250099)); /* The jacket appears quite ordinary. */
		}
	}
}
