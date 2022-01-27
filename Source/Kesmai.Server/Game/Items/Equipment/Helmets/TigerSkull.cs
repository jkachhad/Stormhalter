using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class TigerSkull : Helmet, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000085;

		/// <inheritdoc />
		public override uint BasePrice => 500;

		/// <inheritdoc />
		public override int Weight => 80;

		/// <inheritdoc />
		public override int ProtectionFromStun => 7;
		
		/// <inheritdoc />
		public override int ProtectionFromFire => 0;
		
		/// <inheritdoc />
		public override int ProtectionFromIce => 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="TigerSkull"/> class.
		/// </summary>
		public TigerSkull() : base(36)
		{
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200212)); /* [You are looking at] [a tiger skull.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250107)); /* The skull is fairly thick. */
		}
	}
}