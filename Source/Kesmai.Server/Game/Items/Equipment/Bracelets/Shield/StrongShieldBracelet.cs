using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class StrongShieldBracelet : ShieldBracelet, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 2500;

		/// <inheritdoc />
		public override int Weight => 4;
		
		/// <inheritdoc />
		public override int Shield => 6;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="StrongShieldBracelet"/> class.
		/// </summary>
		public StrongShieldBracelet() : base(8)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200074)); /* [You are looking at] [a silver bracelet made of scales.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250054)); /* The bracelet contains a strong spell of Shield. */
		}
	}
}
