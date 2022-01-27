using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class SteelGauntlets : Gauntlets, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000041;

		/// <inheritdoc />
		public override uint BasePrice => 400;

		/// <inheritdoc />
		public override int Weight => 200;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 4;

		/// <inheritdoc />
		public override int BaseArmorBonus => 1;

		/// <inheritdoc />
		public override int BaseAttackBonus => 2;

		/// <summary>
		/// Initializes a new instance of the <see cref="SteelGauntlets"/> class.
		/// </summary>
		public SteelGauntlets() : base(69)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200170)); /* [You are looking at] [a pair of leather gauntlets with steel plates.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250094)); /* The combat adds for the gauntlets are +2. */
		}
	}
}