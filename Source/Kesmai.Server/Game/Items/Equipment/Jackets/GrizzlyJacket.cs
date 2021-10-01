using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class GrizzlyJacket : Jacket
	{
		/// <inheritdoc />
		public override uint BasePrice => 40;

        /// <inheritdoc />
		public override int Weight => 2000;
        
		/// <summary>
		/// Initializes a new instance of the <see cref="GrizzlyJacket"/> class.
		/// </summary>
		public GrizzlyJacket() : base(273)
		{
		}

        /// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200204)); /* [You are looking at] [a coat made from the skin of an enormous grizzly bear.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250099)); /* The jacket appears quite ordinary. */
		}
	}
}
