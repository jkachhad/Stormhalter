using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class LeatherJacket : Jacket
	{
        /// <inheritdoc />
		public override uint BasePrice => 20;

        /// <inheritdoc />
		public override int Weight => 1200;

        /// <inheritdoc />
		public override int Hindrance => 1;

        /// <summary>
		/// Initializes a new instance of the <see cref="LeatherJacket"/> class.
		/// </summary>
		public LeatherJacket() : base(244)
		{
		}

        /// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200012)); /* [You are looking at] [a jacket made of a strange leather.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250011)); /* The jacket is nothing special. */
		}
	}
}
