using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class ScorpionBracelet : Bracelet, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1;

		/// <inheritdoc />
		public override int Weight => 4;

		/// <summary>
		/// Initializes a new instance of the <see cref="ScorpionBracelet"/> class.
		/// </summary>
		public ScorpionBracelet() : base(21)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200079)); /* [You are looking at] [a heavy golden bracelet patterned with a scorpion.] */
		}
	}
}
