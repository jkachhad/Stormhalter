using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class SharkJacket : Jacket
	{
		/// <inheritdoc />
		public override uint BasePrice => 20;

        /// <inheritdoc />
		public override int Weight => 1200;

        /// <inheritdoc />
		public override int Hindrance => 1;

        /// <summary>
		/// Initializes a new instance of the <see cref="SharkJacket"/> class.
		/// </summary>
		public SharkJacket() : base(270)
		{
		}

        /// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200179)); /* [You are looking at] [a jacket made from the skin of a shark.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250099)); /* The jacket appears quite ordinary. */
		}
	}
}
