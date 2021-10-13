using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class WeakStrengthBracelet : StrengthBracelet, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 500;

		/// <inheritdoc />
		public override int Weight => 4;
		
		/// <inheritdoc />
		public override int StrengthBonus => 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="WeakStrengthBracelet"/> class.
		/// </summary>
		public WeakStrengthBracelet() : base(134)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200083)); /* [You are looking at] [a leather armband studded with nodules.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250059)); /* The bracelet contains a weak spell of Strength. */
		}
	}
}
