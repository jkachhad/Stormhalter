using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class RamSkull : Helmet, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000085;
		
		/// <inheritdoc />
		public override uint BasePrice => 300;

		/// <inheritdoc />
		public override int Weight => 80;

		/// <inheritdoc />
		public override int ProtectionFromStun => 5;
		
		/// <inheritdoc />
		public override int ProtectionFromFire => 0;
		
		/// <inheritdoc />
		public override int ProtectionFromIce => 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="RamSkull"/> class.
		/// </summary>
		public RamSkull() : base(46)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200213)); /* [You are looking at] [a ram skull.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250108)); /* The skull is rather fragile. */
		}
	}
}