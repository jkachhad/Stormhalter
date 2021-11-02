using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class WhaleSapphire : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="WhaleSapphire"/> class.
		/// </summary>
		[WorldForge]
		public WhaleSapphire(uint basePrice) : base(176, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200130)); /* [You are looking at] [a dark-blue sapphire shaped like a whale.] */
		}
	}
}