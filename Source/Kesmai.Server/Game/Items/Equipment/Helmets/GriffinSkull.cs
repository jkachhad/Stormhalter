using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class GriffinSkull : Helmet, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000085;

		/// <inheritdoc />
		public override uint BasePrice => 3000;

		/// <inheritdoc />
		public override int Weight => 80;
		
		/// <inheritdoc />
		public override int ProtectionFromStun => 10;
		
		/// <inheritdoc />
		public override int ProtectionFromFire => 15;
		
		/// <inheritdoc />
		public override int ProtectionFromIce => 15;
		
		/// <inheritdoc />
		public override bool ProvidesNightVision => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="GriffinSkull"/> class.
		/// </summary>
		public GriffinSkull() : base(35)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200214)); /* [You are looking at] [a griffin skull.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250105)); /* The skull is very thick and contains the spell of Night Vision. */
		}
	}
}