using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class AlligatorVest : Armor
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000096; /* vest */

		/// <inheritdoc />
		public override uint BasePrice => 30;

		/// <inheritdoc />
		public override int Weight => 1500;

		/// <inheritdoc />
		public override int Hindrance => 1;

		/// <inheritdoc />
		public override int SlashingProtection => 1;

		/// <inheritdoc />
		public override int PiercingProtection => 1;

		/// <inheritdoc />
		public override int BashingProtection => 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="AlligatorVest"/> class.
		/// </summary>
		public AlligatorVest() : base(249)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200175)); /* [You are looking at] [a vest made from the hide of a alligator.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250097)); /* The vest appears quite ordinary. */
		}
	}
}
