using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class StrongStrengthBracelet : StrengthBracelet, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1500;

		/// <inheritdoc />
		public override int Weight => 4;
		
		/// <inheritdoc />
		public override int StrengthBonus => 6;

		/// <summary>
		/// Initializes a new instance of the <see cref="StrongStrengthBracelet"/> class.
		/// </summary>
		public StrongStrengthBracelet() : base(133)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200082)); /* [You are looking at] [a silver bracelet engraved for decoration.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250118)); /* The bracelet contains a powerful spell of Strength. */
		}
	}
}
