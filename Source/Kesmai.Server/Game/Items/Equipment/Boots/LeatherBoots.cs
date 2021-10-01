using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class LeatherBoots : Boots
	{
		/// <inheritdoc />
		public override uint BasePrice => 20;

		/// <inheritdoc />
		public override int Weight => 480;

		/// <inheritdoc />
		public override int Hindrance => 4;

		/// <summary>
		/// Initializes a new instance of the <see cref="LeatherBoots"/> class.
		/// </summary>
		public LeatherBoots() : base(121)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200206)); /* [You are looking at] [a pair of soft leather boots.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250102)); /* The boots appear quite ordinary. */
		}
	}
}