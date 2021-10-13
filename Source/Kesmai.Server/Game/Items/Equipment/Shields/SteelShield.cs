using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class SteelShield : Shield
	{
		/// <inheritdoc />
		public override uint BasePrice => 300;

		/// <inheritdoc />
		public override int Weight => 3000;

		/// <inheritdoc />
		public override int Category => 1;
		
		/// <inheritdoc />
		public override int BaseArmorBonus => 1;

		/// <inheritdoc />
		public override int ProjectileProtection => 2;

		/// <summary>
		/// Initializes a new instance of the <see cref="SteelShield"/> class.
		/// </summary>
		public SteelShield() : base(188)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200033)); /* [You are looking at] [a steel shield.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250025)); /* The shield will protect you quite well. */
		}
	}
}
