using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class WoodenShield : Shield
	{
		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 25;

		/// <summary>
		/// Gets the weight for this instance.
		/// </summary>
		public override int Weight => 1000;

		/// <summary>
		/// Gets the base armor bonus provided by this <see cref="IArmored" />.
		/// </summary>
		public override int BaseArmorBonus => 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="WoodenShield"/> class.
		/// </summary>
		public WoodenShield() : base(100)
		{
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200031)); /* [You are looking at] [a small wooden shield.]*/

			if (Identified)
				entries.Add(new LocalizationEntry(6250023)); /* The shield appears quite ordinary. */
		}
	}
}
