using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class CandyCaneOllie : Staff, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000023;

		/// <inheritdoc />
		public override uint BasePrice => 1;

		/// <inheritdoc />
		public override int Weight => 1400;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.VeryHeavy;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 8;

		/// <inheritdoc />
		public override int BaseArmorBonus => 4;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Bashing | WeaponFlags.Lawful;

		/// <inheritdoc />
		public override bool CanBind => true;

		/// <inheritdoc />
		public override int ManaRegeneration => 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="CandyCaneOllie"/> class.
		/// </summary>
		public CandyCaneOllie() : base(322)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200337)); /* [You are looking at] [a candy cane infused with the power of the overlord.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250092)); /* The staff seems to have some magical properties. */
		}
	}
}