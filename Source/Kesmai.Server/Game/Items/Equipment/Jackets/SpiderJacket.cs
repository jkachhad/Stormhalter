using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class SpiderJacket : Jacket
	{
        /// <inheritdoc />
		public override uint BasePrice => 20;

        /// <inheritdoc />
		public override int Weight => 1300;

        /// <summary>
		/// Initializes a new instance of the <see cref="SpiderJacket"/> class.
		/// </summary>
		public SpiderJacket() : base(267)
		{
		}
        
        /// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200201)); /* [You are looking at] [a black jacket made from the fur of a spider.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250099)); /* The jacket appears quite ordinary. */
		}
	}
}
