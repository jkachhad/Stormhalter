using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class DrakeScaleArmor : Armor, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000076; /* scales */

		/// <inheritdoc />
		public override uint BasePrice => 1600;

		/// <inheritdoc />
		public override int Weight => 1600;

		/// <inheritdoc />
		public override int Hindrance => 2;

		/// <inheritdoc />
		public override int SlashingProtection => 4;

		/// <inheritdoc />
		public override int PiercingProtection => 4;

		/// <inheritdoc />
		public override int BashingProtection => 4;

		/// <inheritdoc />
		public override int ProjectileProtection => 4;

		/// <inheritdoc />
		public override int ProtectionFromFire => 10;

		/// <inheritdoc />
		public override int ProtectionFromIce => 10;

		/// <summary>
		/// Initializes a new instance of the <see cref="DrakeScaleArmor"/> class.
		/// </summary>
		public DrakeScaleArmor() : base(220)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200182)); /* [You are looking at] [a vest made from the silvery scales of a drake.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250096)); /* The armor appears to have some magical properties. */
		}
	}
}
