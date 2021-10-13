using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class BearJacket : Jacket
	{
		/// <inheritdoc />
		public override uint BasePrice => 20;

		/// <inheritdoc />
		public override int Weight => 1600;

		/// <summary>
		/// Initializes a new instance of the <see cref="BearJacket"/> class.
		/// </summary>
		public BearJacket() : base(271)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200203)); /* [You are looking at] [a coat made from the hide of a single huge bear.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250099)); /* The jacket appears quite ordinary. */
		}
	}
}