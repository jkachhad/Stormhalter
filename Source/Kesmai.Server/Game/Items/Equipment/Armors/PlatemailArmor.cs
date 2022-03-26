using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class PlatemailArmor : Armor
	{
		/// <inheritdoc />
		public override uint BasePrice => 100;

		/// <inheritdoc />
		public override int Weight => 4000;

		/// <inheritdoc />
		public override int Hindrance => 2;

		/// <inheritdoc />
		public override int SlashingMitigation => 2;

		/// <inheritdoc />
		public override int PiercingMitigation => 2;

		/// <inheritdoc />
		public override int BashingMitigation => 2;

		/// <inheritdoc />
		public override int ProjectileMitigation => 1;

		/// <inheritdoc />
		public override bool BlockSpellcast => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="PlatemailArmor"/> class.
		/// </summary>
		public PlatemailArmor() : base(241)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200026)); /* [You are looking at] [an iron breastplate and greaves.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250022)); /* The armor appears quite ordinary. */
		}
	}
}
