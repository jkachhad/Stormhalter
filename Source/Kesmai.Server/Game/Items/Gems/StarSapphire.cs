using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class StarSapphire : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="StarSapphire"/> class.
		/// </summary>
		[WorldForge]
		public StarSapphire(uint basePrice) : base(166, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200128)); /* [You are looking at] [a dazzling diamond.] */
		}
	}
}