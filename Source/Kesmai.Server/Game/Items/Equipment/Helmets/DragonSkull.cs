using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class DragonSkull : Helmet, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000085;

		/// <inheritdoc />
		public override uint BasePrice => 1000;

		/// <inheritdoc />
		public override int Weight => 80;

		/// <inheritdoc />
		public override int ProtectionFromStun => 10;
		
		/// <inheritdoc />
		public override int ProtectionFromFire => 5;
		
		/// <inheritdoc />
		public override int ProtectionFromIce => 0;
		
		/// <inheritdoc />
        public override bool ProvidesNightVision => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="DragonSkull"/> class.
		/// </summary>
		public DragonSkull() : base(47)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200211)); /* [You are looking at] [a dragon skull.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250106)); /* The skull is very thick and seems to have some magical properties. */
		}
	}
}