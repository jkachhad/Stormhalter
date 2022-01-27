using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class DragonScaleArmor : Armor, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000076; /* scales */

		/// <inheritdoc />
		public override uint BasePrice => 800;

		/// <inheritdoc />
		public override int Weight => 3000;

		/// <inheritdoc />
		public override int Hindrance => 2;

		/// <inheritdoc />
		public override int SlashingProtection => 3;

		/// <inheritdoc />
		public override int PiercingProtection => 3;

		/// <inheritdoc />
		public override int BashingProtection => 3;

		/// <inheritdoc />
		public override int ProjectileProtection => 3;

		/// <inheritdoc />
		public override int ProtectionFromFire => 5;

		/// <inheritdoc />
		public override int ProtectionFromIce => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="DragonScaleArmor"/> class.
		/// </summary>
		public DragonScaleArmor() : base(243)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200173)); /* [You are looking at] [a vest made of dragon scales.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250096)); /* The armor appears to have some magical properties. */
		}
	}
}
