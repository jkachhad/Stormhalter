using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class SwallowSapphire : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="SwallowSapphire"/> class.
		/// </summary>
		[WorldForge]
		public SwallowSapphire(uint basePrice) : base(282, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200289)); /* [You are looking at] [a pale sapphire carved into the shape of a swallow.] */
		}
	}
}