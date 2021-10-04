using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class BearSkull : Helmet, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000085;

		/// <inheritdoc />
		public override uint BasePrice => 2000;

		/// <inheritdoc />
		public override int Weight => 80;

		/// <inheritdoc />
		public override int ProtectionFromStun => 10;
		
		/// <inheritdoc />
		public override int ProtectionFromFire => 10;
		
		/// <inheritdoc />
		public override int ProtectionFromIce => 10;
		
		/// <inheritdoc />
		public override bool ProvidesNightVision => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="BearSkull"/> class.
		/// </summary>
		public BearSkull() : base(34)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200210)); /* [You are looking at] [a bear skull.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250105)); /* The skull is very thick and contains the spell of Night Vision. */
		}
	}
}