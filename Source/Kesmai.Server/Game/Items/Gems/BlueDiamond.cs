using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class BlueDiamond : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="BlueDiamond"/> class.
		/// </summary>
		[WorldForge]
		public BlueDiamond(uint basePrice) : base(335, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200290)); /* [You are looking at] [a rare, expertly cut blue diamond.] */
		}
	}
}