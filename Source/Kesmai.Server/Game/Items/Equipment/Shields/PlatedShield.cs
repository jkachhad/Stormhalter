using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class PlatedShield : Shield
	{
		/// <inheritdoc />
		public override uint BasePrice => 150;

		/// <inheritdoc />
		public override int Weight => 2000;

		/// <inheritdoc />
		public override int Category => 1;
		
		/// <inheritdoc />
		public override int BaseArmorBonus => 1;

		/// <inheritdoc />
		public override int ProjectileProtection => 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="PlatedShield"/> class.
		/// </summary>
		public PlatedShield() : base(76)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200032)); /* [You are looking at] [a wooden shield faced with thin steel plates.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250024)); /* The shield provides fairly good protection. */
		}
	}
}
